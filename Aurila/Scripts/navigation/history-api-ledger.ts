import { DotNetObject } from "../common";
import { NavigationLedgerBase } from "./navigation-ledger-base";
import {
    NavCommandResult,
    NavEntryRef,
    NavigateObservation,
    NavigationRun
} from "./navigation-ledger-contract";

const STATE_PROPERTY = "__aurilaNavigation";
const FOREIGN_STATE_PROPERTY = "__aurilaForeignState";
const STORAGE_PREFIX = "aurila.navigation.";
const FORMAT_VERSION = 1;
const TRAVERSAL_TIMEOUT_MS = 5000;

interface HistoryMarker {
    version: number;
    ledgerId: string;
    key: string;
    id: string;
    index: number;
    state: unknown;
}

type HistoryStateEnvelope = Record<string, unknown> & {
    [STATE_PROPERTY]: HistoryMarker;
};

interface StoredLedger {
    version: number;
    ledgerId: string;
    currentKey: string;
    entries: NavEntryRef[];
}

interface PendingTraversal {
    targetKey: string;
    info: Record<string, unknown> | null;
    resolve: (result: NavCommandResult) => void;
    timeout: number;
}

/** Whether the current classic history entry belongs to Aurila's fallback ledger. */
export function hasHistoryLedgerState(): boolean {
    return readMarker(window.history.state) !== null;
}

/**
 * History API implementation used where the Navigation API is unavailable.
 *
 * The classic API cannot enumerate the browser's full session history. This backend therefore
 * maintains a versioned, tab-scoped projection of the contiguous entries Aurila creates. Each real
 * history entry carries a stable key and ledger id; the projection is mirrored in sessionStorage so
 * it survives reloads without inserting sentinel entries into the user's back stack.
 */
export class HistoryApiLedger extends NavigationLedgerBase {
    private ledgerId = "";
    private entries: NavEntryRef[] = [];
    private currentIndex = -1;
    private pendingTraversal: PendingTraversal | null = null;
    private activeNavigationId: number | null = null;
    private readonly popStateHandler = (event: PopStateEvent) => this.onPopState(event);

    constructor(dotNetObject: DotNetObject) {
        super(dotNetObject);
        this.restoreOrAdoptCurrentEntry();

        window.addEventListener("popstate", this.popStateHandler);
    }

    public getSnapshot() {
        return {
            entries: this.entries.map((entry, index) => ({ ...entry, index })),
            currentIndex: this.currentIndex
        };
    }

    public canGoBack(): boolean {
        return this.currentIndex > 0;
    }

    public canGoForward(): boolean {
        return this.currentIndex >= 0 && this.currentIndex < this.entries.length - 1;
    }

    public async navigate(
        path: string,
        historyMode: "push" | "replace",
        state: unknown,
        info: unknown): Promise<NavCommandResult> {

        const destinationUrl = this.toAbsolute(path);
        const destinationPath = this.toAppPath(destinationUrl);

        if (destinationPath === null) {
            return this.refuse("The History API fallback can only navigate within the Aurila app.", "SecurityError");
        }

        const observation = this.observation(
            historyMode,
            destinationUrl,
            destinationPath,
            null,
            -1,
            true,
            false,
            this.isHashChange(this.currentUrl(), destinationUrl),
            info);
        const currentKey = this.currentEntry()?.key;

        if (!await this.confirmIfNeeded(observation)) {
            return this.refuse("Navigation was blocked by a page guard.", "AbortError");
        }

        if (this.disposed || currentKey !== this.currentEntry()?.key) {
            return this.refuse("A newer navigation superseded this request.", "AbortError");
        }

        try {
            if (historyMode === "replace") {
                this.replace(destinationUrl, destinationPath, state);
            } else {
                this.push(destinationUrl, destinationPath, state);
            }
        } catch (err) {
            return this.fromError(err);
        }

        this.save();
        this.publishSnapshot();

        if (observation.hashChange) {
            this.scrollToFragment(destinationUrl);
        } else if (this.active) {
            this.dispatch(observation);
        }

        return this.success();
    }

