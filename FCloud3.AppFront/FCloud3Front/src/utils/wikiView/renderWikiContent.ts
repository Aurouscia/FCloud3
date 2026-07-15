/// <reference path="../../types/cdn-libs.d.ts" />

let mermaidInitialized = false;

function ensureMermaidInit(): void {
    if (mermaidInitialized) return;
    mermaid.initialize({
        startOnLoad: false,
        theme: 'default',
    });
    mermaidInitialized = true;
}

const rendererBase = {
    katex: '/renderers/katex@0.17.0/katex.min.js',
    mermaid: '/renderers/mermaid@11.15.0/mermaid.min.js',
    prism: '/renderers/prismjs@1.30.0',
};

const loadedResources = new Map<string, Promise<void>>();

function loadScript(src: string): Promise<void> {
    const existing = loadedResources.get(src);
    if (existing) return existing;
    const promise = new Promise<void>((resolve, reject) => {
        const script = document.createElement('script');
        script.src = src;
        script.crossOrigin = 'anonymous';
        script.referrerPolicy = 'no-referrer';
        script.onload = () => resolve();
        script.onerror = () => reject(new Error(`加载失败：${src}`));
        document.head.appendChild(script);
    });
    loadedResources.set(src, promise);
    return promise;
}

async function ensureKatex(): Promise<void> {
    if (window.katex) return;
    await loadScript(rendererBase.katex);
}

async function ensureMermaid(): Promise<void> {
    if (window.mermaid) return;
    await loadScript(rendererBase.mermaid);
}

// PrismJS 语言组件依赖关系（按需加载时必须先加载依赖）
const prismLangDeps: Record<string, string[]> = {
    c: ['clike'],
    cpp: ['c'],
    csharp: ['clike'],
    java: ['clike'],
    javascript: ['clike'],
    typescript: ['javascript', 'clike'],
    go: ['clike'],
    markdown: ['markup'],
};

// 常见语言别名映射（class 中的简称 -> PrismJS 实际文件名）
const prismLangAliases: Record<string, string> = {
    js: 'javascript',
    ts: 'typescript',
    cs: 'csharp',
    py: 'python',
    yml: 'yaml',
    shell: 'bash',
    sh: 'bash',
    md: 'markdown',
    html: 'markup',
    xml: 'markup',
    svg: 'markup',
    mathml: 'markup',
};

async function ensurePrismCore(): Promise<void> {
    if (window.Prism) return;
    await loadScript(`${rendererBase.prism}/components/prism-core.min.js`);
}

async function ensurePrismPlugins(): Promise<void> {
    await ensurePrismCore();
    await loadScript(`${rendererBase.prism}/plugins/line-numbers/prism-line-numbers.min.js`);
    await loadScript(`${rendererBase.prism}/plugins/toolbar/prism-toolbar.min.js`);
    await loadScript(`${rendererBase.prism}/plugins/copy-to-clipboard/prism-copy-to-clipboard.min.js`);
}

async function loadPrismLanguage(lang: string): Promise<void> {
    const cacheKey = `prism-lang-${lang}`;
    const existing = loadedResources.get(cacheKey);
    if (existing) return existing;

    const promise = (async () => {
        await ensurePrismPlugins();
        const deps = prismLangDeps[lang] ?? [];
        await Promise.all(deps.map(loadPrismLanguage));
        try {
            await loadScript(`${rendererBase.prism}/components/prism-${lang}.min.js`);
        } catch (e) {
            console.warn(`Prism 语言组件加载失败：${lang}`, e);
        }
    })();
    loadedResources.set(cacheKey, promise);
    return promise;
}

/**
 * 渲染代码高亮（PrismJS）
 * 只处理 <code class="language-xxx"> 元素，并按需加载对应语言组件
 */
export async function renderCodeHighlight(container?: HTMLElement | null): Promise<void> {
    const scope = container ?? document;
    const codes = scope.querySelectorAll('code[class^="language-"]');
    if (codes.length === 0) return;

    const languages = new Set<string>();
    for (const el of codes) {
        const match = el.className.match(/language-([\w-]+)/);
        if (match) {
            const rawLang = match[1].toLowerCase();
            const lang = prismLangAliases[rawLang] ?? rawLang;
            languages.add(lang);
        }
    }

    await ensurePrismPlugins();
    await Promise.all([...languages].map(loadPrismLanguage));

    for (const el of codes) {
        window.Prism.highlightElement(el);
    }
}

/**
 * 渲染 Mermaid 图表
 * 处理 <pre class="mermaid"> 元素
 */
export async function renderMermaid(container?: HTMLElement | null): Promise<void> {
    await ensureMermaid();
    ensureMermaidInit();
    const scope = container ?? document;
    const elements = scope.querySelectorAll('pre.mermaid:not([data-processed])');

    for (const el of elements) {
        const graphDefinition = el.textContent || '';
        try {
            const id = `mermaid-${Math.random().toString(36).slice(2)}`;
            const { svg } = await mermaid.render(id, graphDefinition);
            el.innerHTML = svg;
            el.setAttribute('data-processed', 'true');
        } catch (e) {
            console.error('Mermaid render failed:', e);
            el.classList.add('mermaid-error');
        }
    }
}

/**
 * 渲染 LaTeX 数学公式（KaTeX）
 * 处理 <pre class="latex">（块级）和 <span class="latex-inline">（行内）
 */
export async function renderLatex(container?: HTMLElement | null): Promise<void> {
    await ensureKatex();
    const scope = container ?? document;

    const blockElements = scope.querySelectorAll('pre.latex:not([data-processed])');
    for (const el of blockElements) {
        const latexSource = el.textContent || '';
        try {
            const html = katex.renderToString(latexSource, {
                throwOnError: false,
                displayMode: true,
            });
            el.innerHTML = html;
            el.setAttribute('data-processed', 'true');
        } catch (e) {
            console.error('KaTeX block render failed:', e);
            el.classList.add('latex-error');
        }
    }

    const inlineElements = scope.querySelectorAll('span.latex-inline:not([data-processed])');
    for (const el of inlineElements) {
        const latexSource = el.textContent || '';
        try {
            const html = katex.renderToString(latexSource, {
                throwOnError: false,
                displayMode: false,
            });
            el.innerHTML = html;
            el.setAttribute('data-processed', 'true');
        } catch (e) {
            console.error('KaTeX inline render failed:', e);
            el.classList.add('latex-error');
        }
    }
}

/**
 * 一键渲染所有 Wiki 特殊元素
 * 仅当页面对应元素存在时才加载并渲染，避免无谓的 CDN 请求
 */
export async function renderWikiSpecialElements(container?: HTMLElement | null): Promise<void> {
    const scope = container ?? document;
    const hasCode = scope.querySelector('code[class^="language-"]') !== null;
    const hasMermaid = scope.querySelector('pre.mermaid:not([data-processed])') !== null;
    const hasLatex = scope.querySelector('pre.latex:not([data-processed]), span.latex-inline:not([data-processed])') !== null;

    await Promise.all([
        hasCode ? renderCodeHighlight(container) : Promise.resolve(),
        hasMermaid ? renderMermaid(container) : Promise.resolve(),
        hasLatex ? renderLatex(container) : Promise.resolve(),
    ]);
}
