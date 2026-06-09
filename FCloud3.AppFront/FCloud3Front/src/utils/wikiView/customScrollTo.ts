const activeTimers = new WeakMap<HTMLElement, number>();

export function customScrollTo(container: HTMLElement, targetTop: number): void {
    const existingTimer = activeTimers.get(container);
    if (existingTimer !== undefined) {
        window.clearInterval(existingTimer);
        activeTimers.delete(container);
    }

    const interval = 10;
    const baseInterval = 50;
    const speedRatio = baseInterval / interval;
    const minSpeed = 1;
    const speedFactor = 0.03 * speedRatio;

    let lastScrollTop = container.scrollTop;
    let stuckCount = 0;

    const timer = window.setInterval(() => {
        const current = container.scrollTop;
        const distance = targetTop - current;
        if (Math.abs(distance) < 2) {
            container.scrollTop = targetTop;
            window.clearInterval(timer);
            activeTimers.delete(container);
            return;
        }

        if (current === lastScrollTop) {
            stuckCount++;
            if (stuckCount >= 3) {
                window.clearInterval(timer);
                activeTimers.delete(container);
                return;
            }
        } else {
            stuckCount = 0;
        }
        lastScrollTop = current;

        const speed = Math.max(minSpeed, Math.abs(distance) * speedFactor);
        const direction = distance > 0 ? 1 : -1;
        container.scrollTop = current + direction * speed;
    }, interval);

    activeTimers.set(container, timer);
}
