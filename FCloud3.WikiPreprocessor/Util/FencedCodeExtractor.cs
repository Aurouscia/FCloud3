using FCloud3.WikiPreprocessor.Models;
using System.Text;

namespace FCloud3.WikiPreprocessor.Util
{
    /// <summary>
    /// 围栏代码块提取器
    /// 在正式解析前提取 ``` 包裹的代码块，避免内容被解析器处理
    /// </summary>
    internal static class FencedCodeExtractor
    {
        private const string fenceMarker = "```";
        private const char placeholderPrefix = '\u0001';

        /// <summary>
        /// 从输入中提取所有围栏代码块，替换为占位符
        /// </summary>
        public static (string processedInput, List<ExtractedCodeBlock> blocks) Extract(string input)
        {
            var blocks = new List<ExtractedCodeBlock>();
            var sb = new StringBuilder(input.Length);
            int pointer = 0;

            while (pointer < input.Length)
            {
                int fenceStart = FindFence(input, pointer);
                if (fenceStart == -1)
                {
                    // 没有更多 fence，复制剩余内容
                    sb.Append(input.AsSpan(pointer));
                    break;
                }

                // 复制 fence 之前的文本
                sb.Append(input.AsSpan(pointer, fenceStart - pointer));

                // 解析 opening fence
                int lineEnd = input.IndexOf('\n', fenceStart);
                if (lineEnd == -1) lineEnd = input.Length;
                ReadOnlySpan<char> openingLine = input.AsSpan(fenceStart, lineEnd - fenceStart).TrimEnd('\r');
                string language = ParseLanguage(openingLine);

                // 查找 closing fence
                int contentStart = lineEnd + 1; // 跳过换行符
                if (contentStart > input.Length)
                    contentStart = input.Length;

                int closingFence = FindClosingFence(input, contentStart);
                if (closingFence == -1)
                {
                    // 未闭合，视为普通文本
                    sb.Append(input.AsSpan(fenceStart));
                    break;
                }

                // 提取代码内容（不包含 closing fence 行）
                int contentEnd = input.LastIndexOf('\n', closingFence - 1);
                if (contentEnd < contentStart)
                    contentEnd = contentStart;

                string code;
                if (contentEnd > contentStart)
                {
                    code = input[contentStart..contentEnd];
                }
                else
                {
                    code = string.Empty;
                }

                // 生成占位符
                string placeholder = $"{placeholderPrefix}CODE_{blocks.Count}{placeholderPrefix}";
                sb.Append(placeholder);

                // mermaid 不转义，其他语言做 HTML 转义
                bool isMermaid = language.Equals("mermaid", StringComparison.OrdinalIgnoreCase);
                string processedCode = isMermaid ? code : EscapeCode(code);
                BlockType blockType = isMermaid ? BlockType.Mermaid : BlockType.Code;

                blocks.Add(new ExtractedCodeBlock(language, processedCode, placeholder, blockType));

                // 移动指针到 closing fence 之后
                int closingLineEnd = input.IndexOf('\n', closingFence);
                if (closingLineEnd == -1)
                    closingLineEnd = input.Length;
                pointer = closingLineEnd + 1;
            }

            return (sb.ToString(), blocks);
        }

        /// <summary>
        /// 在解析后的元素树中，将占位符替换为 CodeBlockElement
        /// </summary>
        public static IHtmlable RestorePlaceholders(IHtmlable parsed, List<ExtractedCodeBlock> blocks)
        {
            if (blocks.Count == 0)
                return parsed;

            var dict = blocks.ToDictionary(b => b.Placeholder, b => b);
            return ReplaceInElement(parsed, dict);
        }

        private static IHtmlable ReplaceInElement(IHtmlable element, Dictionary<string, ExtractedCodeBlock> dict)
        {
            if (element is TextElement text && dict.TryGetValue(text.Content, out var block))
            {
                return block.Type switch
                {
                    BlockType.Mermaid => new MermaidBlockElement(block.Code),
                    _ => new CodeBlockElement(block.Language, block.Code)
                };
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
                // BlockElement 的 Content 是 IHtmlable，需要递归替换
                var newContent = ReplaceInElement(blockEle.Content, dict);
                if (newContent != blockEle.Content)
                {
                    // 创建新的 BlockElement 并替换 Content
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
            // 根据具体类型创建新实例
            if (original is SimpleBlockElement s)
                return new SimpleBlockElement(newContent, s.PutLeft, s.PutRight);
            if (original is TitledBlockElement t)
                return new TitledBlockElement(
                    ReplaceInElement(t.Title, new Dictionary<string, ExtractedCodeBlock>()),
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
        /// 查找从指定位置开始的 fence 标记
        /// </summary>
        private static int FindFence(string input, int start)
        {
            for (int i = start; i <= input.Length - fenceMarker.Length; i++)
            {
                if (input.AsSpan(i, fenceMarker.Length).SequenceEqual(fenceMarker.AsSpan()))
                {
                    // 确认是行首（或前面只有空白）
                    bool isLineStart = true;
                    for (int j = i - 1; j >= start; j--)
                    {
                        char c = input[j];
                        if (c == '\n')
                            break;
                        if (!char.IsWhiteSpace(c))
                        {
                            isLineStart = false;
                            break;
                        }
                    }
                    if (isLineStart)
                        return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// 查找 closing fence
        /// </summary>
        private static int FindClosingFence(string input, int start)
        {
            for (int i = start; i <= input.Length - fenceMarker.Length; i++)
            {
                if (input.AsSpan(i, fenceMarker.Length).SequenceEqual(fenceMarker.AsSpan()))
                {
                    // 确认是行首
                    bool isLineStart = true;
                    for (int j = i - 1; j >= 0; j--)
                    {
                        char c = input[j];
                        if (c == '\n')
                            break;
                        if (!char.IsWhiteSpace(c))
                        {
                            isLineStart = false;
                            break;
                        }
                    }
                    if (isLineStart)
                        return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// 从 opening fence 行解析语言标识
        /// ```csharp → csharp
        /// ```     → ""
        /// </summary>
        private static string ParseLanguage(ReadOnlySpan<char> openingLine)
        {
            // openingLine 形如 "```csharp" 或 "```"
            ReadOnlySpan<char> afterFence = openingLine.Slice(fenceMarker.Length).Trim();
            if (afterFence.IsEmpty)
                return string.Empty;

            // 取第一个词作为语言标识
            int spaceIdx = afterFence.IndexOf(' ');
            if (spaceIdx == -1)
                return afterFence.ToString();
            return afterFence.Slice(0, spaceIdx).ToString();
        }

        /// <summary>
        /// 对代码内容做 HTML 转义
        /// </summary>
        private static string EscapeCode(string code)
        {
            if (string.IsNullOrEmpty(code))
                return string.Empty;

            var sb = StringBuilderPool.Rent();
            foreach (char c in code)
            {
                sb.Append(c switch
                {
                    '<' => "&lt;",
                    '>' => "&gt;",
                    '&' => "&amp;",
                    '"' => "&quot;",
                    _ => c.ToString()
                });
            }
            string result = sb.ToString();
            StringBuilderPool.Return(sb);
            return result;
        }
    }

    /// <summary>
    /// 提取出的代码块信息
    /// </summary>
    internal record ExtractedCodeBlock(string Language, string Code, string Placeholder, BlockType Type);

    internal enum BlockType
    {
        Code,
        Mermaid
    }
}
