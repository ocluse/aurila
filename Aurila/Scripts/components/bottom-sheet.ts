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
    private startHeight   = 0;
    private maxHeightPx   = 0;

    // Touch handlers
    private boundTouchStart:  (e: TouchEvent)   => void;
    private boundTouchMove:   (e: TouchEvent)   => void;
    private boundTouchEnd:    (e: TouchEvent)   => void;

    // Mouse/pointer handlers (document-level, attached only during drag)
    private boundPointerDown:   (e: PointerEvent) => void;
    private boundDocPointerMove: (e: PointerEvent) => void;
    private boundDocPointerUp:   (e: PointerEvent) => void;

    // Prevent synthetic pointer events from double-firing after a touch gesture
    private touchActive = false;

    constructor(contentArea: HTMLElement, dotNetObj: DotNetObject, maxHeight: string) {
        this.contentArea = contentArea;
        this.dotNetObj   = dotNetObj;
        this.maxHeight   = maxHeight;

        this.boundTouchStart      = this.handleTouchStart.bind(this);
        this.boundTouchMove       = this.handleTouchMove.bind(this);
        this.boundTouchEnd        = this.handleTouchEnd.bind(this);
        this.boundPointerDown     = this.handlePointerDown.bind(this);
        this.boundDocPointerMove  = this.handleDocPointerMove.bind(this);
        this.boundDocPointerUp    = this.handleDocPointerUp.bind(this);

        // Touch events — on the element
        this.contentArea.addEventListener('touchstart',  this.boundTouchStart,  { passive: false });
        this.contentArea.addEventListener('touchmove',   this.boundTouchMove,   { passive: false });
        this.contentArea.addEventListener('touchend',    this.boundTouchEnd);
        this.contentArea.addEventListener('touchcancel', this.boundTouchEnd);

        // Pointer events — pointerdown on element; move/up on document during drag
        this.contentArea.addEventListener('pointerdown', this.boundPointerDown);

        // Open animation
        this.setTransform('translateY(100%)', '');
        this.contentArea.getBoundingClientRect();
        this.setTransform('translateY(0)', `transform ${OPEN_DURATION}ms ${EASING_OPEN}`);
        setTimeout(() => this.clearTransition(), OPEN_DURATION);
    }

    // ── shared drag core ──────────────────────────────────────────────────────

    private startDrag(clientY: number): void {
        const scrollable = this.findScrollable();
        if (this.state === 'default' && scrollable && scrollable.scrollTop > 0) return;

        this.isDragging    = true;
        this.dragStartY    = clientY;
        this.dragStartTime = performance.now();
        this.currentDeltaY = 0;
        this.startHeight   = this.contentArea.offsetHeight;
        this.maxHeightPx   = this.resolveMaxHeightPx();

        this.clearTransition();
    }

    private moveDrag(clientY: number): void {
        if (!this.isDragging) return;

        const deltaY = clientY - this.dragStartY;

        if (deltaY > 0) {
            this.contentArea.style.height = `${this.startHeight}px`;
            this.setTransform(`translateY(${deltaY}px)`, '');
        } else {
            this.setTransform('translateY(0)', '');
            const newHeight = this.startHeight - deltaY;
            this.contentArea.style.height = `min(${newHeight}px, ${this.maxHeight})`;
        }

        this.currentDeltaY = deltaY;
    }

    private endDrag(): void {
        if (!this.isDragging) return;
        this.isDragging = false;

        const elapsed      = performance.now() - this.dragStartTime;
        const velocity     = elapsed > 0 ? this.currentDeltaY / elapsed : 0;
        const fastFlickDown = velocity > VELOCITY_DISMISS;

        if (this.currentDeltaY < 0) {
            // Dragged UP — expand if past halfway between start height and max
            const halfway       = this.startHeight + (this.maxHeightPx - this.startHeight) / 2;
            const currentHeight = this.contentArea.offsetHeight;
            if (currentHeight >= halfway) {
                this.snapToExpanded();
            } else {
                this.snapToDefault();
            }
        } else if (this.state === 'expanded') {
            // Dragged DOWN from expanded
            // > 50% of maxHeight dragged → dismiss; otherwise → restore to default
            if (fastFlickDown || this.currentDeltaY > this.maxHeightPx * 0.5) {
                this.slideOut().then(() => this.dotNetObj.invokeMethodAsync('HandleDismissed'));
            } else {
                this.snapToDefault();
            }
        } else {
            // Dragged DOWN from default
            if (fastFlickDown || this.currentDeltaY > 120) {
                this.slideOut().then(() => this.dotNetObj.invokeMethodAsync('HandleDismissed'));
            } else {
                this.snapToDefault();
            }
        }

        this.currentDeltaY = 0;
    }

    // ── touch handlers ────────────────────────────────────────────────────────

    private handleTouchStart(e: TouchEvent): void {
        if (e.touches.length !== 1) return;
        this.touchActive = true;
        // Prevent scroll and suppress the synthetic pointer events the browser
        // fires after touch events, which would cause double-handling.
        e.preventDefault();
        this.startDrag(e.touches[0].clientY);
    }

    private handleTouchMove(e: TouchEvent): void {
        if (!this.isDragging || e.touches.length !== 1) return;
        e.preventDefault();
        this.moveDrag(e.touches[0].clientY);
    }

    private handleTouchEnd(e: TouchEvent): void {
        this.touchActive = false;
        this.endDrag();
    }

    // ── pointer (mouse) handlers ──────────────────────────────────────────────

    private handlePointerDown(e: PointerEvent): void {
        // Ignore if a touch gesture is already active (synthetic pointer after touch)
        if (this.touchActive) return;
        if (!e.isPrimary || e.pointerType === 'touch') return;

        this.startDrag(e.clientY);

        if (this.isDragging) {
            // Attach move/up to document so dragging outside the element still works
            document.addEventListener('pointermove', this.boundDocPointerMove);
            document.addEventListener('pointerup',   this.boundDocPointerUp);
            document.addEventListener('pointercancel', this.boundDocPointerUp);
        }
    }

    private handleDocPointerMove(e: PointerEvent): void {
        if (!e.isPrimary) return;
        this.moveDrag(e.clientY);
    }

    private handleDocPointerUp(e: PointerEvent): void {
        if (!e.isPrimary) return;
        document.removeEventListener('pointermove',   this.boundDocPointerMove);
        document.removeEventListener('pointerup',     this.boundDocPointerUp);
        document.removeEventListener('pointercancel', this.boundDocPointerUp);
        this.endDrag();
    }

    // ── snapping ──────────────────────────────────────────────────────────────

    private snapToDefault(): void {
        this.state = 'default';
        this.setTransform('translateY(0)', `transform ${SNAP_DURATION}ms ${EASING_SNAP}, height ${SNAP_DURATION}ms ${EASING_SNAP}`);
        this.contentArea.style.height = '';
        setTimeout(() => this.clearTransition(), SNAP_DURATION);
    }

    private snapToExpanded(): void {
        this.state = 'expanded';
        const targetPx = this.resolveMaxHeightPx();
        this.setTransform('translateY(0)', `transform ${SNAP_DURATION}ms ${EASING_SNAP}, height ${SNAP_DURATION}ms ${EASING_SNAP}`);
        this.contentArea.style.height = `${targetPx}px`;
        setTimeout(() => {
            this.clearTransition();
            this.contentArea.style.height = this.maxHeight;
        }, SNAP_DURATION);
    }

    // ── public API ────────────────────────────────────────────────────────────

    public slideOut(): Promise<void> {
        this.clearTransition();
        this.contentArea.getBoundingClientRect();
        this.setTransform('translateY(100%)', `transform ${CLOSE_DURATION}ms ${EASING_CLOSE}`);
        this.contentArea.style.height = '';
        return new Promise(resolve => setTimeout(resolve, CLOSE_DURATION));
    }

    public dispose(): void {
        this.contentArea.removeEventListener('touchstart',  this.boundTouchStart);
        this.contentArea.removeEventListener('touchmove',   this.boundTouchMove);
        this.contentArea.removeEventListener('touchend',    this.boundTouchEnd);
        this.contentArea.removeEventListener('touchcancel', this.boundTouchEnd);
        this.contentArea.removeEventListener('pointerdown', this.boundPointerDown);
        document.removeEventListener('pointermove',   this.boundDocPointerMove);
        document.removeEventListener('pointerup',     this.boundDocPointerUp);
        document.removeEventListener('pointercancel', this.boundDocPointerUp);
        this.contentArea.style.transform  = '';
        this.contentArea.style.height     = '';
        this.contentArea.style.transition = '';
        this.isDragging   = false;
        this.touchActive  = false;
    }

    // ── helpers ───────────────────────────────────────────────────────────────

    private setTransform(value: string, transition: string): void {
        this.contentArea.style.transition = transition;
        this.contentArea.style.transform  = value;
    }

    private clearTransition(): void {
        this.contentArea.style.transition = '';
    }

    private resolveMaxHeightPx(): number {
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
