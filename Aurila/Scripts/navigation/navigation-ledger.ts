import { DotNetObject } from "../common";
import { HistoryApiLedger, hasHistoryLedgerState } from "./history-api-ledger";
import { NavigationApiLedger } from "./navigation-api-ledger";
import { NavigationLedgerBackend } from "./navigation-ledger-contract";

/**
 * Creates the best session-history backend available in this browser.
 *
 * A restored History API entry stays on the fallback even if the Navigation API has since become
 * available: the two APIs expose separate state values, so switching midway through a tab would
 * discard the managed entry keys and saved page state.
 */
export function createNavigationLedger(dotNetObject: DotNetObject): NavigationLedgerBackend {
    return supportsRequiredNavigationApi() && !hasHistoryLedgerState()
        ? new NavigationApiLedger(dotNetObject)
        : new HistoryApiLedger(dotNetObject);
}

function supportsRequiredNavigationApi(): boolean {
    if (typeof window === "undefined") return false;

    const candidate = (window as Window & { navigation?: Partial<Navigation> }).navigation;
    const navigateEvent = (globalThis as typeof globalThis & {
        NavigateEvent?: { prototype: { intercept?: unknown } };
    }).NavigateEvent;

    return candidate !== undefined
        && typeof candidate.entries === "function"
        && typeof candidate.navigate === "function"
        && typeof candidate.traverseTo === "function"
        && typeof candidate.back === "function"
        && typeof candidate.forward === "function"
        && typeof candidate.reload === "function"
        && typeof candidate.updateCurrentEntry === "function"
        && typeof candidate.addEventListener === "function"
        && typeof candidate.removeEventListener === "function"
        && typeof navigateEvent === "function"
        && typeof navigateEvent.prototype.intercept === "function";
}
