using FCloud3.WikiPreprocessor.Mechanics;
using FCloud3.WikiPreprocessor.Options;
using FCloud3.WikiPreprocessor.Rules;

namespace FCloud3.WikiPreprocessor.Test
{
    [TestClass]
    public class HtmlAreaTest
    {
        private readonly Parser _parser;

        public HtmlAreaTest()
        {
            var parserBuilder = new ParserBuilder();
            _parser = parserBuilder.BuildParser();
        }

        [TestMethod]
        public void BasicStyleBlock()
        {
            string input = "<style>.red { color: red; }</style>";
            var res = _parser.RunToPlain(input);
            Assert.AreEqual("<style>.red { color: red; }</style>", res);
        }

        [TestMethod]
        public void StyleBlockWithMultipleLines()
        {
            string input = "<style>\n.red { color: red; }\n.blue { color: blue; }\n</style>";
            var res = _parser.RunToPlain(input);
            Assert.AreEqual("<style>\n.red { color: red; }\n.blue { color: blue; }\n</style>", res);
        }

        [TestMethod]
        public void StyleBlockWithWikiSyntaxInside()
        {
            // style 标签内的内容不应被 wiki 解析器处理
            string input = "<style>\n#title { color: red; }\n.bold { font-weight: bold; }\n</style>";
            var res = _parser.RunToPlain(input);
            Assert.AreEqual("<style>\n#title { color: red; }\n.bold { font-weight: bold; }\n</style>", res);
        }

        [TestMethod]
        public void StyleBlockWithSurroundingText()
        {
            string input = "hello\n\n<style>\n.red { color: red; }\n</style>\n\nworld";
            var res = _parser.RunToPlain(input);
            Assert.AreEqual("<p>hello</p><style>\n.red { color: red; }\n</style><p>world</p>", res);
        }

        [TestMethod]
        public void StyleBlockWithCssBraces()
        {
            // CSS 中的 {} 不应被误判为模板调用
            string input = "<style>\n.card {\n  border: 1px solid #ccc;\n  padding: 10px;\n}\n</style>";
            var res = _parser.RunToPlain(input);
            Assert.AreEqual("<style>\n.card {\n  border: 1px solid #ccc;\n  padding: 10px;\n}\n</style>", res);
        }

        [TestMethod]
        public void ScriptBlock()
        {
            string input = "<script>\nconsole.log('hello');\n</script>";
            var res = _parser.RunToPlain(input);
            Assert.AreEqual("<script>\nconsole.log('hello');\n</script>", res);
        }

        [TestMethod]
        public void ScriptBlockWithWikiSyntaxInside()
        {
            // script 标签内的内容不应被 wiki 解析器处理
            string input = "<script>\n// 注释\nvar x = **1**;\n</script>";
            var res = _parser.RunToPlain(input);
            Assert.AreEqual("<script>\n// 注释\nvar x = **1**;\n</script>", res);
        }

        [TestMethod]
        public void MultipleStyleBlocks()
        {
            string input = "<style>\n.a {}\n</style>\n\n<style>\n.b {}\n</style>";
            var res = _parser.RunToPlain(input);
            Assert.AreEqual("<style>\n.a {}\n</style><style>\n.b {}\n</style>", res);
        }

        [TestMethod]
        public void StyleAndCodeBlockMixed()
        {
            string input = "<style>\n.red { color: red; }\n</style>\n\n```\ncode\n```";
            var res = _parser.RunToPlain(input);
            Assert.AreEqual("<style>\n.red { color: red; }\n</style><pre><code>code</code></pre>", res);
        }

        [TestMethod]
        public void StyleBlockWithAttributes()
        {
            string input = "<style type=\"text/css\" class=\"custom\">\n.red { color: red; }\n</style>";
            var res = _parser.RunToPlain(input);
            Assert.AreEqual("<style type=\"text/css\" class=\"custom\">\n.red { color: red; }\n</style>", res);
        }

        [TestMethod]
        public void UnclosedStyleBlock()
        {
            // 未闭合的 style 标签：当作普通文本处理（经过 HtmlSanitizer 后输出）
            string input = "<style>\n.red { color: red; }\n";
            var res = _parser.RunToPlain(input);
            // HtmlSanitizer 会保留 <style> 标签，但内容可能被处理
            // 由于未闭合，HtmlAreaExtractor 不会提取，交给正常解析流程
            // 预期：内容被包在 <p> 中（因为未闭合，不触发提取）
            Assert.IsTrue(res.Contains("<p>"));
        }

