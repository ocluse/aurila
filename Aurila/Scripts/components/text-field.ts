export class TextField {
    private element: HTMLTextAreaElement | null;
    private maxLines: number;
    private boundAdjustHeight: () => void;

    constructor(element: HTMLTextAreaElement, maxLines: number) {
        this.element = element;
        this.maxLines = maxLines;

        this.boundAdjustHeight = this.adjustHeight.bind(this);
        this.element.addEventListener('input', this.boundAdjustHeight);
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

    private getLineHeight(): number {
        if (!this.element) return 24;
        const style = getComputedStyle(this.element);
        return parseFloat(style.lineHeight) || 24;
    }

    public dispose(): void {
        if (this.element) {
            this.element.removeEventListener('input', this.boundAdjustHeight);
            this.element = null;
        }
    }
}
