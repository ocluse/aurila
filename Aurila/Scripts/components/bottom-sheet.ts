import { DotNetObject } from "../common";

const DISMISS_THRESHOLD = 120;  // px dragged down before dismissing
const VELOCITY_THRESHOLD = 0.4; // px/ms — fast flick also dismisses
const OPEN_DURATION = 300;      // ms — slide-in
const CLOSE_DURATION = 300;     // ms — slide-out
const SNAP_DURATION = 300;      // ms — snap back after insufficient drag

const EASING_OPEN  = 'cubic-bezier(0.2, 0, 0, 1)';
const EASING_CLOSE = 'cubic-bezier(0.4, 0, 1, 1)';
const EASING_SNAP  = 'cubic-bezier(0.2, 0, 0, 1)';

export class BottomSheet {
    private contentArea: HTMLElement;
    private dotNetObj: DotNetObject;
    private maxHeight: string;

    private isDragging = false;
    private dragStartY = 0;
    private dragStartTime = 0;
    private currentDeltaY = 0;
    private startHeight = 0;

    private boundPointerDown: (e: PointerEvent) => void;
    private boundPointerMove: (e: PointerEvent) => void;
    private boundPointerUp:   (e: PointerEvent) => void;

    constructor(contentArea: HTMLElement, dotNetObj: DotNetObject, maxHeight: string) {
        this.contentArea = contentArea;
        this.dotNetObj   = dotNetObj;
        this.maxHeight   = maxHeight;

        this.boundPointerDown = this.handlePointerDown.bind(this);
        this.boundPointerMove = this.handlePointerMove.bind(this);
        this.boundPointerUp   = this.handlePointerUp.bind(this);

        this.contentArea.addEventListener('pointerdown', this.boundPointerDown);

        // Play the open animation entirely in JS so CSS keyframes never touch transform.
        this.setTransform('translateY(100%)', '');
        // Force a reflow so the browser registers the starting position before we transition.
        this.contentArea.getBoundingClientRect();
        this.setTransform('translateY(0)', `transform ${OPEN_DURATION}ms ${EASING_OPEN}`);
        setTimeout(() => this.clearTransition(), OPEN_DURATION);
    }

    // ── drag ──────────────────────────────────────────────────────────────────

    private handlePointerDown(e: PointerEvent): void {
        if (!e.isPrimary) return;

        const scrollable = this.findScrollable();
        if (scrollable && scrollable.scrollTop > 0) return;

        this.isDragging    = true;
        this.dragStartY    = e.clientY;
        this.dragStartTime = performance.now();
        this.currentDeltaY = 0;
        this.startHeight   = this.contentArea.offsetHeight;

        this.contentArea.setPointerCapture(e.pointerId);
        this.contentArea.addEventListener('pointermove', this.boundPointerMove);
        this.contentArea.addEventListener('pointerup',   this.boundPointerUp);
        this.contentArea.addEventListener('pointercancel', this.boundPointerUp);

        // Kill any in-progress transition so drag is immediate.
        this.clearTransition();
    }

    private handlePointerMove(e: PointerEvent): void {
        if (!this.isDragging || !e.isPrimary) return;

        const deltaY = e.clientY - this.dragStartY;

        if (deltaY > 0) {
            this.setTransform(`translateY(${deltaY}px)`, '');
            this.contentArea.style.height = '';
        } else {
            this.setTransform('translateY(0)', '');
            const newHeight = this.startHeight - deltaY;
            this.contentArea.style.height = `min(${newHeight}px, ${this.maxHeight})`;
        }

        this.currentDeltaY = deltaY;
    }

    private handlePointerUp(e: PointerEvent): void {
        if (!this.isDragging || !e.isPrimary) return;
        this.isDragging = false;

        this.contentArea.removeEventListener('pointermove', this.boundPointerMove);
        this.contentArea.removeEventListener('pointerup',   this.boundPointerUp);
        this.contentArea.removeEventListener('pointercancel', this.boundPointerUp);

        const elapsed = performance.now() - this.dragStartTime;
        const velocity = elapsed > 0 ? this.currentDeltaY / elapsed : 0;
        const shouldDismiss =
            this.currentDeltaY > DISMISS_THRESHOLD ||
            (this.currentDeltaY > 0 && velocity > VELOCITY_THRESHOLD);

        if (shouldDismiss) {
            this.slideOut().then(() => this.dotNetObj.invokeMethodAsync('HandleDismissed'));
        } else {
            // Snap back to resting position from wherever the drag ended.
            const heightTransition = this.currentDeltaY < 0
                ? `, height ${SNAP_DURATION}ms ${EASING_SNAP}`
                : '';
            this.setTransform('translateY(0)', `transform ${SNAP_DURATION}ms ${EASING_SNAP}${heightTransition}`);
            this.contentArea.style.height = '';
            setTimeout(() => this.clearTransition(), SNAP_DURATION);
        }

        this.currentDeltaY = 0;
    }

    // ── public API ────────────────────────────────────────────────────────────

    /** Called by C# PlayCloseAnimation — slides the sheet out and resolves when done. */
    public slideOut(): Promise<void> {
        this.clearTransition();
        // Force reflow so transition starts from current position, not from a cached value.
        this.contentArea.getBoundingClientRect();
        this.setTransform(
            'translateY(100%)',
            `transform ${CLOSE_DURATION}ms ${EASING_CLOSE}`
        );
        this.contentArea.style.height = '';
        return new Promise(resolve => setTimeout(resolve, CLOSE_DURATION));
    }

    public dispose(): void {
        this.contentArea.removeEventListener('pointerdown',   this.boundPointerDown);
        this.contentArea.removeEventListener('pointermove',   this.boundPointerMove);
        this.contentArea.removeEventListener('pointerup',     this.boundPointerUp);
        this.contentArea.removeEventListener('pointercancel', this.boundPointerUp);
        this.contentArea.style.transform  = '';
        this.contentArea.style.height     = '';
        this.contentArea.style.transition = '';
        this.isDragging = false;
    }

    // ── helpers ───────────────────────────────────────────────────────────────

    private setTransform(value: string, transition: string): void {
        this.contentArea.style.transition = transition;
        this.contentArea.style.transform  = value;
    }

    private clearTransition(): void {
        this.contentArea.style.transition = '';
    }

    private findScrollable(): HTMLElement | null {
        for (const el of this.contentArea.querySelectorAll<HTMLElement>('*')) {
            const { overflow, overflowY } = getComputedStyle(el);
            if (/auto|scroll/.test(overflow + overflowY) && el.scrollHeight > el.clientHeight) {
                return el;
            }
        }
        return null;
    }
}
