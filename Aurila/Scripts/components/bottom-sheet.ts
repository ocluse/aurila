import { DotNetObject } from "../common";

const OPEN_DURATION = 300;  // ms
const CLOSE_DURATION = 300;  // ms
const SNAP_DURATION = 300;  // ms

const VELOCITY_DISMISS = 0.4; // px/ms — fast flick down dismisses regardless of distance

const EASING_OPEN = 'cubic-bezier(0.2, 0, 0, 1)';
const EASING_CLOSE = 'cubic-bezier(0.4, 0, 1, 1)';
const EASING_SNAP = 'cubic-bezier(0.2, 0, 0, 1)';

type SheetState = 'default' | 'expanded';

export class BottomSheet {
    private sheet: HTMLElement;
    private dotNetRef: DotNetObject;
    private dragStartY: number = 0;
    private dragStartTranslate: number = 0;
    private isDragging: boolean = false;

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
                this.setTranslate(50, true);
            });
        });
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
        this.dragStartY = this.getClientY(e);
        this.dragStartTranslate = this.getCurrentTranslatePercent();

        // Disable transition during drag for immediate feedback
        this.sheet.style.transition = 'none';

        document.addEventListener('mousemove', this.onDragMove);
        document.addEventListener('touchmove', this.onDragMove, { passive: false });
        document.addEventListener('mouseup', this.onDragEnd);
        document.addEventListener('touchend', this.onDragEnd);
    }

    private handleDragMove(e: MouseEvent | TouchEvent) {
        if (!this.isDragging) return;

        if (e.type === 'touchmove') e.preventDefault();

        const deltaY = this.getClientY(e) - this.dragStartY;
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

        // Re-enable transition for snap
        this.sheet.style.transition = '';

        const current = this.getCurrentTranslatePercent();

        if (current < 25) {
            // Top quarter — snap fully open
            this.setTranslate(0, true);
        } else if (current < 75) {
            // Middle zone — snap to half-open
            this.setTranslate(50, true);
        } else {
            // Bottom quarter — close
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

    private getCurrentTranslatePercent(): number {
        const raw = this.sheet.style.getPropertyValue('--translate-y');
        return parseFloat(raw) || 0;
    }

    private getClientY(e: MouseEvent | TouchEvent): number {
        return e instanceof TouchEvent ? e.touches[0].clientY : e.clientY;
    }
}
