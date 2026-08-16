import { DotNetObject } from "../common";

/**
 * Routes platform close requests to .NET.
 *
 * A close request is whatever the device uses to dismiss a transient surface: `Esc` on a keyboard,
 * the back button or the back gesture on Android. `CloseWatcher` handles all of them, and crucially
 * does so without touching session history — which is what lets Aurila drop the sentinel entries
 * that previously existed only so the back button could close a dialog.
 *
 * `CloseWatcher` is not Baseline, so where it is missing this falls back to `Esc` alone.
 */
export class CloseRequestWatcher {
    private readonly dotNetObject: DotNetObject;
    private readonly abort = new AbortController();
    private watcher: CloseWatcher | null = null;
    private disposed = false;

    constructor(dotNetObject: DotNetObject) {
        this.dotNetObject = dotNetObject;

        if (typeof CloseWatcher === "function") {
            this.watcher = new CloseWatcher();
            this.watcher.addEventListener("close", () => this.requestClose(), { signal: this.abort.signal });
        } else {
            document.addEventListener("keydown", event => {
                if (event.key === "Escape" && !event.defaultPrevented) {
                    event.preventDefault();
                    this.requestClose();
                }
            }, { signal: this.abort.signal });
        }
    }

    public get supportsCloseRequests(): boolean {
        return this.watcher !== null;
    }

    public dispose(): void {
        if (this.disposed) return;
        this.disposed = true;

        this.abort.abort();

        if (this.watcher) {
            this.watcher.destroy();
            this.watcher = null;
        }
    }

    private requestClose(): void {
        if (this.disposed) return;

        void this.dotNetObject
            .invokeMethodAsync("OnCloseRequestedAsync")
            .catch(err => console.error("[aurila] close request failed", err));
    }
}
