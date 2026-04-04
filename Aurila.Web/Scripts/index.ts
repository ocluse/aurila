interface DotNetBridge {
    invokeMethodAsync(methodName: string, ...args: any[]): Promise<any>;
}

let dotNetBridge: DotNetBridge | null = null;
let isInitialized = false;
let suppressNextPop = false;
let interceptionActive = false;
let hasGuardEntry = false;

const historyMarker = "aurila";

function createState(): Record<string, unknown> {
    const current = window.history.state as Record<string, unknown> | null;
    return {
        ...(current ?? {}),
        __aurila: historyMarker,
        __aurilaToken: Date.now().toString(36) + Math.random().toString(36).slice(2)
    };
}

function onPopState(): void {
    if (suppressNextPop) {
        suppressNextPop = false;
        return;
    }

    if (!dotNetBridge || !interceptionActive) {
        return;
    }

    dotNetBridge.invokeMethodAsync("OnHistoryBackRequested")
        .then((handled: boolean) => {
            if (handled) {
                historyReapplyCurrentEntry();
                return;
            }

            suppressNextPop = true;
            window.history.back();
        })
        .catch(() => {
            suppressNextPop = true;
            window.history.back();
        });
}

export function initializeWebHistoryBridge(dotNetObject: DotNetBridge): void {
    if (isInitialized) {
        return;
    }

    dotNetBridge = dotNetObject;
    isInitialized = true;

    window.addEventListener("popstate", onPopState);
}

export function setHistoryInterceptionActive(active: boolean): void {
    interceptionActive = active;

    if (!interceptionActive) {
        return;
    }

    if (!hasGuardEntry) {
        historyReapplyCurrentEntry();
        hasGuardEntry = true;
    }
}

export function historyReapplyCurrentEntry(): void {
    const state = createState();
    window.history.pushState(state, document.title, window.location.href);
}

export function openLinkInNewTab(url: string): void {
    if (!url || url.trim().length === 0) {
        return;
    }

    window.open(url, "_blank");
}

export function disposeWebHistoryBridge(): void {
    if (!isInitialized) {
        return;
    }

    window.removeEventListener("popstate", onPopState);
    dotNetBridge = null;
    isInitialized = false;
    suppressNextPop = false;
    interceptionActive = false;
    hasGuardEntry = false;
}