    public async traverseTo(key: string, info: unknown): Promise<NavCommandResult> {
        const destinationIndex = this.entries.findIndex(entry => entry.key === key);

        if (destinationIndex < 0) {
            return this.refuse("The requested history entry is not in Aurila's managed history segment.");
        }

        if (destinationIndex === this.currentIndex) {
            return this.success();
        }

        if (this.pendingTraversal !== null) {
            this.finishPending(this.refuse("A newer traversal superseded this request.", "AbortError"));
        }

        const destination = this.entries[destinationIndex];
        const observation = this.observation(
            "traverse",
            destination.url ?? location.href,
            destination.path,
            destination.key,
            destinationIndex,
            true,
            false,
            this.isHashChange(this.currentUrl(), destination.url),
            info);
        const currentKey = this.currentEntry()?.key;

        if (!await this.confirmIfNeeded(observation)) {
            return this.refuse("Navigation was blocked by a page guard.", "AbortError");
        }

        if (this.disposed || currentKey !== this.currentEntry()?.key) {
            return this.refuse("A newer navigation superseded this request.", "AbortError");
        }

        return new Promise<NavCommandResult>(resolve => {
            const timeout = window.setTimeout(() => {
                if (this.pendingTraversal?.targetKey !== key) return;
                this.finishPending(this.refuse(
                    "The browser did not complete the requested history traversal."));
            }, TRAVERSAL_TIMEOUT_MS);

            this.pendingTraversal = {
                targetKey: key,
                info: this.toInfo(info),
                resolve,
                timeout
            };

            window.history.go(destinationIndex - this.currentIndex);
        });
    }

    public async back(info: unknown): Promise<NavCommandResult> {
        if (!this.canGoBack()) {
            return this.refuse("No managed entry to go back to.");
        }
        return this.traverseTo(this.entries[this.currentIndex - 1].key, info);
    }

    public async forward(info: unknown): Promise<NavCommandResult> {
        if (!this.canGoForward()) {
            return this.refuse("No managed entry to go forward to.");
        }
        return this.traverseTo(this.entries[this.currentIndex + 1].key, info);
    }

    public async reload(info: unknown): Promise<NavCommandResult> {
        const current = this.currentEntry();

        if (current === null) {
            return this.refuse("There is no current entry to reload.");
        }

        const observation = this.observation(
            "reload",
            current.url ?? location.href,
            current.path,
            null,
            this.currentIndex,
            true,
            false,
            false,
            info);

        if (!await this.confirmIfNeeded(observation)) {
            return this.refuse("Navigation was blocked by a page guard.", "AbortError");
        }

        if (!this.disposed && this.active) {
            this.dispatch(observation);
        }

        return this.disposed
            ? this.refuse("The navigation ledger was disposed.", "AbortError")
            : this.success();
    }

    public updateState(state: unknown): void {
        const current = this.currentEntry();
        if (current === null) return;

        const updated = { ...current, state };
        const envelope = this.envelope(updated, this.foreignState(window.history.state));

        // Let DataCloneError/SecurityError surface to .NET just as Navigation.updateCurrentEntry does.
        window.history.replaceState(envelope, "", updated.url ?? location.href);
        this.entries[this.currentIndex] = updated;
        this.save();
        this.publishSnapshot();
    }

    protected override onDispose(): void {
        window.removeEventListener("popstate", this.popStateHandler);

        if (this.pendingTraversal !== null) {
            this.finishPending(this.refuse("The navigation ledger was disposed.", "AbortError"));
        }

        if (this.activeNavigationId !== null) {
            void this.dotNetObject
                .invokeMethodAsync("OnNavigationAbortedAsync", this.activeNavigationId)
                .catch(() => { });
            this.activeNavigationId = null;
        }
    }

