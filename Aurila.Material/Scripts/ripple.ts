// Pointer-origin ripple, attached once at the document level.
//
// Aurila appearances can only contribute class names, never attributes, so targets are matched by
// the classes the controls already emit rather than by a data- opt-in.

const TARGET_SELECTOR = [
    ".au-clickable",
    ".au-chip",
    ".au-dropdown__item",
    ".au-calendar__day",
    ".au-calendar__item",
    ".au-checkbox__content",
    ".au-switch__content",
].join(",");

const CONTAINER_CLASS = "md-ripple";
const WAVE_CLASS = "md-ripple__wave";

const GROW_MS = 450;
const FADE_MS = 150;
const EASE = "cubic-bezier(0.2, 0, 0, 1)";

let attached = false;

function prefersReducedMotion(): boolean {
    return window.matchMedia?.("(prefers-reduced-motion: reduce)").matches ?? false;
}

function isDisabled(host: HTMLElement): boolean {
    return (
        host.classList.contains("au-clickable--disabled") ||
        host.classList.contains("au-chip--disabled") ||
        host.hasAttribute("disabled") ||
        host.getAttribute("aria-disabled") === "true"
    );
}

function getContainer(host: HTMLElement): HTMLElement {
    const existing = host.querySelector(`:scope > .${CONTAINER_CLASS}`);
    if (existing) {
        return existing as HTMLElement;
    }

    const container = document.createElement("span");
    container.className = CONTAINER_CLASS;
    container.setAttribute("aria-hidden", "true");
    host.appendChild(container);
    return container;
}

/** Radius that reaches the corner furthest from the press, so the wave always covers the host. */
function radiusFrom(rect: DOMRect, x: number, y: number): number {
    const dx = Math.max(x, rect.width - x);
    const dy = Math.max(y, rect.height - y);
    return Math.sqrt(dx * dx + dy * dy);
}

function spawn(host: HTMLElement, clientX: number, clientY: number): void {
    const rect = host.getBoundingClientRect();
    if (rect.width === 0 || rect.height === 0) {
        return;
    }

    const x = clientX - rect.left;
    const y = clientY - rect.top;
    const radius = radiusFrom(rect, x, y);

    const wave = document.createElement("span");
    wave.className = WAVE_CLASS;
    wave.style.left = `${x - radius}px`;
    wave.style.top = `${y - radius}px`;
    wave.style.width = `${radius * 2}px`;
    wave.style.height = `${radius * 2}px`;

    getContainer(host).appendChild(wave);

    const grow = wave.animate(
        [
            { transform: "scale(0)", opacity: "var(--md-sys-state-pressed-opacity, 0.1)" },
            { transform: "scale(1)", opacity: "var(--md-sys-state-pressed-opacity, 0.1)" },
        ],
        { duration: GROW_MS, easing: EASE, fill: "forwards" },
    );

    let released = false;

    const release = () => {
        if (released) {
            return;
        }
        released = true;

        window.removeEventListener("pointerup", release);
        window.removeEventListener("pointercancel", release);

        const fade = wave.animate([{ opacity: "var(--md-sys-state-pressed-opacity, 0.1)" }, { opacity: "0" }], {
            duration: FADE_MS,
            easing: "linear",
            fill: "forwards",
        });

        const cleanup = () => {
            grow.cancel();
            fade.cancel();
            wave.remove();
        };

        // Let the wave finish growing before it disappears, so quick taps still read as a ripple.
        Promise.all([grow.finished, fade.finished]).then(cleanup, cleanup);
    };

    window.addEventListener("pointerup", release);
    window.addEventListener("pointercancel", release);
}

export function enableRipple(): void {
    if (attached) {
        return;
    }
    attached = true;

    document.addEventListener(
        "pointerdown",
        (event: PointerEvent) => {
            if (event.button !== 0 || prefersReducedMotion()) {
                return;
            }

            const target = event.target as Element | null;
            const host = target?.closest?.(TARGET_SELECTOR) as HTMLElement | null;

            if (!host || isDisabled(host)) {
                return;
            }

            spawn(host, event.clientX, event.clientY);
        },
        { passive: true },
    );
}
