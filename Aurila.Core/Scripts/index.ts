import { DotNetObject } from "./common";
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