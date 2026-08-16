import { DotNetObject } from "../common";

const REPLAY_MARKER = "auReplay";

interface NavEntryRef {
    key: string;
    id: string;
    index: number;
    url: string | null;
    path: string | null;
    state: unknown;
}

interface NavSnapshot {
    entries: NavEntryRef[];
    currentIndex: number;
}

interface NavigateObservation {
    kind: "push" | "replace" | "reload" | "traverse";
    destinationUrl: string;
    destinationPath: string | null;
    destinationKey: string | null;
    destinationIndex: number;
    canIntercept: boolean;
    cancelable: boolean;
    userInitiated: boolean;
    hashChange: boolean;
    info: Record<string, unknown> | null;
}

interface NavigationRun {
    navigationId: number;
    observation: NavigateObservation;
    snapshot: NavSnapshot;
}

interface NavCommandResult {
    committed: boolean;
    errorName: string | null;
    errorMessage: string | null;
}

/**
 * The gateway to session history.
 *
 * `navigation.entries()` is the source of truth. This class keeps no list of its own, writes nothing
 * to session storage, and never calls `history.pushState` or `history.replaceState`.
 */
export class NavigationLedger {
    private readonly dotNetObject: DotNetObject;
    private readonly abort = new AbortController();
    private active = false;
    private guardArmed = false;
    private nextNavigationId = 1;
    private disposed = false;

    constructor(dotNetObject: DotNetObject) {
        if (typeof window === "undefined" || !("navigation" in window) || !window.navigation) {
            throw new Error(
                "Aurila requires the Navigation API, which this browser does not support. " +
                "Minimum versions: Chrome/Edge 102, Safari 26.2, Firefox 147.");
        }

        this.dotNetObject = dotNetObject;

        const signal = this.abort.signal;

        // Capture on window is the first hop of the propagation path, so this runs before Blazor's
        // own click handler regardless of which was registered first.
        window.addEventListener("click", e => this.onDocumentClick(e), { capture: true, signal });

        // The document can be hidden or discarded without any navigation the app can observe, so
        // give .NET a last chance to write the page's state onto the entry it is sitting on.
        document.addEventListener("visibilitychange", () => {
            if (document.visibilityState === "hidden") {
                this.persistState();
            }
        }, { signal });

        window.addEventListener("pagehide", () => this.persistState(), { signal });

        navigation.addEventListener("navigate", e => this.onNavigate(e), { signal });
        navigation.addEventListener("navigatesuccess", () => this.publishSnapshot(), { signal });
        navigation.addEventListener("navigateerror", () => this.publishSnapshot(), { signal });
        navigation.addEventListener("currententrychange", () => this.publishSnapshot(), { signal });
    }

    public getSnapshot(): NavSnapshot {
        const entries = navigation.entries().map(e => this.toRef(e));
        const currentKey = navigation.currentEntry?.key;
        const currentIndex = currentKey === undefined
            ? -1
            : entries.findIndex(e => e.key === currentKey);

        return { entries, currentIndex };
    }

    public canGoBack(): boolean {
        return navigation.canGoBack;
    }

    public canGoForward(): boolean {
        return navigation.canGoForward;
    }

    public async navigate(
        path: string,
        history: "push" | "replace",
        state: unknown,
        info: unknown): Promise<NavCommandResult> {

        // A replace with no state of its own would reset the entry's state to null, silently
        // discarding whatever the page had persisted there, so carry the existing value across.
        const carried = state === null || state === undefined
            ? (history === "replace" ? navigation.currentEntry?.getState() : undefined)
            : state;

        return this.run(() => navigation.navigate(this.toAbsolute(path), {
            history,
            ...(carried === undefined ? {} : { state: carried }),
            ...(info === null || info === undefined ? {} : { info })
        }));
    }

    public async traverseTo(key: string, info: unknown): Promise<NavCommandResult> {
        return this.run(() => navigation.traverseTo(key, info == null ? undefined : { info }));
    }

    public async reload(info: unknown): Promise<NavCommandResult> {
        return this.run(() => navigation.reload(info == null ? undefined : { info }));
    }

