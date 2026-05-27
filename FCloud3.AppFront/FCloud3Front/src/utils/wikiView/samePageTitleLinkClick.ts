const targetTagName = 'A';
const titleQueryKey = 'title';

export class SamePageTitleLinkClick {
    currentPathWithoutQuery: string;
    onTitleClick: (titleText: string) => void;

    constructor(currentPathWithoutQuery: string, onTitleClick: (titleText: string) => void) {
        this.currentPathWithoutQuery = currentPathWithoutQuery;
        this.onTitleClick = onTitleClick;
        this.clickHandler = this.clickHandler.bind(this);
    }

    listen(target?: HTMLDivElement | null) {
        if (!target) return;
        const links = target.getElementsByTagName(targetTagName);
        let converted = 0;
        for (const link of links) {
            const anchor = link as HTMLAnchorElement;
            const titleText = extractTitleFromHref(anchor.href, this.currentPathWithoutQuery);
            if (titleText !== null) {
                anchor.addEventListener('click', this.clickHandler);
                anchor.dataset['samePageTitle'] = titleText;
                converted++;
            }
        }
        console.log(`转化 ${converted} 个同页标题链接`);
    }

    clickHandler(e: MouseEvent) {
        const ele = e.currentTarget as HTMLAnchorElement;
        const titleText = ele.dataset['samePageTitle'];
        if (titleText) {
            e.preventDefault(); // 不再有 a 标签自带的“router.push”效果
            this.onTitleClick(titleText);
        }
    }
}

function stripProtocol(url: string): string {
    return url.replace(/^[^:]+:\/\//, '');
}

function extractTitleFromHref(href: string, currentPathWithoutQuery: string): string | null {
    try {
        const url = new URL(href, window.location.origin);
        const pathWithoutQuery = url.href.split('?')[0];
        console.log('[SamePageTitleLinkClick] href:', href);
        console.log('[SamePageTitleLinkClick] pathWithoutQuery:', pathWithoutQuery);
        console.log('[SamePageTitleLinkClick] currentPathWithoutQuery:', currentPathWithoutQuery);
        if (stripProtocol(pathWithoutQuery) !== stripProtocol(currentPathWithoutQuery)) {
            console.log('[SamePageTitleLinkClick] 路径不匹配，跳过');
            return null;
        }
        const queryString = url.href.includes('?') ? url.href.split('?')[1] : '';
        const params = new URLSearchParams(queryString);
        const title = params.get(titleQueryKey);
        console.log('[SamePageTitleLinkClick] title:', title);
        if (!title) {
            console.log('[SamePageTitleLinkClick] 无title参数，跳过');
            return null;
        }
        return title;
    } catch (err) {
        console.log('[SamePageTitleLinkClick] URL解析失败:', err);
        return null;
    }
}
