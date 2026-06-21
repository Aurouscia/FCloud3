using FCloud3.WikiPreprocessor.Models;
using System.Text;

namespace FCloud3.WikiPreprocessor.Util
{
    /// <summary>
    /// HTML 区域提取器
    /// 在正式解析前提取 &lt;style&gt;、&lt;script&gt; 等 HTML 标签包裹的区域，避免内容被解析器处理
    /// 处理方式与 FencedCodeExtractor 类似：提取 → 占位符 → 解析后还原
    /// </summary>
    internal static class HtmlAreaExtractor
    {
        private const char placeholderPrefix = '\u0002';

        /// <summary>
        /// 需要提取保护的 HTML 标签名（小写）
        /// 包括：样式、以及各种常见的嵌入式媒体/交互组件
        /// </summary>
        private static readonly HashSet<string> ProtectedTags = new(StringComparer.OrdinalIgnoreCase)
        {
            "style",
            "iframe",
            "canvas",
            "svg",
            "math",
            "video",
            "audio",
            "embed",
            "object",
            "map",
            "noscript",
            "template"
        };

        /// <summary>
        /// 从输入中提取所有受保护的 HTML 区域，替换为占位符
        /// </summary>
        public static (string processedInput, List<ExtractedHtmlArea> areas) Extract(string input)
        {
            var areas = new List<ExtractedHtmlArea>();
            var sb = new StringBuilder(input.Length);
            int pointer = 0;

            while (pointer < input.Length)
            {
                // 查找下一个受保护标签的起始位置
                int tagStart = FindNextProtectedTag(input, pointer);
                if (tagStart == -1)
                {
                    // 没有更多受保护标签，复制剩余内容
                    sb.Append(input.AsSpan(pointer));
                    break;
                }

                // 复制标签之前的文本
                sb.Append(input.AsSpan(pointer, tagStart - pointer));

                // 解析起始标签，获取标签名和标签结束位置
                var (tagName, openTagEnd) = ParseOpenTag(input, tagStart);
                if (tagName is null || openTagEnd == -1)
                {
                    // 解析失败，当作普通文本处理
                    sb.Append(input.AsSpan(tagStart, 1));
                    pointer = tagStart + 1;
                    continue;
                }

                // 查找对应的闭合标签
                int closeTagStart = FindClosingTag(input, openTagEnd + 1, tagName);
                if (closeTagStart == -1)
                {
                    // 未闭合，当作普通文本处理
                    sb.Append(input.AsSpan(tagStart));
                    break;
                }

                // 提取 HTML 区域内容（包含起始标签和闭合标签）
                int closeTagEnd = closeTagStart + $"</{tagName}>".Length;
                string htmlContent = input[tagStart..closeTagEnd];

                // 生成占位符
                string placeholder = $"{placeholderPrefix}HTML_{areas.Count}{placeholderPrefix}";
                sb.Append(placeholder);

                areas.Add(new ExtractedHtmlArea(tagName, htmlContent, placeholder));

                // 移动指针到闭合标签之后
                pointer = closeTagEnd;
            }

            return (sb.ToString(), areas);
        }

        /// <summary>
        /// 在解析后的元素树中，将占位符替换为原始 HTML 文本
        /// </summary>
        public static IHtmlable RestorePlaceholders(IHtmlable parsed, List<ExtractedHtmlArea> areas)
        {
            if (areas.Count == 0)
                return parsed;

            var dict = areas.ToDictionary(a => a.Placeholder, a => a);
            return ReplaceInElement(parsed, dict);
        }

        private static IHtmlable ReplaceInElement(IHtmlable element, Dictionary<string, ExtractedHtmlArea> dict)
        {
            if (element is TextElement text && dict.TryGetValue(text.Content, out var area))
            {
                // 占位符匹配成功，返回原始 HTML 内容（不经过任何解析，直接输出）
                return new TextElement(area.HtmlContent);
            }

            if (element is ElementCollection collection)
            {
                var newCollection = new ElementCollection(collection.Count);
                foreach (var item in collection)
                {
                    newCollection.Add(ReplaceInElement(item, dict));
                }
                return newCollection.Simplify();
            }

            if (element is BlockElement blockEle)
            {
                var newContent = ReplaceInElement(blockEle.Content, dict);
                if (newContent != blockEle.Content)
                {
                    return CreateBlockWithNewContent(blockEle, newContent);
                }
            }

            if (element is RuledInlineElement ruledInline)
            {
                var newContent = ReplaceInElement(ruledInline.Content, dict);
                if (newContent != ruledInline.Content)
                {
                    return new RuledInlineElement(newContent, ruledInline.Rule);
                }
            }

            return element;
        }

