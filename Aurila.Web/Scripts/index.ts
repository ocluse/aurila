export function openLinkInNewTab(url: string): void {
    if (!url || url.trim().length === 0) {
        return;
    }

    window.open(url, "_blank");
}
