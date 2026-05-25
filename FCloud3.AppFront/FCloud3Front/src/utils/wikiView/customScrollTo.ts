const activeTimers = new WeakMap<HTMLElement, number>();

export function customScrollTo(container: HTMLElement, targetTop: number): void {
    const existingTimer = activeTimers.get(container);
    if (existingTimer !== undefined) {
        window.clearInterval(existingTimer);
    }

    const interval = 10;
    const baseInterval = 50;
    const speedRatio = baseInterval / interval;
    const minSpeed = 1;
    const speedFactor = 0.03 * speedRatio;

    const timer = window.setInterval(() => {
        const current = container.scrollTop;
        const distance = targetTop - current;
        if (Math.abs(distance) < 2) {
            container.scrollTop = targetTop;
            window.clearInterval(timer);
            activeTimers.delete(container);
            return;
        }
        const speed = Math.max(minSpeed, Math.abs(distance) * speedFactor);
        const direction = distance > 0 ? 1 : -1;
        container.scrollTop = current + direction * speed;
    }, interval);

    activeTimers.set(container, timer);
}