    private restoreOrAdoptCurrentEntry(): void {
        const marker = readMarker(window.history.state);

        if (marker !== null && this.restore(marker)) {
            return;
        }

        this.ledgerId = marker?.ledgerId ?? this.newToken("ledger");
        const entry = this.entry(
            location.href,
            marker?.state ?? null,
            marker?.key,
            marker?.id);

        this.entries = [entry];
        this.currentIndex = 0;
        this.writeCurrent(entry, this.foreignState(window.history.state));
        this.save();
    }

    private restore(marker: HistoryMarker): boolean {
        const stored = this.readStored(marker.ledgerId);
        if (stored === null) return false;

        const currentIndex = stored.entries.findIndex(entry => entry.key === marker.key);
        if (currentIndex < 0) return false;

        this.ledgerId = stored.ledgerId;
        this.entries = stored.entries.map((entry, index) => ({ ...entry, index }));
        this.currentIndex = currentIndex;

        const current = {
            ...this.entries[currentIndex],
            id: marker.id,
            index: currentIndex,
            url: location.href,
            path: this.toAppPath(location.href),
            state: marker.state ?? null
        };
        this.entries[currentIndex] = current;
        this.writeCurrent(current, this.foreignState(window.history.state));
        this.save();
        return true;
    }

    private push(url: string, path: string, state: unknown): void {
        const entry = this.entry(url, state ?? null);
        const nextEntries = this.entries.slice(0, this.currentIndex + 1);
        entry.index = nextEntries.length;
        nextEntries.push(entry);

        window.history.pushState(this.envelope(entry, null), "", url);
        this.entries = nextEntries;
        this.currentIndex = entry.index;
    }

    private replace(url: string, path: string, state: unknown): void {
        const current = this.currentEntry();
        if (current === null) {
            throw new DOMException("There is no current history entry.", "InvalidStateError");
        }

        const entry: NavEntryRef = {
            key: current.key,
            id: this.newToken("entry"),
            index: this.currentIndex,
            url,
            path,
            state: state === null || state === undefined ? current.state : state
        };

        window.history.replaceState(
            this.envelope(entry, this.foreignState(window.history.state)),
            "",
            url);
        this.entries[this.currentIndex] = entry;
    }

    private onPopState(event: PopStateEvent): void {
        if (this.disposed) return;

        const previousUrl = this.currentUrl();
        const marker = readMarker(event.state);
        let destinationIndex = marker === null
            ? -1
            : this.entries.findIndex(entry => entry.key === marker.key);

        if (marker !== null && marker.ledgerId !== this.ledgerId) {
            destinationIndex = this.restore(marker) ? this.currentIndex : -1;
        }

        if (marker === null || destinationIndex < 0) {
            this.adoptReachedEntry(marker, event.state);
            destinationIndex = this.currentIndex;
        } else {
            this.currentIndex = destinationIndex;
            const reached: NavEntryRef = {
                ...this.entries[destinationIndex],
                id: marker.id,
                index: destinationIndex,
                url: location.href,
                path: this.toAppPath(location.href),
                state: marker.state ?? null
            };
            this.entries[destinationIndex] = reached;
            this.save();
        }

        const destination = this.currentEntry();
        if (destination === null) return;

        const pending = this.pendingTraversal;
        const matchedPending = pending?.targetKey === destination.key;

        if (pending !== null) {
            this.finishPending(matchedPending
                ? this.success()
                : this.refuse("The browser traversed to a different entry.", "AbortError"));
        }

        const hashChange = this.isHashChange(previousUrl, destination.url);
        const observation = this.observation(
            "traverse",
            destination.url ?? location.href,
            destination.path,
            destination.key,
            destination.index,
            false,
            !matchedPending,
            hashChange,
            matchedPending ? pending?.info : null);

        if (destination.path === null) {
            this.publishSnapshot();
            location.reload();
            return;
        }

        this.publishSnapshot();

        if (hashChange) {
            this.scrollToFragment(destination.url);
        } else if (this.active) {
            this.dispatch(observation);
        }
    }

