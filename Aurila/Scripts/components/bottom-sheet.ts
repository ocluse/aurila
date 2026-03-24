import { DotNetObject } from "../common";

const OPEN_DURATION = 300;  // ms
const CLOSE_DURATION = 300;  // ms
const SNAP_DURATION = 300;  // ms

const VELOCITY_DISMISS = 0.4; // px/ms — fast flick down dismisses regardless of distance

const EASING_OPEN = 'cubic-bezier(0.2, 0, 0, 1)';
const EASING_CLOSE = 'cubic-bezier(0.4, 0, 1, 1)';
const EASING_SNAP = 'cubic-bezier(0.2, 0, 0, 1)';

const MIN_VISIBLE_PERCENT = 15;  // Minimum 15% visible height on open
const MAX_VISIBLE_PERCENT = 50;  // Maximum 50% visible height on open (content above this opens to 50%)

type SheetState = 'default' | 'expanded';

export class BottomSheet {
    private sheet: HTMLElement;
    private dotNetRef: DotNetObject;
    private dragStartY: number = 0;
    private dragStartTranslate: number = 0;
    private isDragging: boolean = false;
    private isTouchGesture: boolean = false;
    private gestureMode: 'undecided' | 'sheet' | 'content' = 'undecided';
    private activeScrollable: HTMLElement | null = null;
    private contentSnapPoint: number = 50;  // Calculated on each open based on content height

    private static readonly DRAG_DECISION_THRESHOLD_PX = 6;

    // Bound event handler references for cleanup
    private onDragStart: (e: MouseEvent | TouchEvent) => void;
    private onDragMove: (e: MouseEvent | TouchEvent) => void;
    private onDragEnd: (e: MouseEvent | TouchEvent) => void;

    constructor(dialog: HTMLElement, dotNetRef: DotNetObject) {
        this.sheet = dialog.getElementsByClassName("au-modal__content-area")[0] as HTMLElement;
        this.sheet.style.setProperty('--translate-y', '100%');
        this.dotNetRef = dotNetRef;

        this.onDragStart = this.handleDragStart.bind(this);
        this.onDragMove = this.handleDragMove.bind(this);
        this.onDragEnd = this.handleDragEnd.bind(this);

        this.sheet.addEventListener('mousedown', this.onDragStart);
        this.sheet.addEventListener('touchstart', this.onDragStart, { passive: true });
    }

    public open() {
        requestAnimationFrame(() => {
            requestAnimationFrame(() => {
                // Calculate snap point after layout has occurred
                this.contentSnapPoint = this.calculateSnapPoint();
                this.setTranslate(this.contentSnapPoint, true);
            });
        });
    }

    private calculateSnapPoint(): number {
        const viewportHeight = window.innerHeight;

        // Safeguard: ensure viewport height is valid
        if (viewportHeight <= 0) {
            return 100 - MAX_VISIBLE_PERCENT; // Default to 50% visible
        }

        // Measure actual content height by summing all direct children
        // (the sheet itself has height: 100%, so we need to measure its contents)
        const contentHeight = this.measureContentHeight();

        // Safeguard: ensure content height is valid and reasonable
        if (contentHeight <= 0 || !Number.isFinite(contentHeight)) {
            return 100 - MAX_VISIBLE_PERCENT; // Default to 50% visible
        }

        // Calculate what percentage of viewport the content needs
        const contentPercent = (contentHeight / viewportHeight) * 100;

        // Clamp between MIN_VISIBLE_PERCENT (15%) and MAX_VISIBLE_PERCENT (50%)
        const visiblePercent = Math.max(
            MIN_VISIBLE_PERCENT,
            Math.min(MAX_VISIBLE_PERCENT, contentPercent)
        );

        // Convert to translateY percentage (100 - visible = translate)
        return 100 - visiblePercent;
    }

    private measureContentHeight(): number {
        const children = this.sheet.children;
        
        if (children.length === 0) {
            return 0;
        }

        let totalHeight = 0;
        for (let i = 0; i < children.length; i++) {
            const child = children[i] as HTMLElement;
            totalHeight += child.offsetHeight;
        }

        // Include padding of the content area
        const computedStyle = window.getComputedStyle(this.sheet);
        const paddingTop = parseFloat(computedStyle.paddingTop) || 0;
        const paddingBottom = parseFloat(computedStyle.paddingBottom) || 0;

        return totalHeight + paddingTop + paddingBottom;
    }

    public close() {
        this.setTranslate(100, true);
    }

    public dispose() {
        this.sheet.removeEventListener('mousedown', this.onDragStart);
        this.sheet.removeEventListener('touchstart', this.onDragStart);
        document.removeEventListener('mousemove', this.onDragMove);
        document.removeEventListener('touchmove', this.onDragMove);
        document.removeEventListener('mouseup', this.onDragEnd);
        document.removeEventListener('touchend', this.onDragEnd);
    }

    private requestClose() {
        this.dotNetRef.invokeMethodAsync('RequestClose');
    }

    private handleDragStart(e: MouseEvent | TouchEvent) {
        this.isDragging = true;
        this.isTouchGesture = e instanceof TouchEvent;
        this.dragStartY = this.getClientY(e);
        this.dragStartTranslate = this.getCurrentTranslatePercent();
        this.gestureMode = this.isTouchGesture ? 'undecided' : 'sheet';
        this.activeScrollable = this.isTouchGesture
            ? this.findScrollableAncestor(e.target)
            : null;

        if (this.gestureMode === 'sheet') {
            this.sheet.style.transition = 'none';
        }

        document.addEventListener('mousemove', this.onDragMove);
        document.addEventListener('touchmove', this.onDragMove, { passive: false });
        document.addEventListener('mouseup', this.onDragEnd);
        document.addEventListener('touchend', this.onDragEnd);
    }

