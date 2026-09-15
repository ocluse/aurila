import { DotNetObject } from "../common";
import { NavigationLedgerBase, REPLAY_MARKER } from "./navigation-ledger-base";
import {
    NavCommandResult,
    NavEntryRef,
    NavigateObservation,
    NavigationRun
} from "./navigation-ledger-contract";

/** Navigation API implementation. The browser's NavigationHistoryEntry list is authoritative. */
export class NavigationApiLedger extends NavigationLedgerBase {
    private readonly navigateHandler = (event: NavigateEvent) => this.onNavigate(event);
    private readonly snapshotHandler = () => this.publishSnapshot();

    constructor(dotNetObject: DotNetObject) {
        super(dotNetObject);

        navigation.addEventListener("navigate", this.navigateHandler);
        navigation.addEventListener("navigatesuccess", this.snapshotHandler);
        navigation.addEventListener("navigateerror", this.snapshotHandler);
        navigation.addEventListener("currententrychange", this.snapshotHandler);
    }

    public getSnapshot() {
        const entries = navigation.entries().map(entry => this.toRef(entry));
        const currentKey = navigation.currentEntry?.key;
        const currentIndex = currentKey === undefined
            ? -1
            : entries.findIndex(entry => entry.key === currentKey);

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

    public updateState(state: unknown): void {
        navigation.updateCurrentEntry({ state });
    }

    protected override onDispose(): void {
        navigation.removeEventListener("navigate", this.navigateHandler);
        navigation.removeEventListener("navigatesuccess", this.snapshotHandler);
        navigation.removeEventListener("navigateerror", this.snapshotHandler);
        navigation.removeEventListener("currententrychange", this.snapshotHandler);
    }

    private onNavigate(event: NavigateEvent): void {
        if (!this.active || !this.owns(event)) {
            return;
        }

        if (this.guardArmed && !this.isReplay(event.info) && !this.isRebind(event.info)) {
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

        const original = this.toInfo(event.info) ?? {};
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
            info: this.toInfo(event.info)
        };
    }

    private async run(command: () => NavigationResult): Promise<NavCommandResult> {
        try {
            const result = command();

            result.finished?.catch(() => { });

            await result.committed;
            return { committed: true, errorName: null, errorMessage: null };
        } catch (err) {
            const error = err as { name?: string; message?: string };
            return {
                committed: false,
                errorName: error?.name ?? "Error",
                errorMessage: error?.message ?? String(err)
            };
        }
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
}
