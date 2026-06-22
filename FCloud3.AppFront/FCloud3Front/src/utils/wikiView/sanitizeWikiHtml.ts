/**
 * 允许嵌入的 iframe 来源白名单
 * 注意：修改后应同步检查后端的 HtmlAreaExtractor/HtmlSanitizer 配置
 */
const allowedIframeOrigins: string[] = (import.meta.env.VITE_IframeWhitelist as string | undefined)
    ?.split('\n')
    .map(line => line.trim())
    .filter(line => line.length > 0 && !line.startsWith('#'))
    .map(line => {
        const eqIdx = line.indexOf('=');
        return eqIdx > 0 ? line.substring(eqIdx + 1).trim() : line;
    })
    .filter(line => line.startsWith('https://') || line.startsWith('http://'))
    ?? [];

const defaultIframeSandbox = 'allow-scripts';
const defaultIframeAllow = "fullscreen 'none'; camera 'none'; microphone 'none'; geolocation 'none'; payment 'none'; autoplay 'none'";

function isAllowedIframeSrc(src: string | null): boolean {
    if (!src) return false;
    if (!src.startsWith('https://') && !src.startsWith('http://')) return false;
    try {
        const url = new URL(src);
        return allowedIframeOrigins.some(origin =>
            url.origin.toLowerCase() === origin.toLowerCase()
        );
    } catch {
        return false;
    }
}

function sanitizeIframe(iframe: HTMLIFrameElement): void {
    if (!isAllowedIframeSrc(iframe.getAttribute('src'))) {
        iframe.remove();
        return;
    }
    iframe.setAttribute('sandbox', defaultIframeSandbox);
    iframe.setAttribute('allow', defaultIframeAllow);
    iframe.setAttribute('referrerpolicy', 'no-referrer');
    iframe.setAttribute('loading', 'lazy');
    iframe.removeAttribute('allowfullscreen');
    iframe.removeAttribute('frameborder');
}

/**
 * 净化 wiki 渲染用的 HTML
 * 后端 WikiPreprocessor 已进行过 HTML 净化（HtmlSanitizer + HtmlAreaExtractor），
 * 此处仅负责 iframe 的 src 白名单校验和 sandbox 等安全属性的强制添加。
 */
export function sanitizeWikiHtml(dirty: string | undefined | null): string {
    if (!dirty) return '';

    const parser = new DOMParser();
    const doc = parser.parseFromString(dirty, 'text/html');

    const iframes = doc.querySelectorAll('iframe');
    iframes.forEach(sanitizeIframe);

    return doc.body.innerHTML;
}