    private handleDragMove(e: MouseEvent | TouchEvent) {
        if (!this.isDragging) return;

        const currentY = this.getClientY(e);
        const deltaY = currentY - this.dragStartY;

        if (this.isTouchGesture) {
            this.resolveTouchGestureMode(e, deltaY, currentY);
            if (this.gestureMode !== 'sheet') {
                return;
            }
        }

        if (e.type === 'touchmove') {
            if (!e.cancelable) {
                return;
            }
            e.preventDefault();
        }

        const windowHeight = window.innerHeight;
        const deltaPercent = (deltaY / windowHeight) * 100;

        const newTranslate = Math.min(100, Math.max(0, this.dragStartTranslate + deltaPercent));
        this.setTranslate(newTranslate, false);
    }

    private handleDragEnd(e: MouseEvent | TouchEvent) {
        if (!this.isDragging) return;
        this.isDragging = false;

        document.removeEventListener('mousemove', this.onDragMove);
        document.removeEventListener('touchmove', this.onDragMove);
        document.removeEventListener('mouseup', this.onDragEnd);
        document.removeEventListener('touchend', this.onDragEnd);

        this.activeScrollable = null;

        if (this.gestureMode !== 'sheet') {
            this.gestureMode = 'undecided';
            return;
        }

        this.gestureMode = 'undecided';
        this.sheet.style.transition = '';

        const current = this.getCurrentTranslatePercent();

        // Calculate dynamic thresholds based on contentSnapPoint
        // Midpoint between fully-open (0%) and content snap point
        const upperThreshold = this.contentSnapPoint / 2;
        // Midpoint between content snap point and closed (100%)
        const lowerThreshold = this.contentSnapPoint + (100 - this.contentSnapPoint) / 2;

        if (current < upperThreshold) {
            // Snap fully open
            this.setTranslate(0, true);
        } else if (current < lowerThreshold) {
            // Snap to content height
            this.setTranslate(this.contentSnapPoint, true);
        } else {
            // Close
            this.setTranslate(100, true);
            this.requestClose();
        }
    }

    private setTranslate(percent: number, animated: boolean) {
        if (!animated) {
            this.sheet.style.transition = 'none';
        } else {
            this.sheet.style.transition = '';
        }
        this.sheet.style.setProperty('--translate-y', `${percent}%`);
    }

    private resolveTouchGestureMode(e: MouseEvent | TouchEvent, deltaY: number, currentY: number): void {
        if (this.gestureMode === 'sheet') {
            return;
        }

        if (Math.abs(deltaY) < BottomSheet.DRAG_DECISION_THRESHOLD_PX) {
            return;
        }

        if (this.gestureMode === 'content') {
            if (deltaY > 0
                && this.isScrollableAtTop(this.activeScrollable)
                && this.canTakeOverTouchGesture(e)) {
                this.switchToSheetMode(currentY);
            }
            return;
        }

        if (deltaY < 0) {
            if (this.getCurrentTranslatePercent() > 0) {
                this.switchToSheetMode(currentY);
            } else {
                this.gestureMode = 'content';
            }
            return;
        }

        if (this.canScrollUp(this.activeScrollable)) {
            this.gestureMode = 'content';
            return;
        }

        if (!this.canTakeOverTouchGesture(e)) {
            this.gestureMode = 'content';
            return;
        }

        this.switchToSheetMode(currentY);
    }

    private canTakeOverTouchGesture(e: MouseEvent | TouchEvent): boolean {
        return !(e instanceof TouchEvent) || e.cancelable;
    }

    private switchToSheetMode(currentY: number): void {
        this.gestureMode = 'sheet';
        this.dragStartY = currentY;
        this.dragStartTranslate = this.getCurrentTranslatePercent();
        this.sheet.style.transition = 'none';
    }

    private findScrollableAncestor(target: EventTarget | null): HTMLElement | null {
        if (!(target instanceof HTMLElement)) {
            return null;
        }

        let element: HTMLElement | null = target;
        while (element && element !== this.sheet) {
            if (this.isVerticallyScrollable(element)) {
                return element;
            }
            element = element.parentElement;
        }

        return null;
    }

    private isVerticallyScrollable(element: HTMLElement): boolean {
        if (element.scrollHeight <= element.clientHeight + 1) {
            return false;
        }

        const overflowY = window.getComputedStyle(element).overflowY;
        return overflowY === 'auto' || overflowY === 'scroll' || overflowY === 'overlay';
    }

    private canScrollUp(element: HTMLElement | null): boolean {
        if (!element) {
            return false;
        }
        return element.scrollTop > 0;
    }

    private isScrollableAtTop(element: HTMLElement | null): boolean {
        if (!element) {
            return true;
        }
        return element.scrollTop <= 0;
    }

    private getCurrentTranslatePercent(): number {
        const raw = this.sheet.style.getPropertyValue('--translate-y');
        return parseFloat(raw) || 0;
    }

    private getClientY(e: MouseEvent | TouchEvent): number {
        return e instanceof TouchEvent ? e.touches[0].clientY : e.clientY;
    }
}