    private adoptReachedEntry(marker: HistoryMarker | null, rawState: unknown): void {
        this.ledgerId = marker?.ledgerId ?? this.newToken("ledger");
        const entry = this.entry(
            location.href,
            marker?.state ?? null,
            marker?.key,
            marker?.id);
        this.entries = [entry];
        this.currentIndex = 0;
        this.writeCurrent(entry, this.foreignState(rawState));
        this.save();
    }

    private async confirmIfNeeded(observation: NavigateObservation): Promise<boolean> {
        if (!this.guardArmed || this.isReplay(observation.info) || this.isRebind(observation.info)) {
            return true;
        }

        try {
            return await this.dotNetObject
                .invokeMethodAsync<boolean>("ConfirmLeaveAsync", observation);
        } catch (err) {
            console.error("[aurila] navigation guard failed; allowing the navigation.", err);
            return true;
        }
    }

    private dispatch(observation: NavigateObservation): void {
        const previousNavigationId = this.activeNavigationId;
        if (previousNavigationId !== null) {
            void this.dotNetObject
                .invokeMethodAsync("OnNavigationAbortedAsync", previousNavigationId)
                .catch(() => { });
        }

        const navigationId = this.nextNavigationId++;
        this.activeNavigationId = navigationId;
        const run: NavigationRun = {
            navigationId,
            observation,
            snapshot: this.getSnapshot()
        };

        void Promise.resolve()
            .then(() => this.dotNetObject.invokeMethodAsync("RunNavigationAsync", run))
            .catch(err => console.error("[aurila] navigation driver failed", err))
            .finally(() => {
                if (this.activeNavigationId === navigationId) {
                    this.activeNavigationId = null;
                }
            });
    }

    private finishPending(result: NavCommandResult): void {
        const pending = this.pendingTraversal;
        if (pending === null) return;

        this.pendingTraversal = null;
        window.clearTimeout(pending.timeout);
        pending.resolve(result);
    }

    private observation(
        kind: NavigateObservation["kind"],
        destinationUrl: string,
        destinationPath: string | null,
        destinationKey: string | null,
        destinationIndex: number,
        cancelable: boolean,
        userInitiated: boolean,
        hashChange: boolean,
        info: unknown): NavigateObservation {

        return {
            kind,
            destinationUrl,
            destinationPath,
            destinationKey,
            destinationIndex,
            canIntercept: true,
            cancelable,
            userInitiated,
            hashChange,
            info: this.toInfo(info)
        };
    }

    private entry(url: string, state: unknown, key?: string, id?: string): NavEntryRef {
        return {
            key: key ?? this.newToken("key"),
            id: id ?? this.newToken("entry"),
            index: 0,
            url,
            path: this.toAppPath(url),
            state
        };
    }

    private currentEntry(): NavEntryRef | null {
        return this.entries[this.currentIndex] ?? null;
    }

    private currentUrl(): string | null {
        return this.currentEntry()?.url ?? null;
    }

    private envelope(entry: NavEntryRef, foreignState: unknown): HistoryStateEnvelope {
        // Retain object-shaped state at the top level for interoperability with the host app and
        // other libraries using history.state. A primitive has to live under our reserved wrapper.
        const envelope: Record<string, unknown> = isRecord(foreignState)
            ? { ...foreignState }
            : { [FOREIGN_STATE_PROPERTY]: foreignState };

        delete envelope[STATE_PROPERTY];
        envelope[STATE_PROPERTY] = {
            version: FORMAT_VERSION,
            ledgerId: this.ledgerId,
            key: entry.key,
            id: entry.id,
            index: entry.index,
            state: entry.state
        } satisfies HistoryMarker;

        return envelope as HistoryStateEnvelope;
    }

    private writeCurrent(entry: NavEntryRef, foreignState: unknown): void {
        try {
            window.history.replaceState(this.envelope(entry, foreignState), "", entry.url ?? location.href);
        } catch (err) {
            console.warn("[aurila] could not annotate the current history entry", err);
        }
    }