        private static IHtmlable CreateBlockWithNewContent(BlockElement original, IHtmlable newContent)
        {
            if (original is SimpleBlockElement s)
                return new SimpleBlockElement(newContent, s.PutLeft, s.PutRight);
            if (original is TitledBlockElement t)
                return new TitledBlockElement(
                    ReplaceInElement(t.Title, new Dictionary<string, ExtractedHtmlArea>()),
                    t.TitleOriginal, null, t.Level, newContent, t.TitleId);
            if (original is RuledBlockElement r)
                return new RuledBlockElement(newContent, r.GenByRule);
            if (original is FootNoteBodyElement f)
                return new FootNoteBodyElement(f.Name, newContent, f.Hash);
            if (original is FootNoteBodyPlaceholderElement p)
                return new FootNoteBodyPlaceholderElement(p.Body, p.Rule);
            return original;
        }

        /// <summary>
        /// 查找从指定位置开始的下一个受保护标签
        /// </summary>
        private static int FindNextProtectedTag(string input, int start)
        {
            for (int i = start; i < input.Length; i++)
            {
                if (input[i] == '<')
                {
                    // 检查是否是起始标签（不是闭合标签）
                    if (i + 1 < input.Length && input[i + 1] != '/')
                    {
                        // 尝试匹配受保护标签名
                        foreach (var tag in ProtectedTags)
                        {
                            if (i + 1 + tag.Length <= input.Length)
                            {
                                ReadOnlySpan<char> candidate = input.AsSpan(i + 1, tag.Length);
                                if (candidate.Equals(tag, StringComparison.OrdinalIgnoreCase))
                                {
                                    // 确认标签名后紧跟空白、> 或 />
                                    int afterTag = i + 1 + tag.Length;
                                    if (afterTag < input.Length)
                                    {
                                        char c = input[afterTag];
                                        if (c == '>' || c == ' ' || c == '\t' || c == '\n' || c == '\r' || c == '/')
                                            return i;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return -1;
        }

        /// <summary>
        /// 解析起始标签，返回标签名和标签结束位置（> 的位置）
        /// </summary>
        private static (string? tagName, int endPos) ParseOpenTag(string input, int start)
        {
            if (start >= input.Length || input[start] != '<')
                return (null, -1);

            int i = start + 1;
            // 跳过可能的空白（虽然标签名不应该以空白开头）
            while (i < input.Length && char.IsWhiteSpace(input[i]))
                i++;

            // 读取标签名
            int nameStart = i;
            while (i < input.Length && input[i] != '>' && input[i] != ' ' && input[i] != '\t' && input[i] != '\n' && input[i] != '\r' && input[i] != '/')
                i++;

            if (nameStart == i)
                return (null, -1);

            string tagName = input[nameStart..i];

            // 查找 >
            while (i < input.Length && input[i] != '>')
            {
                if (input[i] == '"' || input[i] == '\'')
                {
                    // 跳过引号内的内容
                    char quote = input[i];
                    i++;
                    while (i < input.Length && input[i] != quote)
                        i++;
                    if (i < input.Length) i++;
                }
                else
                {
                    i++;
                }
            }

            if (i >= input.Length || input[i] != '>')
                return (null, -1);

            return (tagName, i);
        }

        /// <summary>
        /// 查找闭合标签
        /// </summary>
        private static int FindClosingTag(string input, int start, string tagName)
        {
            string closingTag = $"</{tagName}>";
            for (int i = start; i <= input.Length - closingTag.Length; i++)
            {
                if (input.AsSpan(i, closingTag.Length).Equals(closingTag, StringComparison.OrdinalIgnoreCase))
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// 检查某行是否是 HTML 区域占位符
        /// </summary>
        public static bool IsHtmlAreaPlaceholder(string? text)
        {
            return text is not null && text.Length >= 3
                && text[0] == placeholderPrefix
                && text[^1] == placeholderPrefix;
        }
    }

    /// <summary>
    /// 提取出的 HTML 区域信息
    /// </summary>
    internal record ExtractedHtmlArea(string TagName, string HtmlContent, string Placeholder);
}
