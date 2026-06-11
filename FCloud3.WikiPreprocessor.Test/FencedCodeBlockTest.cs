using FCloud3.WikiPreprocessor.Mechanics;
using FCloud3.WikiPreprocessor.Options;

namespace FCloud3.WikiPreprocessor.Test
{
    [TestClass]
    public class FencedCodeBlockTest
    {
        private readonly Parser _parser;

        public FencedCodeBlockTest()
        {
            var parserBuilder = new ParserBuilder();
            _parser = parserBuilder.BuildParser();
        }

        [TestMethod]
        public void BasicCodeBlock()
        {
            string input = "```\nint x = 1;\n```";
            var res = _parser.RunToPlain(input);
            Assert.AreEqual("<pre><code>int x = 1;</code></pre>", res);
        }

        [TestMethod]
        public void CodeBlockWithLanguage()
        {
            string input = "```csharp\nint x = 1;\n```";
            var res = _parser.RunToPlain(input);
            Assert.AreEqual("<pre><code class=\"language-csharp\">int x = 1;</code></pre>", res);
        }

        [TestMethod]
        public void CodeBlockWithHtmlChars()
        {
            string input = "```\n<div>hello</div>\n```";
            var res = _parser.RunToPlain(input);
            Assert.AreEqual("<pre><code>&lt;div&gt;hello&lt;/div&gt;</code></pre>", res);
        }

        [TestMethod]
        public void CodeBlockWithWikiSyntax()
        {
            // 代码块内的 wiki 语法不应被解析
            string input = "```\n# 标题\n**粗体**\n```";
            var res = _parser.RunToPlain(input);
            Assert.AreEqual("<pre><code># 标题\n**粗体**</code></pre>", res);
        }

        [TestMethod]
        public void UnclosedFence()
        {
            // 未闭合的 fence：opening ``` 被行内代码规则匹配（` 是行内代码标记）
            string input = "```\nint x = 1;\n";
            var res = _parser.RunToPlain(input);
            // ``` 中的前两个 ` 被匹配为 <code></code>，剩余一个 `
            Assert.AreEqual("<p><code></code>`</p><p>int x = 1;</p>", res);
        }

        [TestMethod]
        public void MultipleCodeBlocks()
        {
            string input = "```\na\n```\n\n```\nb\n```";
            var res = _parser.RunToPlain(input);
            Assert.AreEqual("<pre><code>a</code></pre><pre><code>b</code></pre>", res);
        }

        [TestMethod]
        public void CodeBlockWithSurroundingText()
        {
            string input = "hello\n\n```\ncode\n```\n\nworld";
            var res = _parser.RunToPlain(input);
            Assert.AreEqual("<p>hello</p><pre><code>code</code></pre><p>world</p>", res);
        }

        [TestMethod]
        public void EmptyCodeBlock()
        {
            string input = "```\n```";
            var res = _parser.RunToPlain(input);
            Assert.AreEqual("<pre><code></code></pre>", res);
        }

        [TestMethod]
        public void CodeBlockWithExtraAfterLanguage()
        {
            // ```csharp hello → 语言取 csharp，忽略 hello
            string input = "```csharp hello\ncode\n```";
            var res = _parser.RunToPlain(input);
            Assert.AreEqual("<pre><code class=\"language-csharp\">code</code></pre>", res);
        }

        [TestMethod]
        public void CodeBlockWithMultipleLines()
        {
            string input = "```\nline1\nline2\nline3\n```";
            var res = _parser.RunToPlain(input);
            Assert.AreEqual("<pre><code>line1\nline2\nline3</code></pre>", res);
        }

        [TestMethod]
        public void InlineCodeStillWorks()
        {
            // 行内代码 `` ` `` 仍应正常工作
            string input = "`inline code`";
            var res = _parser.RunToPlain(input);
            Assert.AreEqual("<p><code>inline code</code></p>", res);
        }

        [TestMethod]
        public void CodeBlockWithAmpersand()
        {
            string input = "```\na && b\n```";
            var res = _parser.RunToPlain(input);
            Assert.AreEqual("<pre><code>a &amp;&amp; b</code></pre>", res);
        }

        [TestMethod]
        public void MermaidBlock()
        {
            string input = "```mermaid\ngraph TD;\n    A-->B;\n```";
            var res = _parser.RunToPlain(input);
            Assert.AreEqual("<pre class=\"mermaid\">graph TD;\n    A-->B;</pre>", res);
        }

        [TestMethod]
        public void MermaidFlowchart()
        {
            string input = "```mermaid\nflowchart LR\n    Start --> Stop\n```";
            var res = _parser.RunToPlain(input);
            Assert.AreEqual("<pre class=\"mermaid\">flowchart LR\n    Start --> Stop</pre>", res);
        }

        [TestMethod]
        public void MermaidWithSurroundingText()
        {
            string input = "请看下图\n\n```mermaid\ngraph TD;\nA-->B;\n```\n\n结束";
            var res = _parser.RunToPlain(input);
            Assert.AreEqual("<p>请看下图</p><pre class=\"mermaid\">graph TD;\nA-->B;</pre><p>结束</p>", res);
        }

        [TestMethod]
        public void MermaidCaseInsensitive()
        {
            string input = "```MERMAID\ngraph TD;\nA-->B;\n```";
            var res = _parser.RunToPlain(input);
            Assert.AreEqual("<pre class=\"mermaid\">graph TD;\nA-->B;</pre>", res);
        }

        [TestMethod]
        public void MermaidAndCodeBlockMixed()
        {
            string input = "```csharp\nint x = 1;\n```\n\n```mermaid\ngraph TD;\nA-->B;\n```";
            var res = _parser.RunToPlain(input);
            Assert.AreEqual(
                "<pre><code class=\"language-csharp\">int x = 1;</code></pre>" +
                "<pre class=\"mermaid\">graph TD;\nA-->B;</pre>", res);
        }


    }
}
