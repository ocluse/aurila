export interface DotNetObject {
    invokeMethodAsync(methodName: string, ...args: any[]): Promise<any>;
}