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

    constructor(dialog: HTMLElement) {
        this.sheet = dialog.getElementsByClassName("au-modal__content-area")[0] as HTMLElement;
        this.sheet.style.setProperty('--translate-y', '100%');
    }

    public open() {
        //set var --translate-y to 50%:
        console.log("Opening bottom sheet");
        requestAnimationFrame(() => {
        requestAnimationFrame(() => {
            this.sheet.style.setProperty('--translate-y', '50%');
        });
    });
    }

    public close() {
        console.log("Closing bottom sheet");
        this.sheet.style.setProperty('--translate-y', '100%');
    }

    public dispose() {

    }
}
