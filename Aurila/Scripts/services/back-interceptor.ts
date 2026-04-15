import { DotNetObject } from "../common";


export class BackInterceptor {
    private dotNetRef: DotNetObject;
    private currentUrl: string | null = null;

    private initialized = false;
    private disposed = false;

    constructor(dotNetRef: DotNetObject) {

        this.dotNetRef = dotNetRef;

        window.addEventListener("popstate", this.onPopState.bind(this));
    }

    initialize() {
        if (this.initialized) return;

        this.initialized = true;

        //place a stopper entry:
        history.pushState({ type: "stopper" }, "");

        //place a control entry:
        history.pushState({ type: "control" }, "");
    }

    cleanup() {
        if (!this.initialized) return;

        this.initialized = false;
        //go back to the stopper entry:
        history.go(-1);

        window.removeEventListener("popstate", this.onPopState.bind(this));
    }

    setWindowLocation(url: string) {
        console.log("Setting window location to:", url);
        this.currentUrl = url;
        history.replaceState({ type: "control" }, "", url);
    }

    getWindowLocation() {
        const baseUrl = window.location.origin;
        return window.location.href.replace(baseUrl, "");
    }

    private async onPopState(event: PopStateEvent) {

        const type = event.state?.type;

        if (type === "stopper") {
            // push a new control entry for URL change:
            history.pushState({ type: "control" }, "");

            if (this.currentUrl === null) {
                history.replaceState({ type: "control" }, "", this.currentUrl);
            }
        }

        const handled = await this.dotNetRef.invokeMethodAsync("HandlePopStateAsync", event.state);

        if (!handled) {
            //go back two steps to bypass the stopper and control entries:
            history.go(-2);
        }
    }
}