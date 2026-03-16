import { DotNetObject } from "../common";

const OPEN_DURATION  = 300;  // ms
const CLOSE_DURATION = 300;  // ms
const SNAP_DURATION  = 300;  // ms

const VELOCITY_DISMISS = 0.4; // px/ms — fast flick down dismisses regardless of distance

const EASING_OPEN  = 'cubic-bezier(0.2, 0, 0, 1)';
const EASING_CLOSE = 'cubic-bezier(0.4, 0, 1, 1)';
const EASING_SNAP  = 'cubic-bezier(0.2, 0, 0, 1)';

type SheetState = 'default' | 'expanded';

export class BottomSheet {
    private contentArea: HTMLElement;
    private dotNetObj: DotNetObject;
    private maxHeight: string;

    private state: SheetState = 'default';

    private isDragging    = false;
    private dragStartY    = 0;
    private dragStartTime = 0;
    private currentDeltaY = 0;
    private startHeight   = 0;   // px — offsetHeight at drag-start
    private maxHeightPx   = 0;   // px — resolved maxHeight at drag-start

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

        // Open animation — entirely JS-driven so CSS keyframes never own transform.
        this.setTransform('translateY(100%)', '');
        this.contentArea.getBoundingClientRect(); // force reflow
        this.setTransform('translateY(0)', `transform ${OPEN_DURATION}ms ${EASING_OPEN}`);
        setTimeout(() => this.clearTransition(), OPEN_DURATION);
    }

    // ── drag ──────────────────────────────────────────────────────────────────

    private handlePointerDown(e: PointerEvent): void {
        if (!e.isPrimary) return;

        // When expanded, allow dragging from anywhere (sheet fills most of screen).
        // When at default height, only start if content is scrolled to top.
        if (this.state === 'default') {
            const scrollable = this.findScrollable();
            if (scrollable && scrollable.scrollTop > 0) return;
        }

        this.isDragging    = true;
        this.dragStartY    = e.clientY;
        this.dragStartTime = performance.now();
        this.currentDeltaY = 0;
        this.startHeight   = this.contentArea.offsetHeight;
        this.maxHeightPx   = this.resolveMaxHeightPx();

        this.contentArea.setPointerCapture(e.pointerId);
        this.contentArea.addEventListener('pointermove',   this.boundPointerMove);
        this.contentArea.addEventListener('pointerup',     this.boundPointerUp);
        this.contentArea.addEventListener('pointercancel', this.boundPointerUp);

        this.clearTransition();
    }

    private handlePointerMove(e: PointerEvent): void {
        if (!this.isDragging || !e.isPrimary) return;

        const deltaY = e.clientY - this.dragStartY;

        if (deltaY > 0) {
            // Dragging down — translate downward.
            // Pin height in px so clearing the CSS value (e.g. "90vh") doesn't
            // collapse the element to its natural height mid-drag.
            this.contentArea.style.height = `${this.startHeight}px`;
            this.setTransform(`translateY(${deltaY}px)`, '');
        } else {
            // Dragging up — stretch height, keep at translateY(0).
            this.setTransform('translateY(0)', '');
            const newHeight = this.startHeight - deltaY; // deltaY negative → adds
            this.contentArea.style.height = `min(${newHeight}px, ${this.maxHeight})`;
        }

        this.currentDeltaY = deltaY;
    }

    private handlePointerUp(e: PointerEvent): void {
        if (!this.isDragging || !e.isPrimary) return;
        this.isDragging = false;

        this.contentArea.removeEventListener('pointermove',   this.boundPointerMove);
        this.contentArea.removeEventListener('pointerup',     this.boundPointerUp);
        this.contentArea.removeEventListener('pointercancel', this.boundPointerUp);

        const elapsed  = performance.now() - this.dragStartTime;
        const velocity = elapsed > 0 ? this.currentDeltaY / elapsed : 0;
        const fastFlickDown = velocity > VELOCITY_DISMISS;

        if (this.currentDeltaY < 0) {
            // ── Dragged UP ────────────────────────────────────────────────────
            // Expand if released past halfway between current height and maxHeight.
            const halfway = this.startHeight + (this.maxHeightPx - this.startHeight) / 2;
            const currentHeight = this.contentArea.offsetHeight;
            if (currentHeight >= halfway) {
                this.snapToExpanded();
            } else {
                this.snapToDefault();
            }
        } else if (this.state === 'expanded') {
            // ── Dragged DOWN from expanded ────────────────────────────────────
            // dragged > 50% of maxHeight down → dismiss; otherwise → restore to default size.
            if (fastFlickDown || this.currentDeltaY > this.maxHeightPx * 0.5) {
                this.slideOut().then(() => this.dotNetObj.invokeMethodAsync('HandleDismissed'));
            } else {
                // Still more than half visible → snap back to natural (default) height.
                this.snapToDefault();
            }
        } else {
            // ── Dragged DOWN from default ─────────────────────────────────────
            const dismiss120px = this.currentDeltaY > 120;
            if (dismiss120px || fastFlickDown) {
                this.slideOut().then(() => this.dotNetObj.invokeMethodAsync('HandleDismissed'));
            } else {
                this.snapToDefault();
            }
        }

        this.currentDeltaY = 0;
    }

    // ── snapping ──────────────────────────────────────────────────────────────

    private snapToDefault(): void {
        this.state = 'default';
        // Transition height back to natural. We can't transition to 'auto', so
        // transition to 0 then clear — but the element has content so it won't
        // actually collapse; instead set an explicit transition and let the browser
        // interpolate from the current px height down by clearing to ''.
        // Simplest reliable approach: transition transform, then on end clear height.
        this.setTransform('translateY(0)', `transform ${SNAP_DURATION}ms ${EASING_SNAP}, height ${SNAP_DURATION}ms ${EASING_SNAP}`);
        this.contentArea.style.height = '';
        setTimeout(() => this.clearTransition(), SNAP_DURATION);
    }

    private snapToExpanded(): void {
        this.state = 'expanded';
        // Resolve maxHeight to px so we can transition from the current px height.
        const targetPx = this.resolveMaxHeightPx();
        this.setTransform('translateY(0)', `transform ${SNAP_DURATION}ms ${EASING_SNAP}, height ${SNAP_DURATION}ms ${EASING_SNAP}`);
        this.contentArea.style.height = `${targetPx}px`;
        setTimeout(() => {
            this.clearTransition();
            // Switch to the CSS value so it stays correct if the viewport resizes.
            this.contentArea.style.height = this.maxHeight;
        }, SNAP_DURATION);
    }

    // ── public API ────────────────────────────────────────────────────────────

    public slideOut(): Promise<void> {
        this.clearTransition();
        this.contentArea.getBoundingClientRect(); // force reflow
        this.setTransform('translateY(100%)', `transform ${CLOSE_DURATION}ms ${EASING_CLOSE}`);
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

    /** Resolves the maxHeight CSS value (e.g. "90vh") to pixels at the current moment. */
    private resolveMaxHeightPx(): number {
        // Apply it temporarily to a zero-size element to let the browser compute it.
        const probe = document.createElement('div');
        probe.style.cssText = `position:fixed;visibility:hidden;height:${this.maxHeight};top:0`;
        document.body.appendChild(probe);
        const px = probe.offsetHeight;
        document.body.removeChild(probe);
        return px;
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

