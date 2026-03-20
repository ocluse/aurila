import { DotNetObject } from "../common";

export class TextField {
    private element: HTMLTextAreaElement | null;
    private maxLines: number;
    private boundAdjustHeight: () => void;
    private dotNetObjRef: DotNetObject | null;
    private isComposing: boolean = false;
    private lastAppliedVersion: number = 0;
    private boundInputHandler: () => void;
    private boundFocusHandler: () => void;
    private boundBlurHandler: () => void;
    private boundCompositionStartHandler: () => void;
    private boundCompositionEndHandler: () => void;

    constructor(element: HTMLTextAreaElement, maxLines: number, dotNetObjRef: DotNetObject, initialValue: string) {
        this.element = element;
        this.maxLines = maxLines;
        this.dotNetObjRef = dotNetObjRef;

        this.boundAdjustHeight = this.adjustHeight.bind(this);
        this.boundInputHandler = this.handleInput.bind(this);
        this.boundFocusHandler = this.handleFocus.bind(this);
        this.boundBlurHandler = this.handleBlur.bind(this);
        this.boundCompositionStartHandler = this.handleCompositionStart.bind(this);
        this.boundCompositionEndHandler = this.handleCompositionEnd.bind(this);

        this.element.value = initialValue ?? '';
        this.element.addEventListener('input', this.boundAdjustHeight);
        this.element.addEventListener('input', this.boundInputHandler);
        this.element.addEventListener('focus', this.boundFocusHandler);
        this.element.addEventListener('blur', this.boundBlurHandler);
        this.element.addEventListener('compositionstart', this.boundCompositionStartHandler);
        this.element.addEventListener('compositionend', this.boundCompositionEndHandler);
        this.adjustHeight();
    }

    private handleInput(): void {
        if (!this.element || !this.dotNetObjRef) return;

        this.dotNetObjRef.invokeMethodAsync(
            'HandleInputFromJS',
            this.element.value,
            this.element.selectionStart,
            this.element.selectionEnd,
            this.isComposing
        );
    }

    private handleFocus(): void {
        if (!this.dotNetObjRef) return;
        this.dotNetObjRef.invokeMethodAsync('HandleFocusFromJS');
    }

    private handleBlur(): void {
        if (!this.dotNetObjRef) return;
        this.dotNetObjRef.invokeMethodAsync('HandleBlurFromJS');
    }

    private handleCompositionStart(): void {
        this.isComposing = true;
    }

    private handleCompositionEnd(): void {
        this.isComposing = false;
        this.handleInput();
    }

    private adjustHeight(): void {
        if (!this.element) return;

        const maxHeight = this.getLineHeight() * this.maxLines;

        this.element.style.height = 'auto';
        const newHeight = Math.min(this.element.scrollHeight, maxHeight);

        this.element.style.height = `${newHeight}px`;
        this.element.style.overflowY = newHeight >= maxHeight ? 'auto' : 'hidden';
    }

    public setMaxLines(maxLines: number): void {
        this.maxLines = maxLines;
        this.adjustHeight();
    }

    public applyExternalValue(value: string, version: number): void {
        if (!this.element) return;
        if (version <= this.lastAppliedVersion) return;

        if (this.element.value !== value) {
            this.element.value = value;
            this.adjustHeight();
        }

        this.lastAppliedVersion = version;
    }

    private getLineHeight(): number {
        if (!this.element) return 24;
        const style = getComputedStyle(this.element);
        return parseFloat(style.lineHeight) || 24;
    }

    public dispose(): void {
        if (this.element) {
            this.element.removeEventListener('input', this.boundAdjustHeight);
            this.element.removeEventListener('input', this.boundInputHandler);
            this.element.removeEventListener('focus', this.boundFocusHandler);
            this.element.removeEventListener('blur', this.boundBlurHandler);
            this.element.removeEventListener('compositionstart', this.boundCompositionStartHandler);
            this.element.removeEventListener('compositionend', this.boundCompositionEndHandler);
            this.element = null;
        }

        this.dotNetObjRef = null;
    }
}