        [TestMethod]
        public void StyleBlockCaseInsensitive()
        {
            string input = "<STYLE>\n.red { color: red; }\n</STYLE>";
            var res = _parser.RunToPlain(input);
            Assert.AreEqual("<STYLE>\n.red { color: red; }\n</STYLE>", res);
        }

        [TestMethod]
        public void StyleBlockInsideTemplate()
        {
            // 模板中的 style 标签也应被保护
            var parserBuilder = new ParserBuilder()
                .Template.AddTemplates(new()
                {
                    new Template("样式卡片", "<style>\n.card { border: 1px solid #ccc; }\n</style><div class=\"card\">[[[__内容__]]]</div>")
                });
            var parser = parserBuilder.BuildParser();

            string input = "{{样式卡片}内容::hello world}";
            var res = parser.RunToPlain(input, true);
            Assert.IsTrue(res.Contains("<style>\n.card { border: 1px solid #ccc; }\n</style>"));
            Assert.IsTrue(res.Contains("<div class=\"card\">"));
        }

        [TestMethod]
        public void StyleBlockMultiline_NotSplitIntoParagraphs()
        {
            // 核心验证：多行 style 标签不会被拆分为多个 <p>
            // 如果没有 HtmlAreaExtractor 保护，.red/.blue/.green 各行会变成独立的 <p>
            string input = "<style>\n.red { color: red; }\n.blue { color: blue; }\n.green { color: green; }\n</style>";
            var res = _parser.RunToPlain(input);
            // 必须不包含 <p> 标签，否则说明被拆分了
            Assert.IsFalse(res.Contains("<p>"), "style 块不应被拆分为 <p> 段落");
            Assert.AreEqual("<style>\n.red { color: red; }\n.blue { color: blue; }\n.green { color: green; }\n</style>", res);
        }

        [TestMethod]
        public void ScriptBlockMultiline_NotSplitIntoParagraphs()
        {
            string input = "<script>\nfunction a() {}\nfunction b() {}\nfunction c() {}\n</script>";
            var res = _parser.RunToPlain(input);
            Assert.IsFalse(res.Contains("<p>"), "script 块不应被拆分为 <p> 段落");
            Assert.AreEqual("<script>\nfunction a() {}\nfunction b() {}\nfunction c() {}\n</script>", res);
        }

        [TestMethod]
        public void SvgBlockMultiline_NotSplitIntoParagraphs()
        {
            string input = "<svg>\n  <rect x=\"10\" y=\"10\" />\n  <circle cx=\"50\" cy=\"50\" />\n  <text>hello</text>\n</svg>";
            var res = _parser.RunToPlain(input);
            Assert.IsFalse(res.Contains("<p>"), "svg 块不应被拆分为 <p> 段落");
            Assert.AreEqual("<svg>\n  <rect x=\"10\" y=\"10\" />\n  <circle cx=\"50\" cy=\"50\" />\n  <text>hello</text>\n</svg>", res);
        }

        [TestMethod]
        public void IframeBlockMultiline_NotSplitIntoParagraphs()
        {
            string input = "<iframe>\n  <p>fallback</p>\n  <a href=\"/\">link</a>\n</iframe>";
            var res = _parser.RunToPlain(input);
            // iframe 块本身不应被 <p> 包裹（但内部可以有 <p>）
            Assert.IsFalse(res.Contains("<p><iframe>"), "iframe 块不应被 <p> 包裹");
            Assert.IsFalse(res.Contains("</iframe></p>"), "iframe 块不应被 </p> 包裹");
            Assert.AreEqual("<iframe>\n  <p>fallback</p>\n  <a href=\"/\">link</a>\n</iframe>", res);
        }

        [TestMethod]
        public void VideoBlockMultiline_NotSplitIntoParagraphs()
        {
            string input = "<video>\n  <source src=\"a.mp4\" />\n  <source src=\"a.webm\" />\n  <track src=\"subtitles.vtt\" />\n</video>";
            var res = _parser.RunToPlain(input);
            Assert.IsFalse(res.Contains("<p>"), "video 块不应被拆分为 <p> 段落");
            Assert.AreEqual("<video>\n  <source src=\"a.mp4\" />\n  <source src=\"a.webm\" />\n  <track src=\"subtitles.vtt\" />\n</video>", res);
        }

