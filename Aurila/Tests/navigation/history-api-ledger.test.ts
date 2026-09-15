import assert from "node:assert/strict";
import { afterEach, test } from "node:test";
import { DotNetObject } from "../../Scripts/common";
import { HistoryApiLedger } from "../../Scripts/navigation/history-api-ledger";
import { NavigationApiLedger } from "../../Scripts/navigation/navigation-api-ledger";
import { createNavigationLedger } from "../../Scripts/navigation/navigation-ledger";

class FakeEventTarget {
    private readonly listeners = new Map<string, Set<(event: any) => void>>();

    public addEventListener(type: string, listener: (event: any) => void, options?: AddEventListenerOptions): void {
        const listeners = this.listeners.get(type) ?? new Set();
        listeners.add(listener);
        this.listeners.set(type, listeners);

        options?.signal?.addEventListener("abort", () => listeners.delete(listener), { once: true });
    }

    public removeEventListener(type: string, listener: (event: any) => void): void {
        this.listeners.get(type)?.delete(listener);
    }

    public dispatch(type: string, event: any): void {
        for (const listener of this.listeners.get(type) ?? []) {
            listener(event);
        }
    }
}

class FakeStorage {
    private readonly values = new Map<string, string>();

    public getItem(key: string): string | null {
        return this.values.get(key) ?? null;
    }

    public setItem(key: string, value: string): void {
        this.values.set(key, value);
    }
}

class FakeDotNet implements DotNetObject {
    public readonly calls: { method: string; args: any[] }[] = [];
    public allowLeave = true;

    public invokeMethodAsync<T>(method: string, ...args: any[]): Promise<T> {
        this.calls.push({ method, args });
        const value = method === "ConfirmLeaveAsync" ? this.allowLeave : undefined;
        return Promise.resolve(value as T);
    }
}

interface FakeLocation {
    href: string;
    origin: string;
    reloadCount: number;
    reload(): void;
}

interface FakeHistoryEntry {
    url: string;
    state: unknown;
}

class FakeHistory {
    private currentIndex = 0;
    private readonly entries: FakeHistoryEntry[];

    constructor(
        initialUrl: string,
        initialState: unknown,
        private readonly location: FakeLocation,
        private readonly events: FakeEventTarget) {

        this.entries = [{ url: initialUrl, state: initialState }];
    }

    public get state(): unknown {
        return this.entries[this.currentIndex].state;
    }

    public get length(): number {
        return this.entries.length;
    }

    public pushState(state: unknown, _unused: string, url?: string | URL | null): void {
        const href = this.resolve(url);
        this.entries.splice(this.currentIndex + 1);
        this.entries.push({ state, url: href });
        this.currentIndex++;
        this.setLocation(href);
    }

    public replaceState(state: unknown, _unused: string, url?: string | URL | null): void {
        const href = this.resolve(url);
        this.entries[this.currentIndex] = { state, url: href };
        this.setLocation(href);
    }

    public go(delta: number): void {
        const destination = this.currentIndex + delta;
        if (destination < 0 || destination >= this.entries.length) return;

        this.currentIndex = destination;
        const entry = this.entries[this.currentIndex];
        this.setLocation(entry.url);
        this.events.dispatch("popstate", { state: entry.state });
    }

    private resolve(url?: string | URL | null): string {
        return url == null ? this.location.href : new URL(url, this.location.href).href;
    }

    private setLocation(href: string): void {
        this.location.href = href;
        this.location.origin = new URL(href).origin;
    }
}

interface BrowserFixture {
    history: FakeHistory;
    storage: FakeStorage;
}

let activeLedger: HistoryApiLedger | null = null;

afterEach(() => {
    activeLedger?.dispose();
    activeLedger = null;
});

test("adopts the current entry without hiding object-shaped host state", () => {
    const { history } = installBrowser({ blazor: "retained" });
    activeLedger = new HistoryApiLedger(new FakeDotNet());

    assert.equal((history.state as any).blazor, "retained");
    assert.equal(typeof (history.state as any).__aurilaNavigation.key, "string");
    assert.deepEqual(activeLedger.getSnapshot().entries.map(entry => entry.path), ["/"]);
});

test("the public factory selects the fallback when the Navigation API is absent", () => {
    installBrowser(null);
    delete (globalThis as any).NavigateEvent;

    const backend = createNavigationLedger(new FakeDotNet());

    assert.ok(backend instanceof HistoryApiLedger);
    activeLedger = backend;
});

test("the public factory retains the native backend when its full contract is available", () => {
    installBrowser(null);
    const navigation = completeNavigationApiStub();
    (window as any).navigation = navigation;
    (globalThis as any).navigation = navigation;
    (globalThis as any).NavigateEvent = function NavigateEvent() { };
    (globalThis as any).NavigateEvent.prototype.intercept = () => { };

    const backend = createNavigationLedger(new FakeDotNet());

    assert.ok(backend instanceof NavigationApiLedger);
    backend.dispose();
    delete (globalThis as any).navigation;
    delete (globalThis as any).NavigateEvent;
});

