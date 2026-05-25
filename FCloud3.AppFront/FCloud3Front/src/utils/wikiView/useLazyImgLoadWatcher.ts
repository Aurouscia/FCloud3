import { ref } from 'vue';

export function useLazyImgLoadWatcher(containerRef: () => HTMLElement | null | undefined) {
    const anyImgLoaded = ref(false);
    let observer: MutationObserver | null = null;
    let handledImgs = new WeakSet<HTMLImageElement>();

    function handleLoad() {
        console.log('检测到图片懒加载')
        anyImgLoaded.value = true;
    }

    function attachListeners(container: HTMLElement) {
        const imgs = container.querySelectorAll('img[loading="lazy"]');
        imgs.forEach(img => {
            const imgEl = img as HTMLImageElement;
            if (handledImgs.has(imgEl)) return;
            handledImgs.add(imgEl);
            imgEl.addEventListener('load', handleLoad, { once: true });
            imgEl.addEventListener('error', handleLoad, { once: true });
        });
    }

    function startWatching() {
        const container = containerRef();
        if (!container) return;

        attachListeners(container);

        observer = new MutationObserver(() => {
            attachListeners(container);
        });
        observer.observe(container, { childList: true, subtree: true });
    }

    function stopWatching() {
        observer?.disconnect();
        observer = null;
        anyImgLoaded.value = false;
        handledImgs = new WeakSet();
    }

    return { anyImgLoaded, startWatching, stopWatching };
}
