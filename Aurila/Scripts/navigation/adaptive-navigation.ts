import { DotNetObject } from "../common";

export class AdaptiveNavigationLayoutObserver {
    private element: HTMLElement | null;
    private dotNetObject: DotNetObject;
    private resizeObserver: ResizeObserver | null = null;

    constructor(element: HTMLElement, dotNetObject: DotNetObject) {
        this.element = element;
        this.dotNetObject = dotNetObject;
        this.setElement(element);
    }

    public setElement(element: HTMLElement | null): void {
        if (this.resizeObserver && this.element) {
            this.resizeObserver.unobserve(this.element);
        }

        this.element = element;

        if (!this.resizeObserver) {
            this.resizeObserver = new ResizeObserver(entries => {
                if (!entries.length) return;

                const entry = entries[0];
                const width = Math.round(entry.contentRect.width);
                this.dotNetObject.invokeMethodAsync("HandleLayoutWidthChanged", width)
                    .catch(error => console.error("Adaptive layout width callback failed:", error));
            });
        }

        if (this.element) {
            this.resizeObserver.observe(this.element);
            const width = Math.round(this.element.getBoundingClientRect().width);
            this.dotNetObject.invokeMethodAsync("HandleLayoutWidthChanged", width)
                .catch(error => console.error("Adaptive layout initial width callback failed:", error));
        }
    }

    public dispose(): void {
        if (this.resizeObserver) {
            if (this.element) {
                this.resizeObserver.unobserve(this.element);
            }
            this.resizeObserver.disconnect();
            this.resizeObserver = null;
        }

        this.element = null;
    }
}