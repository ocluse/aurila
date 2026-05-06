import { DotNetObject } from "./common";

interface NavEntry {
    id: string;
    url: string;
    serializedState: string | undefined;
}

interface RouteInfo {
    url: string,
    serializedState?: string;
}

enum NavigationType {
    Push = 0,
    Pop = 1,
    Replace = 2,
    UpdateUrl = 3
}

enum PopStatHandlingResult {
    Navigating = 0,
    Handled = 1,
    NotHandled = 2
}

type PendingOperation =
    | { type: 'replace'; id: string; url: string }
    | { type: 'pop'; id: string; url: string };

export class AurilaApp {

    private layerCount: number = 0;
    private navStack: NavEntry[];
    private dotNetObject: DotNetObject;
    private pendingOperation: PendingOperation | null = null;

    private popStateQueue: PopStateEvent[] = [];
    private isProcessingPopState: boolean = false;

    private readonly boundPopStateHandler: (event: PopStateEvent) => void;

    constructor(dotNetObject: DotNetObject) {

        this.dotNetObject = dotNetObject;
        this.boundPopStateHandler = this.onPopState.bind(this);
        window.addEventListener("popstate", this.boundPopStateHandler);

        const navStackJson = sessionStorage.getItem("navStack");

        if (navStackJson) {
            try {
                const parsed: unknown = JSON.parse(navStackJson);

                if (Array.isArray(parsed) && parsed.every(isValidNavEntry)) {
                    this.navStack = parsed;

                    const currentState = history.state;

                    if (!currentState.sentinel) {
                        this.pushSentinel();
                    }

                } else {
                    console.warn("navStack in session storage has invalid shape; resetting");
                    this.navStack = [];
                    history.pushState({ rootSentinel: {} }, "", null);
                }


            } catch (e) {
                console.error("Failed to parse navStack from sessionStorage:", e);
                this.navStack = [];
                history.pushState({ rootSentinel: {} }, "", null);
            }
        } else {
            //clean stack, push root sentinel:
            this.navStack = [];
            history.pushState({ rootSentinel: {} }, "", null);
        }
    }

    public updateLayerCount(count: number) {
        this.layerCount = count;

        //ensure sentinel if we have at least one layer:
        if (this.layerCount >= 1 && !this.hasSentinel()) {
            console.debug("Pushing sentinel because we have at least one layer and no sentinel was found.");
            this.pushSentinel();
        }
    }

    public getCurrentLocation(): string {
        return window.location.pathname + window.location.search + window.location.hash;
    }

    public getNavStack(): NavEntry[] {
        return this.navStack;
    }

    public completeNavigation(routeInfo: RouteInfo, navigationType: NavigationType) {

        console.log("Completing navigation. Route info:", routeInfo, "Navigation type:", NavigationType[navigationType]);
        if (navigationType === NavigationType.Push) {
            const entryId = crypto.randomUUID();

            //replace the sentinel:
            history.replaceState({ id: entryId, navEntry: {} }, "", routeInfo.url);
            this.pushSentinel();

            this.navStack.push({ id: entryId, url: routeInfo.url, serializedState: routeInfo.serializedState });
        }
        else if (navigationType === NavigationType.Replace) {
            const navEntryToReplace = this.navStack.length > 0
                ? this.navStack[this.navStack.length - 1]
                : null;

            if (!navEntryToReplace) {
                throw new Error("Invalid replace operation attempted.");
            }

            navEntryToReplace.url = routeInfo.url;
            navEntryToReplace.serializedState = routeInfo.serializedState;

            this.pendingOperation = { type: 'replace', id: navEntryToReplace.id, url: routeInfo.url };

            this.ensureSentinel();
            history.back();

        } else if (navigationType === NavigationType.UpdateUrl) {
            const navEntryToUpdate = this.navStack.length > 0
                ? this.navStack[this.navStack.length - 1]
                : null;

            if (!navEntryToUpdate) {
                throw new Error("Invalid UpdateUrl operation attempted.");
            }

            navEntryToUpdate.url = routeInfo.url;
            if (routeInfo.serializedState !== undefined) {
                navEntryToUpdate.serializedState = routeInfo.serializedState;
            }

            // Persist to session storage so refreshes retain the URL
            sessionStorage.setItem("navStack", JSON.stringify(this.navStack));

            // Update the address bar immediately. 
            // We are currently on the sentinel, so we just replace its URL.
            history.replaceState(history.state, "", routeInfo.url);
        } else if (navigationType === NavigationType.Pop) {
            if (this.navStack.length === 0) {
                throw new Error("Invalid pop operation attempted: navStack is empty.");
            }

            this.navStack.pop();

            const destination = this.navStack.length > 0
                ? this.navStack[this.navStack.length - 1]
                : null;

            if (destination) {
                destination.serializedState = routeInfo.serializedState;
            }

            this.pendingOperation = {
                type: 'pop',
                id: destination?.id ?? '',
                url: destination?.url ?? routeInfo.url
            };

            history.go(-2);
        }

        sessionStorage.setItem("navStack", JSON.stringify(this.navStack));
    }

    private onPopState(event: PopStateEvent) {

        this.popStateQueue.push(event);

        if (!this.isProcessingPopState) {
            void this.drainPopStateQueue();
        }
    }

    private async drainPopStateQueue(): Promise<void> {
        this.isProcessingPopState = true;

        try {
            while (this.popStateQueue.length > 0) {
                const event = this.popStateQueue.shift()!;
                await this.handlePopState(event)
            }
        } finally {
            this.isProcessingPopState = false;
        }
    }

    private async handlePopState(event: PopStateEvent): Promise<void> {
        if (this.pendingOperation?.type === 'replace') {
            const op = this.pendingOperation;
            this.pendingOperation = null;

            history.replaceState({ id: op.id, navEntry: {} }, "", op.url);
            this.pushSentinel();
            return;
        }

        if (this.pendingOperation?.type === 'pop') {
            const op = this.pendingOperation;
            this.pendingOperation = null;

            if (op.id) {
                history.replaceState({ id: op.id, navEntry: {} }, "", op.url);
                this.pushSentinel();
            }

            sessionStorage.setItem("navStack", JSON.stringify(this.navStack));
            return;
        }

        if (this.layerCount >= 1) {
            const result = await this.dotNetObject.invokeMethodAsync<PopStatHandlingResult>("HandlePopStateAsync");

            if (result === PopStatHandlingResult.Handled) {
                this.pushSentinel();
            }
            // PopStatHandlingResult.Navigating: .NET will call completeNavigation,
            // no sentinel needed here — it will be pushed there.
            // PopStatHandlingResult.NotHandled: let the browser continue going back.
        } else {
            history.back();
        }
    }

    private pushSentinel() {
        history.pushState({ sentinel: {} }, "", null);
    }

    private ensureSentinel() {
        if (!this.hasSentinel()) {
            throw new Error("Expected to find a sentinel state, but did not. This should never happen.");
        }
    }

    private hasSentinel(): boolean {
        const currentState = history.state;
        return !!currentState && !!currentState.sentinel;
    }

    public dispose() {
        window.removeEventListener("popstate", this.boundPopStateHandler);
    }
}

function isValidNavEntry(entry: unknown): entry is NavEntry {
    if (typeof entry !== 'object' || entry === null) return false;
    const e = entry as Record<string, unknown>;
    return typeof e['id'] === 'string' && typeof e['url'] === 'string';
}