test("push, replace, traversal, and forward truncation preserve ledger semantics", async () => {
    installBrowser(null);
    const dotNet = new FakeDotNet();
    activeLedger = new HistoryApiLedger(dotNet);
    activeLedger.activate();

    await activeLedger.navigate("/one", "push", { value: 1 }, null);
    await activeLedger.navigate("/two", "push", { value: 2 }, null);

    const beforeReplace = activeLedger.getSnapshot().entries[2];
    await activeLedger.navigate("/two?edited=true", "replace", null, { auRebind: true });
    const afterReplace = activeLedger.getSnapshot().entries[2];

    assert.equal(afterReplace.key, beforeReplace.key);
    assert.notEqual(afterReplace.id, beforeReplace.id);
    assert.deepEqual(afterReplace.state, { value: 2 });

    const one = activeLedger.getSnapshot().entries[1];
    assert.equal((await activeLedger.traverseTo(one.key, { source: "test" })).committed, true);
    assert.equal(activeLedger.getSnapshot().currentIndex, 1);

    await activeLedger.navigate("/three", "push", null, null);
    assert.deepEqual(activeLedger.getSnapshot().entries.map(entry => entry.path), ["/", "/one", "/three"]);
    assert.equal(activeLedger.canGoForward(), false);
});

test("restores stable entry keys and state after a reload", async () => {
    installBrowser(null);
    const first = new HistoryApiLedger(new FakeDotNet());
    activeLedger = first;

    await first.navigate("/one", "push", null, null);
    first.updateState({ draft: "saved" });
    const beforeReload = first.getSnapshot();
    first.dispose();

    const restored = new HistoryApiLedger(new FakeDotNet());
    activeLedger = restored;
    const afterReload = restored.getSnapshot();

    assert.deepEqual(afterReload, beforeReload);
});

test("a guard can reject framework-initiated navigation before history changes", async () => {
    const { history } = installBrowser(null);
    const dotNet = new FakeDotNet();
    dotNet.allowLeave = false;
    activeLedger = new HistoryApiLedger(dotNet);
    activeLedger.activate();
    activeLedger.setGuardArmed(true);

    const result = await activeLedger.navigate("/blocked", "push", null, null);

    assert.equal(result.committed, false);
    assert.equal(result.errorName, "AbortError");
    assert.equal(history.length, 1);
    assert.equal(activeLedger.getSnapshot().entries.length, 1);
    assert.equal(dotNet.calls.filter(call => call.method === "ConfirmLeaveAsync").length, 1);
});

test("browser-initiated back is reported as a non-cancelable user traversal", async () => {
    const { history } = installBrowser(null);
    const dotNet = new FakeDotNet();
    activeLedger = new HistoryApiLedger(dotNet);
    activeLedger.activate();

    await activeLedger.navigate("/one", "push", null, null);
    await activeLedger.navigate("/two", "push", null, null);
    await drainTasks();
    dotNet.calls.length = 0;

    history.go(-1);
    await drainTasks();

    const run = dotNet.calls.find(call => call.method === "RunNavigationAsync")?.args[0];
    assert.equal(run.observation.kind, "traverse");
    assert.equal(run.observation.destinationPath, "/one");
    assert.equal(run.observation.cancelable, false);
    assert.equal(run.observation.userInitiated, true);
});

function installBrowser(initialState: unknown): BrowserFixture {
    const href = "https://example.test/app/";
    const events = new FakeEventTarget();
    const location: FakeLocation = {
        href,
        origin: new URL(href).origin,
        reloadCount: 0,
        reload() { this.reloadCount++; }
    };
    const history = new FakeHistory(href, initialState, location, events);
    const storage = new FakeStorage();
    const documentEvents = new FakeEventTarget();

    const windowObject = Object.assign(events, {
        history,
        setTimeout,
        clearTimeout
    });
    const documentObject = Object.assign(documentEvents, {
        baseURI: href,
        visibilityState: "visible",
        getElementById: () => null
    });

    Object.assign(globalThis, {
        window: windowObject,
        document: documentObject,
        location,
        sessionStorage: storage
    });

    return { history, storage };
}

async function drainTasks(): Promise<void> {
    await new Promise<void>(resolve => setImmediate(resolve));
}

function completeNavigationApiStub(): Record<string, any> {
    const result = {
        committed: Promise.resolve(),
        finished: Promise.resolve()
    };

    return {
        entries: () => [],
        navigate: () => result,
        traverseTo: () => result,
        back: () => result,
        forward: () => result,
        reload: () => result,
        updateCurrentEntry: () => { },
        addEventListener: () => { },
        removeEventListener: () => { },
        canGoBack: false,
        canGoForward: false,
        currentEntry: null
    };
}
