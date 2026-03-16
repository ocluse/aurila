import { ElementScrollValues } from '../models/element-scroll-values';
import { DotNetObject } from '../common';
import { ScrollOrientation } from '../enums';

export class ScrollBox {
    private element: HTMLElement | null;
    private dotNetObjRef: DotNetObject | null;
    private throttleMilliseconds: number;
    private orientation: ScrollOrientation;
    private isThrottled: boolean = false;
    private triedScrollWhileThrottled: boolean = false;
    private timeoutId: number | null = null;

    private boundScrollHandler: () => void;

    constructor(
        element: HTMLElement,
        dotNetObjRef: DotNetObject,
        throttleMilliseconds: number,
        orientation: ScrollOrientation
    ) {
        this.element = element;
        this.dotNetObjRef = dotNetObjRef;
        this.throttleMilliseconds = throttleMilliseconds;
        this.orientation = orientation;

        this.boundScrollHandler = this.handleScroll.bind(this);
        this.setElement(element);
    }

    public setElement(element: HTMLElement | null): void {
        if (this.element) {
            this.element.removeEventListener('scroll', this.boundScrollHandler);
        }
        this.element = element;
        if (this.element) {
            this.element.addEventListener('scroll', this.boundScrollHandler);
        }
    }

    private handleScroll(): void {
        if (this.isThrottled) {
            this.triedScrollWhileThrottled = true;
            return;
        }

        this.isThrottled = true;
        this.notifyScrollChanged();

        this.timeoutId = window.setTimeout(() => {
            this.isThrottled = false;
            if (this.triedScrollWhileThrottled) {
                this.triedScrollWhileThrottled = false;
                this.notifyScrollChanged();
            }
        }, this.throttleMilliseconds);
    }

    private notifyScrollChanged(): void {
        if (!this.element || !this.dotNetObjRef) return;

        const scrollValues: ElementScrollValues = {
            scrollTop: this.element.scrollTop,
            scrollLeft: this.element.scrollLeft,
            scrollHeight: this.element.scrollHeight,
            scrollWidth: this.element.scrollWidth,
            clientHeight: this.element.clientHeight,
            clientWidth: this.element.clientWidth,
        };

        this.dotNetObjRef.invokeMethodAsync('HandleScrollFromJS', scrollValues).catch(error => {
            console.error('Error invoking C# scroll handler:', error);
        });
    }

    // --- Methods callable from C# ---

    public scrollToPosition(positionPx: number): void {
        if (!this.element) return;

        if (this.orientation === ScrollOrientation.Vertical) {
            this.element.scrollTop = positionPx;
        } else {
            this.element.scrollLeft = positionPx;
        }

        this.notifyScrollChanged();
    }

    public scrollToEnd(isVertical: boolean): void {
        if (!this.element) return;

        if (isVertical) {
            this.element.scrollTop = this.element.scrollHeight - this.element.clientHeight;
        } else {
            this.element.scrollLeft = this.element.scrollWidth - this.element.clientWidth;
        }

        this.notifyScrollChanged();
    }

    public scrollToStart(isVertical: boolean): void {
        if (!this.element) return;

        if (isVertical) {
            this.element.scrollTop = 0;
        } else {
            this.element.scrollLeft = 0;
        }

        this.notifyScrollChanged();
    }

    public dispose(): void {
        if (this.element) {
            this.element.removeEventListener('scroll', this.boundScrollHandler);
        }
        if (this.timeoutId !== null) {
            clearTimeout(this.timeoutId);
        }
        this.element = null;
        this.dotNetObjRef = null;
    }
}