    public async back(info: unknown): Promise<NavCommandResult> {
        if (!navigation.canGoBack) {
            return this.refuse("No entry to go back to.");
        }
        return this.run(() => navigation.back(info == null ? undefined : { info }));
    }

    public async forward(info: unknown): Promise<NavCommandResult> {
        if (!navigation.canGoForward) {
            return this.refuse("No entry to go forward to.");
        }
        return this.run(() => navigation.forward(info == null ? undefined : { info }));
    }

    /**
     * Replaces the current entry's state in place: no new entry, no URL change, no navigate event.
     */
    public updateState(state: unknown): void {
        navigation.updateCurrentEntry({ state });
    }

    public activate(): void {
        this.active = true;
    }

    /**
     * Tells the ledger whether anything in .NET might refuse to leave the current page. Only when
     * armed does a navigation take the slower prevent-then-replay path.
     */
    public setGuardArmed(armed: boolean): void {
        this.guardArmed = armed;
    }

    public dispose(): void {
        if (this.disposed) return;
        this.disposed = true;
        this.abort.abort();
    }

    /**
     * Claims plain left-clicks on in-app anchors.
     *
     * Blazor's own link interception bails out when the event's default is already prevented, so
     * preventing it here is enough to stop it pushing history behind the ledger's back. Modified and
     * non-primary clicks are deliberately left alone so the browser can open them in a new tab.
     */
    private onDocumentClick(event: MouseEvent): void {
        if (!this.active || event.defaultPrevented || event.button !== 0) {
            return;
        }

        if (event.ctrlKey || event.metaKey || event.shiftKey || event.altKey) {
            return;
        }

        const anchor = (event.target as Element | null)?.closest?.("a[href]") as HTMLAnchorElement | null;

        if (!anchor || anchor.hasAttribute("download")) {
            return;
        }

        const target = anchor.getAttribute("target");

        if (target && target !== "_self") {
            return;
        }

        const path = this.toAppPath(anchor.href);

        if (path === null) {
            return;
        }

        if (anchor.getAttribute("aria-disabled") === "true") {
            event.preventDefault();
            return;
        }

        event.preventDefault();

        // Anchors rendered by a clickable already have a .NET click handler that performs the
        // navigation with its own payload and history mode; anything else is navigated here so that
        // a hand-written <a href> works without ceremony.
        if (!anchor.hasAttribute("data-au-link")) {
            // Persist first: once the navigation commits, the entry being left can no longer be
            // written to.
            void this.persistStateThenNavigate(path);
        }
    }

    private onNavigate(event: NavigateEvent): void {
        if (!this.active || !this.owns(event)) {
            return;
        }

        if (this.guardArmed && !this.isReplay(event) && !this.isRebind(event)) {
            if (event.cancelable) {
                event.preventDefault();
                void this.confirmThenReplay(event);
                return;
            }

            console.debug(
                "[aurila] navigation to %s cannot be cancelled by the page; the browser does not " +
                "permit blocking this traversal.",
                event.destination.url);
        }

        const navigationId = this.nextNavigationId++;

        event.signal.addEventListener("abort", () => {
            void this.dotNetObject
                .invokeMethodAsync("OnNavigationAbortedAsync", navigationId)
                .catch(() => { });
        });

        event.intercept({
            handler: async () => {
                await this.dotNetObject.invokeMethodAsync("RunNavigationAsync", {
                    navigationId,
                    observation: this.observe(event),
                    snapshot: this.getSnapshot()
                } satisfies NavigationRun);
            }
        });
    }

    private async confirmThenReplay(event: NavigateEvent): Promise<void> {
        let allowed = false;

        try {
            allowed = await this.dotNetObject
                .invokeMethodAsync<boolean>("ConfirmLeaveAsync", this.observe(event));
        } catch (err) {
            console.error("[aurila] navigation guard failed; allowing the navigation.", err);
            allowed = true;
        }

        if (!allowed || this.disposed) {
            return;
        }

        const original = (typeof event.info === "object" && event.info !== null)
            ? event.info as Record<string, unknown>
            : {};

        const info = { ...original, [REPLAY_MARKER]: true };

        switch (event.navigationType) {
            case "traverse":
                await this.traverseTo(event.destination.key, info);
                return;

            case "reload":
                await this.reload(info);
                return;

            default:
                await this.navigate(
                    this.toAppPath(event.destination.url) ?? event.destination.url,
                    event.navigationType,
                    event.destination.getState(),
                    info);
                return;
        }
    }

