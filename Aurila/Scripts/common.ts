export interface DotNetObject {
    invokeMethodAsync(methodName: string, ...args: any[]): Promise<any>;
    invokeMethodAsync<T>(method: string, ...args: any[]): Promise<T>;
}

export interface ScrollValues {
    scrollTop: number;
    scrollLeft: number;
    scrollHeight: number;
    scrollWidth: number;
    clientHeight: number;
    clientWidth: number;
}