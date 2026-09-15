import { DotNetObject } from "../common";
import { NavCommandResult, NavSnapshot, NavigationLedgerBackend } from "./navigation-ledger-contract";

export const REPLAY_MARKER = "auReplay";
export const REBIND_MARKER = "auRebind";

/** Browser-independent lifecycle, link interception, and .NET notifications for both ledgers. */
export abstract class NavigationLedgerBase implements NavigationLedgerBackend {
    protected active = false;
    protected guardArmed = false;
    protected nextNavigationId = 1;
    protected disposed = false;

    private readonly documentClickHandler = (event: MouseEvent) => this.onDocumentClick(event);
    private readonly visibilityChangeHandler = () => {
        if (document.visibilityState === "hidden") {
            this.persistState();
        }
    };
    private readonly pageHideHandler = () => this.persistState();

    constructor(protected readonly dotNetObject: DotNetObject) {
        // Capture on window is the first hop of the propagation path, so this runs before Blazor's
        // own click handler regardless of which was registered first.
        window.addEventListener("click", this.documentClickHandler, true);

        // The document can be hidden or discarded without any navigation the app can observe, so
        // give .NET a last chance to write the page's state onto the entry it is sitting on.
        document.addEventListener("visibilitychange", this.visibilityChangeHandler);
        window.addEventListener("pagehide", this.pageHideHandler);
    }

    public abstract getSnapshot(): NavSnapshot;
    public abstract canGoBack(): boolean;
    public abstract canGoForward(): boolean;
    public abstract navigate(path: string, history: "push" | "replace", state: unknown, info: unknown): Promise<NavCommandResult>;
    public abstract traverseTo(key: string, info: unknown): Promise<NavCommandResult>;
    public abstract reload(info: unknown): Promise<NavCommandResult>;
    public abstract back(info: unknown): Promise<NavCommandResult>;
    public abstract forward(info: unknown): Promise<NavCommandResult>;
    public abstract updateState(state: unknown): void;

    public activate(): void {
        this.active = true;
    }

    public setGuardArmed(armed: boolean): void {
        this.guardArmed = armed;
    }

    public dispose(): void {
        if (this.disposed) return;
        this.disposed = true;
        window.removeEventListener("click", this.documentClickHandler, true);
        document.removeEventListener("visibilitychange", this.visibilityChangeHandler);
        window.removeEventListener("pagehide", this.pageHideHandler);
        this.onDispose();
    }

    protected onDispose(): void { }

    protected async persistStateThenNavigate(path: string): Promise<void> {
        try {
            await this.dotNetObject.invokeMethodAsync("PersistStateAsync");
        } catch {
            // A failure to save state must not stop the user navigating.
        }

        await this.navigate(path, "push", undefined, undefined);
    }

    protected persistState(): void {
        if (!this.active || this.disposed) return;

        void this.dotNetObject
            .invokeMethodAsync("PersistStateAsync")
            .catch(() => { });
    }

    protected publishSnapshot(): void {
        if (this.disposed) return;

        void this.dotNetObject
            .invokeMethodAsync("OnSnapshotChangedAsync", this.getSnapshot())
            .catch(err => console.error("[aurila] ledger: snapshot publish failed", err));
    }

    protected refuse(message: string, errorName = "InvalidStateError"): NavCommandResult {
        return { committed: false, errorName, errorMessage: message };
    }

    protected toAppPath(url: string | null): string | null {
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

    protected toAbsolute(path: string): string {
        return new URL(path.replace(/^\/+/, ""), document.baseURI).href;
    }

    protected toInfo(info: unknown): Record<string, unknown> | null {
        return typeof info === "object" && info !== null
            ? info as Record<string, unknown>
            : null;
    }

    protected isReplay(info: unknown): boolean {
        const value = this.toInfo(info);
        return value !== null && REPLAY_MARKER in value;
    }

    protected isRebind(info: unknown): boolean {
        return this.toInfo(info)?.[REBIND_MARKER] === true;
    }

    /** Claims ordinary left-clicks on anchors that belong to this app. */
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

        // A clickable's .NET handler performs its own navigation with its payload and history mode.
        // Hand-written anchors come through here after the current page state has been persisted.
        if (!anchor.hasAttribute("data-au-link")) {
            void this.persistStateThenNavigate(path);
        }
    }
}