    /**
     * A URL change that keeps the page on screen. The page is not being left, so holding it back to
     * ask a guard would mean prompting the user about their own filter change.
     */
    private isRebind(event: NavigateEvent): boolean {
        return typeof event.info === "object"
            && event.info !== null
            && (event.info as Record<string, unknown>)["auRebind"] === true;
    }

    private isReplay(event: NavigateEvent): boolean {
        return typeof event.info === "object"
            && event.info !== null
            && REPLAY_MARKER in (event.info as Record<string, unknown>);
    }

    /**
     * Whether this is a navigation Aurila is able and willing to take over. Everything else is left
     * to the browser, which is what keeps downloads, form posts and external links working.
     */
    private owns(event: NavigateEvent): boolean {
        return event.canIntercept
            && !event.hashChange
            && event.downloadRequest === null
            && event.formData === null
            && this.toAppPath(event.destination.url) !== null;
    }

    private observe(event: NavigateEvent): NavigateObservation {
        const destination = event.destination;

        return {
            kind: event.navigationType,
            destinationUrl: destination.url,
            destinationPath: this.toAppPath(destination.url),
            destinationKey: event.navigationType === "traverse" ? destination.key : null,
            destinationIndex: destination.index,
            canIntercept: event.canIntercept,
            cancelable: event.cancelable,
            userInitiated: event.userInitiated,
            hashChange: event.hashChange,
            info: (event.info ?? null) as Record<string, unknown> | null
        };
    }

    private async persistStateThenNavigate(path: string): Promise<void> {
        try {
            await this.dotNetObject.invokeMethodAsync("PersistStateAsync");
        } catch {
            // A failure to save state must not stop the user navigating.
        }

        await this.navigate(path, "push", undefined, undefined);
    }

    private persistState(): void {
        if (!this.active || this.disposed) return;

        void this.dotNetObject
            .invokeMethodAsync("PersistStateAsync")
            .catch(() => { });
    }

    private publishSnapshot(): void {
        if (this.disposed) return;

        void this.dotNetObject
            .invokeMethodAsync("OnSnapshotChangedAsync", this.getSnapshot())
            .catch(err => console.error("[aurila] ledger: snapshot publish failed", err));
    }

    private async run(command: () => NavigationResult): Promise<NavCommandResult> {
        try {
            const result = command();

            result.finished?.catch(() => { });

            await result.committed;
            return { committed: true, errorName: null, errorMessage: null };
        } catch (err) {
            const e = err as { name?: string; message?: string };
            return {
                committed: false,
                errorName: e?.name ?? "Error",
                errorMessage: e?.message ?? String(err)
            };
        }
    }

    private refuse(message: string): NavCommandResult {
        return { committed: false, errorName: "InvalidStateError", errorMessage: message };
    }

    private toRef(entry: NavigationHistoryEntry): NavEntryRef {
        return {
            key: entry.key,
            id: entry.id,
            index: entry.index,
            url: entry.url,
            path: this.toAppPath(entry.url),
            state: entry.getState() ?? null
        };
    }

    /**
     * Reduces an absolute URL to the app-relative path Aurila routes against, or null when the URL
     * does not belong to this app.
     */
    private toAppPath(url: string | null): string | null {
        if (!url) return null;

        let target: URL;
        try {
            target = new URL(url);
        } catch {
            return null;
        }

        if (target.origin !== location.origin) return null;

        const base = new URL(document.baseURI);
        const basePath = base.pathname.endsWith("/") ? base.pathname : base.pathname + "/";

        if (!(target.pathname + "/").startsWith(basePath)) return null;

        return "/" + target.pathname.slice(basePath.length) + target.search + target.hash;
    }

    private toAbsolute(path: string): string {
        return new URL(path.replace(/^\/+/, ""), document.baseURI).href;
    }
}