        [TestMethod]
        public void CanvasBlockMultiline_NotSplitIntoParagraphs()
        {
            string input = "<canvas>\n  // draw something\n  ctx.fillRect(0,0,100,100);\n</canvas>";
            var res = _parser.RunToPlain(input);
            Assert.IsFalse(res.Contains("<p>"), "canvas 块不应被拆分为 <p> 段落");
            Assert.AreEqual("<canvas>\n  // draw something\n  ctx.fillRect(0,0,100,100);\n</canvas>", res);
        }

        [TestMethod]
        public void NoscriptBlockMultiline_NotSplitIntoParagraphs()
        {
            string input = "<noscript>\n  <p>请启用 JavaScript</p>\n  <p>否则页面无法正常工作</p>\n</noscript>";
            var res = _parser.RunToPlain(input);
            // noscript 块本身不应被 <p> 包裹（但内部可以有 <p>）
            Assert.IsFalse(res.Contains("<p><noscript>"), "noscript 块不应被 <p> 包裹");
            Assert.IsFalse(res.Contains("</noscript></p>"), "noscript 块不应被 </p> 包裹");
            Assert.AreEqual("<noscript>\n  <p>请启用 JavaScript</p>\n  <p>否则页面无法正常工作</p>\n</noscript>", res);
        }

        [TestMethod]
        public void HtmlBlockWithSurroundingParagraphs()
        {
            // 验证 HTML 块前后有普通文本时，普通文本正常解析为 <p>，但 HTML 块本身不包 <p>
            string input = "第一段\n\n<style>\n.red { color: red; }\n</style>\n\n第二段";
            var res = _parser.RunToPlain(input);
            // 前后文本应该是 <p>
            Assert.IsTrue(res.Contains("<p>第一段</p>"));
            Assert.IsTrue(res.Contains("<p>第二段</p>"));
            // style 块本身不应被 <p> 包裹
            Assert.IsTrue(res.Contains("<style>\n.red { color: red; }\n</style>"));
            // 整体检查：style 前后不应该有 <p> 包裹它
            Assert.IsFalse(res.Contains("<p><style>"));
            Assert.IsFalse(res.Contains("</style></p>"));
        }

        [TestMethod]
        public void HtmlBlockWithWikiSyntaxMultiline_NotParsed()
        {
            // 多行 HTML 块内的 wiki 语法不应被解析
            string input = "<script>\n# 这不是标题\n**这不是粗体**\n- 这不是列表\n> 这不是引用\n</script>";
            var res = _parser.RunToPlain(input);
            Assert.IsFalse(res.Contains("<h"), "script 内的 # 不应被解析为标题");
            Assert.IsFalse(res.Contains("<b>"), "script 内的 ** 不应被解析为粗体");
            Assert.IsFalse(res.Contains("<ul>"), "script 内的 - 不应被解析为列表");
            Assert.IsFalse(res.Contains("<div class=\"quote\">"), "script 内的 > 不应被解析为引用");
            Assert.AreEqual("<script>\n# 这不是标题\n**这不是粗体**\n- 这不是列表\n> 这不是引用\n</script>", res);
        }

        [TestMethod]
        public void HtmlBlockWithTemplateBraces_NotParsed()
        {
            // HTML 块内的 {{}} 和 {} 不应被误判为模板/植入
            string input = "<style>\n.card {\n  content: \"{{template}}\";\n  border: {1px solid red};\n}\n</style>";
            var res = _parser.RunToPlain(input);
            Assert.IsFalse(res.Contains("[[解析错误]]"), "style 内的 {{}} 不应触发模板解析错误");
            Assert.AreEqual("<style>\n.card {\n  content: \"{{template}}\";\n  border: {1px solid red};\n}\n</style>", res);
        }

        [TestMethod]
        public void MultipleHtmlBlocksMixedWithText()
        {
            string input = "text1\n\n<style>\n.a{}\n</style>\n\ntext2\n\n<script>\nconsole.log(1)\n</script>\n\ntext3";
            var res = _parser.RunToPlain(input);
            Assert.AreEqual(
                "<p>text1</p>" +
                "<style>\n.a{}\n</style>" +
                "<p>text2</p>" +
                "<script>\nconsole.log(1)\n</script>" +
                "<p>text3</p>",
                res);
        }

