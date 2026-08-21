export class AccessibleButton {
    private element: HTMLElement | null = null;
    private spacePressed = false;
    private readonly boundKeyDown: (event: KeyboardEvent) => void;
    private readonly boundKeyUp: (event: KeyboardEvent) => void;
    private readonly boundBlur: () => void;

    constructor(element: HTMLElement) {
        this.boundKeyDown = this.handleKeyDown.bind(this);
        this.boundKeyUp = this.handleKeyUp.bind(this);
        this.boundBlur = this.handleBlur.bind(this);
        this.setElement(element);
    }

    public setElement(element: HTMLElement | null): void {
        if (this.element === element) return;

        this.detach();
        this.element = element;

        if (this.element) {
            this.element.addEventListener('keydown', this.boundKeyDown);
            this.element.addEventListener('keyup', this.boundKeyUp);
            this.element.addEventListener('blur', this.boundBlur);
        }
    }

    private handleKeyDown(event: KeyboardEvent): void {
        if (!this.element || event.target !== this.element || event.defaultPrevented) return;

        if (event.key === 'Enter') {
            event.preventDefault();

            if (!event.repeat) {
                this.element.click();
            }
        } else if (this.element.getAttribute('role') === 'button'
            && (event.key === ' ' || event.key === 'Spacebar')) {
            event.preventDefault();

            if (!event.repeat) {
                this.spacePressed = true;
            }
        }
    }

    private handleKeyUp(event: KeyboardEvent): void {
        if (!this.element || event.target !== this.element) return;
        if (this.element.getAttribute('role') !== 'button') return;
        if (event.key !== ' ' && event.key !== 'Spacebar') return;

        event.preventDefault();

        if (this.spacePressed) {
            this.spacePressed = false;
            this.element.click();
        }
    }

    private handleBlur(): void {
        this.spacePressed = false;
    }

    private detach(): void {
        if (!this.element) return;

        this.element.removeEventListener('keydown', this.boundKeyDown);
        this.element.removeEventListener('keyup', this.boundKeyUp);
        this.element.removeEventListener('blur', this.boundBlur);
        this.spacePressed = false;
    }

    public dispose(): void {
        this.detach();
        this.element = null;
    }
}
