import DOMPurify from 'dompurify';

/**
 * 允许嵌入的 iframe 来源白名单
 * 注意：修改后应同步检查后端的 HtmlAreaExtractor/HtmlSanitizer 配置
 */
const allowedIframeOrigins: string[] = [
    'https://www.youtube.com',
    'https://www.youtube-nocookie.com',
    'https://player.bilibili.com',
    'https://www.bilibili.com',
    'https://music.163.com',
];

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

let hooksRegistered = false;

function ensureHooks(): void {
    if (hooksRegistered) return;
    hooksRegistered = true;

    DOMPurify.addHook('uponSanitizeAttribute', (node, data) => {
        if (!(node instanceof HTMLIFrameElement) || data.attrName !== 'src') {
            return;
        }
        if (!isAllowedIframeSrc(data.attrValue)) {
            data.keepAttr = false;
        }
    });

    DOMPurify.addHook('afterSanitizeAttributes', (node) => {
        if (!(node instanceof HTMLIFrameElement)) {
            return;
        }
        if (!node.getAttribute('src')) {
            node.remove();
            return;
        }
        node.setAttribute('sandbox', defaultIframeSandbox);
        node.setAttribute('allow', defaultIframeAllow);
        node.setAttribute('referrerpolicy', 'no-referrer');
        node.setAttribute('loading', 'lazy');
        node.removeAttribute('allowfullscreen');
        node.removeAttribute('frameborder');
    });
}

/**
 * 净化 wiki 渲染用的 HTML
 * - 移除 <script>
 * - 只允许 iframe 加载白名单内的 src
 * - 为 iframe 强制添加 sandbox、allow、referrerpolicy、loading 等安全属性
 * - 移除危险事件处理器和 srcdoc
 */
export function sanitizeWikiHtml(dirty: string | undefined | null): string {
    if (!dirty) return '';
    ensureHooks();

    return DOMPurify.sanitize(dirty, {
        ADD_TAGS: ['iframe'],
        ADD_ATTR: ['src', 'sandbox', 'allow', 'referrerpolicy', 'loading', 'frameborder', 'allowfullscreen'],
        FORBID_TAGS: ['script'],
        FORBID_ATTR: [
            'onerror', 'onload', 'onclick', 'onmouseover', 'onmouseenter',
            'onmouseleave', 'onfocus', 'onblur', 'srcdoc'
        ],
        ALLOW_DATA_ATTR: false,
        RETURN_TRUSTED_TYPE: false,
    });
}
