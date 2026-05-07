import { DotNetObject } from "../common";

export class HorizontalPager {
    private element: HTMLElement;
    private dotNetObject: DotNetObject;
    private observer: IntersectionObserver;

    constructor(element: HTMLElement, dotNetObject: DotNetObject) {
        this.element = element;
        this.dotNetObject = dotNetObject;

        const options = {
            root: this.element,
            rootMargin: '0px',
            threshold: 0.51 
        };

        this.observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    const indexStr = entry.target.getAttribute('data-page-index');
                    if (indexStr) {
                        const newIndex = parseInt(indexStr, 10);
                        this.dotNetObject.invokeMethodAsync('OnPageScrolledIntoView', newIndex);
                    }
                }
            });
        }, options);

        this.observeChildren();
    }

    public observeChildren() {
        this.observer.disconnect();
        Array.from(this.element.children).forEach(child => {
            if (child.classList.contains('au-pager__page')) {
                this.observer.observe(child);
            }
        });
    }

    public scrollToPage(index: number) {
        const targetPage = this.element.querySelector(`[data-page-index="${index}"]`) as HTMLElement;
        if (targetPage) {
            // Calculate exactly where the child is relative to the current scroll position
            const targetRect = targetPage.getBoundingClientRect();
            const containerRect = this.element.getBoundingClientRect();

            // Current scroll + distance the child is from the left edge of the container
            const scrollTarget = this.element.scrollLeft + (targetRect.left - containerRect.left);

            // Scroll ONLY this specific container
            this.element.scrollTo({
                left: scrollTarget,
                behavior: 'smooth'
            });
        }
    }

    public dispose() {
        if (this.observer) {
            this.observer.disconnect();
        }
    }
}
