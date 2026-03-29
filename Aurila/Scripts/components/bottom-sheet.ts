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
    private hasDraggedBeyondClickThreshold: boolean = false;
    private isTouchGesture: boolean = false;
    private gestureMode: 'undecided' | 'sheet' | 'content' = 'undecided';
    private activeScrollable: HTMLElement | null = null;
    private initialSnapPoint: number = 50;  // Calculated on each open based on content height
    private maxExpandSnapPoint: number = 0; // Furthest upward expansion based on content height
    private suppressClickUntilTimestamp: number = 0;
    private wheelSnapLockUntilTimestamp: number = 0;
    private recomputeRafId: number | null = null;

    private resizeObserver: ResizeObserver | null = null;
    private mutationObserver: MutationObserver | null = null;

    private static readonly DRAG_DECISION_THRESHOLD_PX = 6;
    private static readonly CLICK_SUPPRESS_THRESHOLD_PX = 8;
    private static readonly CLICK_SUPPRESS_WINDOW_MS = 350;
    private static readonly WHEEL_SNAP_LOCK_MS = 220;
    private static readonly TRANSLATE_EPSILON = 0.25;

    // Bound event handler references for cleanup
    private onDragStart: (e: MouseEvent | TouchEvent) => void;
    private onDragMove: (e: MouseEvent | TouchEvent) => void;
    private onDragEnd: (e: MouseEvent | TouchEvent) => void;
    private onClickCapture: (e: MouseEvent) => void;
    private onWheel: (e: WheelEvent) => void;
    private onWindowResize: () => void;

    constructor(dialog: HTMLElement, dotNetRef: DotNetObject) {
        this.sheet = dialog.getElementsByClassName("au-modal__content-area")[0] as HTMLElement;
        this.sheet.style.setProperty('--translate-y', '100%');
        this.dotNetRef = dotNetRef;

        this.onDragStart = this.handleDragStart.bind(this);
        this.onDragMove = this.handleDragMove.bind(this);
        this.onDragEnd = this.handleDragEnd.bind(this);
        this.onClickCapture = this.handleClickCapture.bind(this);
        this.onWheel = this.handleWheel.bind(this);
        this.onWindowResize = this.scheduleSnapPointRecompute.bind(this);

        this.sheet.addEventListener('mousedown', this.onDragStart);
        this.sheet.addEventListener('touchstart', this.onDragStart, { passive: true });
        this.sheet.addEventListener('click', this.onClickCapture, true);
        this.sheet.addEventListener('wheel', this.onWheel, { passive: false });
        window.addEventListener('resize', this.onWindowResize);

        this.initObservers();
    }

    public open() {
        requestAnimationFrame(() => {
            requestAnimationFrame(() => {
                // Calculate snap point after layout has occurred
                this.recomputeSnapPoints();
                this.setTranslate(this.initialSnapPoint, true);
            });
        });
    }

    private recomputeSnapPoints(): void {
        const { initialTranslatePercent, maxExpandTranslatePercent } = this.calculateSnapPoints();
        this.initialSnapPoint = initialTranslatePercent;
        this.maxExpandSnapPoint = maxExpandTranslatePercent;
    }

    private calculateSnapPoints(): { initialTranslatePercent: number; maxExpandTranslatePercent: number } {
        const viewportHeight = window.innerHeight;

        // Safeguard: ensure viewport height is valid
        if (viewportHeight <= 0) {
            const fallback = 100 - MAX_VISIBLE_PERCENT;
            return { initialTranslatePercent: fallback, maxExpandTranslatePercent: fallback };
        }

        // Measure intrinsic content height from a detached clone so height: 100% children
        // (like ScrollBox) resolve to content-driven size instead of viewport size.
        const contentHeight = this.measureContentHeight();

        // Safeguard: ensure content height is valid and reasonable
        if (contentHeight <= 0 || !Number.isFinite(contentHeight)) {
            const fallback = 100 - MAX_VISIBLE_PERCENT;
            return { initialTranslatePercent: fallback, maxExpandTranslatePercent: fallback };
        }

        // Calculate what percentage of viewport the content needs
        const contentPercent = (contentHeight / viewportHeight) * 100;

        // Clamp between MIN_VISIBLE_PERCENT (15%) and MAX_VISIBLE_PERCENT (50%)
        const visiblePercent = Math.max(
            MIN_VISIBLE_PERCENT,
            Math.min(MAX_VISIBLE_PERCENT, contentPercent)
        );

        // Max expansion should never exceed the height needed by content.
        const maxVisiblePercent = Math.max(
            MIN_VISIBLE_PERCENT,
            Math.min(100, contentPercent)
        );

        return {
            initialTranslatePercent: 100 - visiblePercent,
            maxExpandTranslatePercent: 100 - maxVisiblePercent,
        };
    }

    private measureContentHeight(): number {
        const children = this.sheet.children;
        
        if (children.length === 0) {
            return 0;
        }

        const measureContainer = document.createElement('div');
        measureContainer.style.position = 'fixed';
        measureContainer.style.visibility = 'hidden';
        measureContainer.style.pointerEvents = 'none';
        measureContainer.style.left = '-99999px';
        measureContainer.style.top = '0';
        measureContainer.style.height = 'auto';
        measureContainer.style.maxHeight = 'none';
        measureContainer.style.overflow = 'visible';
        measureContainer.style.transform = 'none';

        const sheetRect = this.sheet.getBoundingClientRect();
        const measureWidth = sheetRect.width > 0 ? sheetRect.width : window.innerWidth;
        measureContainer.style.width = `${measureWidth}px`;

        for (let i = 0; i < children.length; i++) {
            measureContainer.appendChild(children[i].cloneNode(true));
        }

        document.body.appendChild(measureContainer);
        const measuredHeight = measureContainer.scrollHeight;
        measureContainer.remove();

        // Include padding of the content area
        const computedStyle = window.getComputedStyle(this.sheet);
        const paddingTop = parseFloat(computedStyle.paddingTop) || 0;
        const paddingBottom = parseFloat(computedStyle.paddingBottom) || 0;

        return measuredHeight + paddingTop + paddingBottom;
    }

    public close() {
        this.setTranslate(100, true);
    }

    public dispose() {
        this.sheet.removeEventListener('mousedown', this.onDragStart);
        this.sheet.removeEventListener('touchstart', this.onDragStart);
        this.sheet.removeEventListener('click', this.onClickCapture, true);
        this.sheet.removeEventListener('wheel', this.onWheel);
        window.removeEventListener('resize', this.onWindowResize);
        document.removeEventListener('mousemove', this.onDragMove);
        document.removeEventListener('touchmove', this.onDragMove);
        document.removeEventListener('mouseup', this.onDragEnd);
        document.removeEventListener('touchend', this.onDragEnd);

        if (this.recomputeRafId !== null) {
            cancelAnimationFrame(this.recomputeRafId);
            this.recomputeRafId = null;
        }

        this.resizeObserver?.disconnect();
        this.resizeObserver = null;
        this.mutationObserver?.disconnect();
        this.mutationObserver = null;
    }

    private requestClose() {
        this.dotNetRef.invokeMethodAsync('RequestClose');
    }

    private handleDragStart(e: MouseEvent | TouchEvent) {
        if (e instanceof MouseEvent && e.button !== 0) {
            return;
        }

        this.isDragging = true;
        this.hasDraggedBeyondClickThreshold = false;
        this.isTouchGesture = e instanceof TouchEvent;
        this.dragStartY = this.getClientY(e);
        const dragStartX = this.getClientX(e);
        this.dragStartTranslate = this.getCurrentTranslatePercent();
        this.dragStartX = dragStartX;
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
        const currentX = this.getClientX(e);
        const deltaY = currentY - this.dragStartY;
        const deltaX = currentX - this.dragStartX;

        if (!this.hasDraggedBeyondClickThreshold
            && Math.hypot(deltaX, deltaY) >= BottomSheet.CLICK_SUPPRESS_THRESHOLD_PX) {
            this.hasDraggedBeyondClickThreshold = true;
        }

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

        const newTranslate = Math.min(100, Math.max(this.maxExpandSnapPoint, this.dragStartTranslate + deltaPercent));
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

        if (this.hasDraggedBeyondClickThreshold) {
            this.suppressClickUntilTimestamp = performance.now() + BottomSheet.CLICK_SUPPRESS_WINDOW_MS;
        }

        if (this.gestureMode !== 'sheet') {
            this.gestureMode = 'undecided';
            return;
        }

        this.gestureMode = 'undecided';
        this.sheet.style.transition = '';

        const current = this.getCurrentTranslatePercent();

        // Midpoint between max expanded and initial snap point
        const upperThreshold = this.maxExpandSnapPoint + (this.initialSnapPoint - this.maxExpandSnapPoint) / 2;
        // Midpoint between initial snap point and closed
        const lowerThreshold = this.initialSnapPoint + (100 - this.initialSnapPoint) / 2;

        if (current < upperThreshold) {
            // Snap maximally expanded for current content
            this.setTranslate(this.maxExpandSnapPoint, true);
        } else if (current < lowerThreshold) {
            // Snap to content height
            this.setTranslate(this.initialSnapPoint, true);
        } else {
            // Close
            this.setTranslate(100, true);
            this.requestClose();
        }
    }

    private setTranslate(percent: number, animated: boolean) {
        percent = Math.min(100, Math.max(this.maxExpandSnapPoint, percent));

        if (!animated) {
            this.sheet.style.transition = 'none';
        } else {
            this.sheet.style.transition = '';
        }
        this.sheet.style.setProperty('--translate-y', `${percent}%`);
    }

    private handleClickCapture(e: MouseEvent): void {
        if (performance.now() > this.suppressClickUntilTimestamp) {
            return;
        }

        e.preventDefault();
        e.stopPropagation();
        e.stopImmediatePropagation();
    }

    private handleWheel(e: WheelEvent): void {
        if (this.isDragging) {
            return;
        }

        const current = this.getCurrentTranslatePercent();
        const canExpandFurther = current > this.maxExpandSnapPoint + BottomSheet.TRANSLATE_EPSILON;

        // Keep wheel-up as normal "scroll up" behavior.
        if (e.deltaY <= 0) {
            return;
        }

        if (!canExpandFurther) {
            return;
        }

        if (performance.now() < this.wheelSnapLockUntilTimestamp) {
            e.preventDefault();
            return;
        }

        const nextStage = this.getNextExpandStage(current);
        if (nextStage === null) {
            return;
        }

        e.preventDefault();
        this.wheelSnapLockUntilTimestamp = performance.now() + BottomSheet.WHEEL_SNAP_LOCK_MS;
        this.setTranslate(nextStage, true);
    }

    private getNextExpandStage(currentTranslate: number): number | null {
        const stages = [this.initialSnapPoint, this.maxExpandSnapPoint]
            .filter((value, index, self) => self.indexOf(value) === index)
            .sort((a, b) => b - a);

        for (const stage of stages) {
            if (stage < currentTranslate - BottomSheet.TRANSLATE_EPSILON) {
                return stage;
            }
        }

        return null;
    }

    private initObservers(): void {
        this.resizeObserver = new ResizeObserver(() => {
            this.scheduleSnapPointRecompute();
        });

        for (const child of Array.from(this.sheet.children)) {
            if (child instanceof HTMLElement) {
                this.resizeObserver.observe(child);
            }
        }

        this.mutationObserver = new MutationObserver(() => {
            this.resizeObserver?.disconnect();
            for (const child of Array.from(this.sheet.children)) {
                if (child instanceof HTMLElement) {
                    this.resizeObserver?.observe(child);
                }
            }

            this.scheduleSnapPointRecompute();
        });

        this.mutationObserver.observe(this.sheet, {
            childList: true,
            subtree: true,
            attributes: true,
            characterData: true,
        });
    }

    private scheduleSnapPointRecompute(): void {
        if (this.recomputeRafId !== null) {
            return;
        }

        this.recomputeRafId = requestAnimationFrame(() => {
            this.recomputeRafId = null;
            this.recomputeSnapPoints();

            const current = this.getCurrentTranslatePercent();
            if (current >= 100 - BottomSheet.TRANSLATE_EPSILON) {
                return;
            }

            if (current < this.maxExpandSnapPoint - BottomSheet.TRANSLATE_EPSILON) {
                this.setTranslate(this.maxExpandSnapPoint, true);
                return;
            }

            if (Math.abs(current - this.initialSnapPoint) <= BottomSheet.TRANSLATE_EPSILON) {
                this.setTranslate(this.initialSnapPoint, false);
            }
        });
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

    private dragStartX: number = 0;

    private getClientY(e: MouseEvent | TouchEvent): number {
        return e instanceof TouchEvent ? e.touches[0].clientY : e.clientY;
    }

    private getClientX(e: MouseEvent | TouchEvent): number {
        return e instanceof TouchEvent ? e.touches[0].clientX : e.clientX;
    }
}
