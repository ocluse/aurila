import { DotNetObject } from "../common";

export class TextField {
    private element: HTMLTextAreaElement | null;
    private minLines: number;
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
    private boundKeyDownHandler: (event: KeyboardEvent) => void;
    private boundBeforeInputHandler: (event: InputEvent) => void;
    private enterBehavior: string;
    private enterModifier: string;
    private virtualEnterBehavior: string;
    private canSubmit: boolean;
    private submitPending: boolean = false;
    private lastEnterKeyDownAction: 'submit' | 'newline' | null = null;
    private lastEnterKeyDownAt: number = 0;

    constructor(
        element: HTMLTextAreaElement,
        maxLines: number,
        minLines: number,
        dotNetObjRef: DotNetObject,
        initialValue: string,
        enterBehavior: string,
        enterModifier: string,
        virtualEnterBehavior: string,
        canSubmit: boolean
    ) {
        this.element = element;
        this.minLines = minLines;
        this.maxLines = maxLines;
        this.dotNetObjRef = dotNetObjRef;
        this.enterBehavior = enterBehavior;
        this.enterModifier = enterModifier;
        this.virtualEnterBehavior = virtualEnterBehavior;
        this.canSubmit = canSubmit;

        this.boundAdjustHeight = this.adjustHeight.bind(this);
        this.boundInputHandler = this.handleInput.bind(this);
        this.boundFocusHandler = this.handleFocus.bind(this);
        this.boundBlurHandler = this.handleBlur.bind(this);
        this.boundCompositionStartHandler = this.handleCompositionStart.bind(this);
        this.boundCompositionEndHandler = this.handleCompositionEnd.bind(this);
        this.boundKeyDownHandler = this.handleKeyDown.bind(this);
        this.boundBeforeInputHandler = this.handleBeforeInput.bind(this);

        this.element.value = initialValue ?? '';
        this.element.addEventListener('input', this.boundAdjustHeight);
        this.element.addEventListener('input', this.boundInputHandler);
        this.element.addEventListener('focus', this.boundFocusHandler);
        this.element.addEventListener('blur', this.boundBlurHandler);
        this.element.addEventListener('compositionstart', this.boundCompositionStartHandler);
        this.element.addEventListener('compositionend', this.boundCompositionEndHandler);
        this.element.addEventListener('keydown', this.boundKeyDownHandler);
        this.element.addEventListener('beforeinput', this.boundBeforeInputHandler);
        this.adjustHeight();
    }

    private handleKeyDown(event: KeyboardEvent): void {
        if (!this.element || !this.canSubmit) return;
        if (event.key !== 'Enter' && event.code !== 'NumpadEnter') return;
        if (event.isComposing || this.isComposing) return;

        const modifierPressed = this.isConfiguredModifierPressed(event);
        const shouldSubmit = this.enterBehavior === 'SubmitUnlessModified'
            ? !modifierPressed
            : this.enterBehavior === 'SubmitWhenModified' && modifierPressed;

        this.lastEnterKeyDownAction = shouldSubmit ? 'submit' : 'newline';
        this.lastEnterKeyDownAt = performance.now();

        if (!shouldSubmit) return;

        event.preventDefault();

        if (!event.repeat) {
            this.submitCurrentValue();
        }
    }

    private handleBeforeInput(event: InputEvent): void {
        if (!this.element || !this.canSubmit) return;
        if (event.inputType !== 'insertLineBreak' && event.inputType !== 'insertParagraph') return;
        if (event.isComposing || this.isComposing) return;

        const followedKeyDown = performance.now() - this.lastEnterKeyDownAt < 500
            ? this.lastEnterKeyDownAction
            : null;

        this.lastEnterKeyDownAction = null;
        this.lastEnterKeyDownAt = 0;

        if (followedKeyDown === 'newline') {
            return;
        }

        if (followedKeyDown === 'submit') {
            event.preventDefault();
            return;
        }

        const shouldSubmit = this.virtualEnterBehavior === 'Submit'
            || (this.virtualEnterBehavior === 'FollowUnmodifiedEnter'
                && this.enterBehavior === 'SubmitUnlessModified');

        if (!shouldSubmit) return;

        event.preventDefault();
        this.submitCurrentValue();
    }

    private isConfiguredModifierPressed(event: KeyboardEvent): boolean {
        switch (this.enterModifier) {
            case 'Control': return event.ctrlKey;
            case 'Alt': return event.altKey;
            case 'Meta': return event.metaKey;
            case 'ControlOrMeta': return event.ctrlKey || event.metaKey;
            case 'Shift':
            default:
                return event.shiftKey;
        }
    }

    private submitCurrentValue(): void {
        if (!this.element || !this.dotNetObjRef || this.submitPending) return;

        this.submitPending = true;
        this.dotNetObjRef.invokeMethodAsync('HandleSubmitFromJS', this.element.value)
            .finally(() => this.submitPending = false);
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

        const minHeight = this.getLineHeight() * this.minLines;
        const maxHeight = this.getLineHeight() * this.maxLines;

        this.element.style.height = 'auto';
        const newHeight = Math.max(minHeight, Math.min(this.element.scrollHeight, maxHeight));

        this.element.style.height = `${newHeight}px`;
        this.element.style.overflowY = newHeight >= maxHeight && this.element.scrollHeight > maxHeight ? 'auto' : 'hidden';
    }

    public setLineBounds(minLines: number, maxLines: number): void {
        this.minLines = minLines;
        this.maxLines = maxLines;
        this.adjustHeight();
    }

    public setEnterOptions(
        enterBehavior: string,
        enterModifier: string,
        virtualEnterBehavior: string,
        canSubmit: boolean
    ): void {
        this.enterBehavior = enterBehavior;
        this.enterModifier = enterModifier;
        this.virtualEnterBehavior = virtualEnterBehavior;
        this.canSubmit = canSubmit;
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
            this.element.removeEventListener('keydown', this.boundKeyDownHandler);
            this.element.removeEventListener('beforeinput', this.boundBeforeInputHandler);
            this.element = null;
        }

        this.dotNetObjRef = null;
    }
}
