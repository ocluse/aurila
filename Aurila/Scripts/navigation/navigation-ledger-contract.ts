export interface NavEntryRef {
    key: string;
    id: string;
    index: number;
    url: string | null;
    path: string | null;
    state: unknown;
}

export interface NavSnapshot {
    entries: NavEntryRef[];
    currentIndex: number;
}

export interface NavigateObservation {
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

export interface NavigationRun {
    navigationId: number;
    observation: NavigateObservation;
    snapshot: NavSnapshot;
}

export interface NavCommandResult {
    committed: boolean;
    errorName: string | null;
    errorMessage: string | null;
}

/** The platform-neutral contract consumed through JS interop by JsNavigationLedger. */
export interface NavigationLedgerBackend {
    getSnapshot(): NavSnapshot;
    canGoBack(): boolean;
    canGoForward(): boolean;
    navigate(path: string, history: "push" | "replace", state: unknown, info: unknown): Promise<NavCommandResult>;
    traverseTo(key: string, info: unknown): Promise<NavCommandResult>;
    reload(info: unknown): Promise<NavCommandResult>;
    back(info: unknown): Promise<NavCommandResult>;
    forward(info: unknown): Promise<NavCommandResult>;
    updateState(state: unknown): void;
    activate(): void;
    setGuardArmed(armed: boolean): void;
    dispose(): void;
}