        [TestMethod]
        public void InlineStyleTagNotAffected()
        {
            // 行内不应出现的 style 标签（单行的）
            string input = "<style>.red{color:red}</style>";
            var res = _parser.RunToPlain(input);
            Assert.AreEqual("<style>.red{color:red}</style>", res);
        }

        [TestMethod]
        public void IframeBlock()
        {
            string input = "<iframe src=\"https://example.com\">\n</iframe>";
            var res = _parser.RunToPlain(input);
            Assert.AreEqual("<iframe src=\"https://example.com\">\n</iframe>", res);
        }

        [TestMethod]
        public void SvgBlock()
        {
            string input = "<svg>\n  <circle cx=\"50\" cy=\"50\" r=\"40\" />\n</svg>";
            var res = _parser.RunToPlain(input);
            Assert.AreEqual("<svg>\n  <circle cx=\"50\" cy=\"50\" r=\"40\" />\n</svg>", res);
        }

        [TestMethod]
        public void VideoBlock()
        {
            string input = "<video controls>\n  <source src=\"movie.mp4\" type=\"video/mp4\">\n</video>";
            var res = _parser.RunToPlain(input);
            Assert.AreEqual("<video controls>\n  <source src=\"movie.mp4\" type=\"video/mp4\">\n</video>", res);
        }

        [TestMethod]
        public void AudioBlock()
        {
            string input = "<audio controls>\n  <source src=\"audio.mp3\" type=\"audio/mpeg\">\n</audio>";
            var res = _parser.RunToPlain(input);
            Assert.AreEqual("<audio controls>\n  <source src=\"audio.mp3\" type=\"audio/mpeg\">\n</audio>", res);
        }

        [TestMethod]
        public void CanvasBlock()
        {
            string input = "<canvas id=\"myCanvas\" width=\"200\" height=\"100\">\n</canvas>";
            var res = _parser.RunToPlain(input);
            Assert.AreEqual("<canvas id=\"myCanvas\" width=\"200\" height=\"100\">\n</canvas>", res);
        }

        [TestMethod]
        public void MathBlock()
        {
            string input = "<math>\n  <mi>x</mi>\n</math>";
            var res = _parser.RunToPlain(input);
            Assert.AreEqual("<math>\n  <mi>x</mi>\n</math>", res);
        }

        [TestMethod]
        public void EmbedBlock()
        {
            string input = "<embed src=\"movie.swf\" type=\"application/x-shockwave-flash\">\n</embed>";
            var res = _parser.RunToPlain(input);
            Assert.AreEqual("<embed src=\"movie.swf\" type=\"application/x-shockwave-flash\">\n</embed>", res);
        }

        [TestMethod]
        public void ObjectBlock()
        {
            string input = "<object data=\"movie.swf\" type=\"application/x-shockwave-flash\">\n</object>";
            var res = _parser.RunToPlain(input);
            Assert.AreEqual("<object data=\"movie.swf\" type=\"application/x-shockwave-flash\">\n</object>", res);
        }

        [TestMethod]
        public void NoscriptBlock()
        {
            string input = "<noscript>\n  <p>请启用 JavaScript</p>\n</noscript>";
            var res = _parser.RunToPlain(input);
            Assert.AreEqual("<noscript>\n  <p>请启用 JavaScript</p>\n</noscript>", res);
        }

        [TestMethod]
        public void MultipleHtmlBlocks()
        {
            string input = "<style>\n.a{}\n</style>\n\n<script>\nconsole.log(1)\n</script>\n\n<svg>\n<circle/>\n</svg>";
            var res = _parser.RunToPlain(input);
            Assert.AreEqual(
                "<style>\n.a{}\n</style>" +
                "<script>\nconsole.log(1)\n</script>" +
                "<svg>\n<circle/>\n</svg>",
                res);
        }

        [TestMethod]
        public void HtmlBlockWithWikiSyntaxInside()
        {
            // SVG 内的内容不应被 wiki 解析器处理
            string input = "<svg>\n  <text>**粗体**</text>\n  # 标题\n</svg>";
            var res = _parser.RunToPlain(input);
            Assert.AreEqual("<svg>\n  <text>**粗体**</text>\n  # 标题\n</svg>", res);
        }

        [TestMethod]
        public void HtmlBlockCaseInsensitive()
        {
            string input = "<IFRAME>\ncontent\n</IFRAME>";
            var res = _parser.RunToPlain(input);
            Assert.AreEqual("<IFRAME>\ncontent\n</IFRAME>", res);
        }
    }
}
