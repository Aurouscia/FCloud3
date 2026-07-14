// CDN 加载库的类型声明
// 这些库通过 <script> 标签从 unpkg CDN 加载，不通过 npm 安装

declare global {
    const katex: {
        renderToString: (
            tex: string,
            options?: {
                throwOnError?: boolean;
                displayMode?: boolean;
            }
        ) => string;
    };

    const mermaid: {
        initialize: (config: {
            startOnLoad?: boolean;
            theme?: string;
        }) => void;
        render: (
            id: string,
            text: string
        ) => Promise<{ svg: string }>;
    };

    const Prism: {
        highlightAll: () => void;
        highlightElement: (element: Element) => void;
    };

    interface Window {
        katex: typeof katex;
        mermaid: typeof mermaid;
        Prism: typeof Prism;
    }
}

export {};