    private foreignState(value: unknown): unknown {
        if (isRecord(value) && STATE_PROPERTY in value) {
            const foreign = { ...value };
            delete foreign[STATE_PROPERTY];
            return foreign;
        }
        return value ?? null;
    }

    private save(): void {
        const stored: StoredLedger = {
            version: FORMAT_VERSION,
            ledgerId: this.ledgerId,
            currentKey: this.currentEntry()?.key ?? "",
            entries: this.entries.map((entry, index) => ({ ...entry, index }))
        };

        try {
            sessionStorage.setItem(STORAGE_PREFIX + this.ledgerId, JSON.stringify(stored));
        } catch (err) {
            console.debug("[aurila] session history projection could not be persisted", err);
        }
    }

    private readStored(ledgerId: string): StoredLedger | null {
        try {
            const raw = sessionStorage.getItem(STORAGE_PREFIX + ledgerId);
            if (raw === null) return null;

            const value = JSON.parse(raw) as unknown;
            if (!isRecord(value)
                || value.version !== FORMAT_VERSION
                || value.ledgerId !== ledgerId
                || !Array.isArray(value.entries)) {
                return null;
            }

            const entries = value.entries.filter(isNavEntry);
            if (entries.length !== value.entries.length || entries.length === 0) return null;

            return {
                version: FORMAT_VERSION,
                ledgerId,
                currentKey: typeof value.currentKey === "string" ? value.currentKey : "",
                entries
            };
        } catch {
            return null;
        }
    }

    private isHashChange(from: string | null, to: string | null): boolean {
        if (from === null || to === null) return false;

        try {
            const previous = new URL(from);
            const next = new URL(to);
            return previous.origin === next.origin
                && previous.pathname === next.pathname
                && previous.search === next.search
                && previous.hash !== next.hash;
        } catch {
            return false;
        }
    }

    private scrollToFragment(url: string | null): void {
        if (url === null) return;

        try {
            const hash = new URL(url).hash;
            if (hash.length <= 1) return;

            const id = decodeURIComponent(hash.slice(1));
            window.setTimeout(() => document.getElementById(id)?.scrollIntoView(), 0);
        } catch {
            // A malformed escape sequence should not fail an otherwise valid navigation.
        }
    }

    private newToken(prefix: string): string {
        const cryptoObject = globalThis.crypto;

        if (typeof cryptoObject?.randomUUID === "function") {
            return `${prefix}-${cryptoObject.randomUUID()}`;
        }

        if (typeof cryptoObject?.getRandomValues === "function") {
            const values = new Uint32Array(4);
            cryptoObject.getRandomValues(values);
            return `${prefix}-${Array.from(values, value => value.toString(16)).join("")}`;
        }

        return `${prefix}-${Date.now().toString(36)}-${Math.random().toString(36).slice(2)}`;
    }

    private success(): NavCommandResult {
        return { committed: true, errorName: null, errorMessage: null };
    }

    private fromError(err: unknown): NavCommandResult {
        const error = err as { name?: string; message?: string };
        return this.refuse(error?.message ?? String(err), error?.name ?? "Error");
    }
}

function readMarker(value: unknown): HistoryMarker | null {
    if (!isRecord(value)) return null;

    const marker = value[STATE_PROPERTY];
    if (!isRecord(marker)
        || marker.version !== FORMAT_VERSION
        || typeof marker.ledgerId !== "string"
        || typeof marker.key !== "string"
        || typeof marker.id !== "string"
        || typeof marker.index !== "number") {
        return null;
    }

    return marker as unknown as HistoryMarker;
}

function isNavEntry(value: unknown): value is NavEntryRef {
    return isRecord(value)
        && typeof value.key === "string"
        && typeof value.id === "string"
        && typeof value.index === "number"
        && (typeof value.url === "string" || value.url === null)
        && (typeof value.path === "string" || value.path === null);
}

function isRecord(value: unknown): value is Record<string, unknown> {
    return typeof value === "object" && value !== null;
}
