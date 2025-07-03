import { DotNetObject } from "../common";

export class PullToRefreshBox {
    private contentElement: HTMLElement | null = null;
    private parentElement: HTMLElement | null = null;
    private peekElement: HTMLElement | null = null;
    private dotNetObj: DotNetObject;
    private startY: number = 0;
    private isRefreshing: boolean = false;
    private threshold: number = 50;
    private timeThreshold: number = 300; // ms
    private startTime: number = 0;

    private boundTouchStart: (event: TouchEvent) => void;
    private boundTouchMove: (event: TouchEvent) => void;
    private boundTouchEnd: (event: TouchEvent) => void;

    constructor(contentElement: HTMLElement, dotNetObj: DotNetObject) {
        this.dotNetObj = dotNetObj;

        this.boundTouchStart = this.handleTouchStart.bind(this);
        this.boundTouchMove = this.handleTouchMove.bind(this);
        this.boundTouchEnd = this.handleTouchEnd.bind(this);

        this.setElement(contentElement);
    }

    private handleTouchStart(event: TouchEvent): void {
        if (!this.contentElement) return;

        const scrollTop = this.contentElement.scrollTop;

        if (scrollTop === 0) {
            this.startY = event.touches[0].pageY;
            this.startTime = Date.now();
        }
    }

    private handleTouchMove(event: TouchEvent): void {
        if (!this.contentElement || !this.parentElement || !this.startY || this.isRefreshing) return;

        const currentY = event.touches[0].pageY;
        const deltaY = currentY - this.startY;
        const scrollTop = this.contentElement.scrollTop;

        if (deltaY > 0 && scrollTop === 0) {
            const pullAmount = Math.min(deltaY, this.threshold);
            const progress = Math.min(deltaY / this.threshold, 1);

            this.parentElement.style.setProperty('--pull-distance', `${pullAmount}px`);
            this.parentElement.style.setProperty('--pull-progress', `${progress}`);

            event.preventDefault();
        }
    }

    private handleTouchEnd(event: TouchEvent): void {
        if (!this.startY || this.isRefreshing || !this.parentElement) return;

        const deltaY = event.changedTouches[0].pageY - this.startY;
        const duration = Date.now() - this.startTime;

        this.parentElement.style.setProperty('--pull-distance', '0px');
        this.parentElement.style.setProperty('--pull-progress', '0');

        if (deltaY > this.threshold && duration >= this.timeThreshold) {
            this.isRefreshing = true;
            this.dotNetObj.invokeMethodAsync('HandleRefresh')
                .finally(() => {
                    this.isRefreshing = false;
                });
        }

        this.startY = 0;
    }

    public setElement(contentElement: HTMLElement): void {
        if (this.contentElement === contentElement) return;

        this.dispose();

        this.contentElement = contentElement;
        this.parentElement = contentElement.parentElement;

        if (this.contentElement) {
            this.parentElement?.style.setProperty('--pull-distance', '0px');
            this.parentElement?.style.setProperty('--pull-progress', '0');

            this.contentElement.addEventListener('touchstart', this.boundTouchStart);
            this.contentElement.addEventListener('touchmove', this.boundTouchMove);
            this.contentElement.addEventListener('touchend', this.boundTouchEnd);
        }

        this.peekElement = this.contentElement.previousElementSibling as HTMLElement | null;

        if (this.peekElement) {
            const peekHeight = this.peekElement.offsetHeight;
            if (peekHeight > this.threshold) {
                this.threshold = peekHeight;
            }
        }
    }

    public dispose(): void {
        if (this.contentElement) {
            this.contentElement.removeEventListener('touchstart', this.boundTouchStart);
            this.contentElement.removeEventListener('touchmove', this.boundTouchMove);
            this.contentElement.removeEventListener('touchend', this.boundTouchEnd);
            this.contentElement.style.transform = '';
            this.contentElement.style.transition = '';
        }

        this.isRefreshing = false;
        this.startY = 0;
    }
}