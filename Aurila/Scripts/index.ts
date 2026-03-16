import { DotNetObject, ScrollValues } from "./common";
import { ScrollOrientation } from "./enums";
import { ScrollBox } from "./components/scroll-box";
import { TextField } from "./components/text-field"; 
import { PullToRefreshBox } from "./components/pull-to-refresh-box";

export function createScrollBox(
    element: HTMLElement,
    dotNetObject: DotNetObject,
    throttleMs: number,
    orientation: ScrollOrientation
): ScrollBox {
    return new ScrollBox(element, dotNetObject, throttleMs, orientation);
}

export function createPullToRefreshBox(
    contentElement: HTMLElement,
    dotNetObject: DotNetObject,
): PullToRefreshBox {
    return new PullToRefreshBox(contentElement, dotNetObject);
}

export function createTextField(
    element: HTMLTextAreaElement,
    maxLines: number
): TextField {
    return new TextField(element, maxLines);
}

export function showDialog(dialog: HTMLDialogElement) {
    dialog.showModal();
}

export function closeDialog(dialog: HTMLDialogElement) {
    dialog.close();
}

export function showPopover(popover: HTMLDialogElement) {
    popover.showPopover();
}

export function hidePopover(popover: HTMLDialogElement) {
    popover.hidePopover();
}

export function isNearBottom(element: HTMLElement, threshold: number): boolean {
    if (!element) return false;

    const distanceToBottom = element.scrollHeight - (element.scrollTop + element.clientHeight);
    return distanceToBottom < threshold;
}

export function scrollToBottom(element: HTMLElement): void {
    if (!element) return;
    element.scrollTop = element.scrollHeight;
}

export function scrollToTop(element: HTMLElement): void {
    if (!element) return;
    element.scrollTop = 0;
}

export function scrollToPosition(element: HTMLElement, position: number, isVertical: boolean) {
    if (!element) return;
    if (isVertical) {
        element.scrollTop = position;
    } else {
        element.scrollLeft = position;
    }
}

export function getScrollValues(element: HTMLElement): ScrollValues {
    if (!element) return {
        scrollTop: 0,
        scrollLeft: 0,
        scrollHeight: 0,
        scrollWidth: 0,
        clientHeight: 0,
        clientWidth: 0
    };
    return {
        scrollTop: element.scrollTop,
        scrollLeft: element.scrollLeft,
        scrollHeight: element.scrollHeight,
        scrollWidth: element.scrollWidth,
        clientHeight: element.clientHeight,
        clientWidth: element.clientWidth
    };
}