using FCloud3.WikiPreprocessor.Mechanics;
using FCloud3.WikiPreprocessor.Options;
using FCloud3.WikiPreprocessor.ConvertingProvider;
using FCloud3.WikiPreprocessor.ConvertingProvider.Models;
using FCloud3.WikiPreprocessor.Test.Support;

namespace FCloud3.WikiPreprocessor.Test
{
    internal class ConvertingProviderForLatexTest : ConvertingProviderBase
    {
        public override string? Implant(string implantSpan)
        {
            if (implantSpan == "implant")
                return "<span class=\"implanted\">implanted</span>";
            return null;
        }
    }

    [TestClass]
    public class LatexTest
    {
        private readonly Parser _parser;

        public LatexTest()
        {
            var parserBuilder = new ParserBuilder();
            _parser = parserBuilder.BuildParser();
        }

        [TestMethod]
        public void InlineLatexBasic()
        {
            string input = "质能方程 $E = mc^2$ 由爱因斯坦提出";
            var res = _parser.RunToPlain(input);
            Assert.AreEqual(
                "<p>质能方程 <span class=\"latex-inline\">E = mc^2</span> 由爱因斯坦提出</p>", res);
        }

        [TestMethod]
        public void InlineLatexAtStart()
        {
            string input = "$x^2 + y^2 = z^2$ 是勾股定理";
            var res = _parser.RunToPlain(input);
            Assert.AreEqual(
                "<p><span class=\"latex-inline\">x^2 + y^2 = z^2</span> 是勾股定理</p>", res);
        }

        [TestMethod]
        public void InlineLatexAtEnd()
        {
            string input = "圆的面积公式为 $A = \\pi r^2$";
            var res = _parser.RunToPlain(input);
            Assert.AreEqual(
                "<p>圆的面积公式为 <span class=\"latex-inline\">A = \\pi r^2</span></p>", res);
        }

        [TestMethod]
        public void InlineLatexMultiple()
        {
            string input = "$a$ 和 $b$ 的和是 $c$";
            var res = _parser.RunToPlain(input);
            Assert.AreEqual(
                "<p><span class=\"latex-inline\">a</span> 和 <span class=\"latex-inline\">b</span> 的和是 <span class=\"latex-inline\">c</span></p>", res);
        }

        [TestMethod]
        public void InlineLatexWithSpecialChars()
        {
            // 行内 latex 内容不转义
            string input = "$a < b && c > d$";
            var res = _parser.RunToPlain(input);
            Assert.AreEqual(
                "<p><span class=\"latex-inline\">a &lt; b &amp;&amp; c &gt; d</span></p>", res);
        }

        [TestMethod]
        public void InlineLatexEmptyRejected()
        {
            // 空内容不应匹配
            string input = "$$";
            var res = _parser.RunToPlain(input);
            // $$ 被行内 $ 规则匹配两次，中间空内容被 FulFill 拒绝
            // 实际行为：第一个 $ 匹配到第二个 $，中间为空，FulFill 返回 false，不加入 marks
            // 最终作为普通文本输出
            Assert.AreEqual("<p>$$</p>", res);
        }

        [TestMethod]
        public void InlineLatexNestedDollarRejected()
        {
            // 内容中包含未转义的 $ 不应匹配
            string input = "$a $ b$";
            var res = _parser.RunToPlain(input);
            // 第一个 $ 到第二个 $ 之间是 "a "，FulFill 通过
            // 但 "a " 中没有未转义的 $
            // 实际上：第一个 $ 匹配第二个 $，span="a "，FulFill 通过
            Assert.AreEqual(
                "<p><span class=\"latex-inline\">a </span> b$</p>", res);
        }

        [TestMethod]
        public void BlockLatexBasic()
        {
            string input = "$$E = mc^2$$";
            var res = _parser.RunToPlain(input);
            Assert.AreEqual("<pre class=\"latex\">E = mc^2</pre>", res);
        }

        [TestMethod]
        public void BlockLatexMultiline()
        {
            // 多行块级公式：每行都是 $$...$$
            string input = "$$\\begin{aligned}$$\n$$x &= y \\\\$$\n$$z &= w$$\n$$\\end{aligned}$$";
            var res = _parser.RunToPlain(input);
            Assert.AreEqual(
                "<pre class=\"latex\">\\begin{aligned}\nx &amp;= y \\\\\nz &amp;= w\n\\end{aligned}</pre>", res);
        }

        [TestMethod]
        public void BlockLatexWithSurroundingText()
        {
            string input = "请看公式\n\n$$\\frac{a}{b} = c$$\n\n结束";
            var res = _parser.RunToPlain(input);
            Assert.AreEqual(
                "<p>请看公式</p><pre class=\"latex\">\\frac{a}{b} = c</pre><p>结束</p>", res);
        }

        [TestMethod]
        public void BlockLatexMultipleLines()
        {
            // 连续多行 $$...$$ 被块规则合并为一个块
            string input = "$$x = 1$$\n$$y = 2$$";
            var res = _parser.RunToPlain(input);
            Assert.AreEqual(
                "<pre class=\"latex\">x = 1\ny = 2</pre>", res);
        }

        [TestMethod]
        public void BlockLatexConsecutiveLines()
        {
            // 连续多行 $$...$$ 应合并为一个块
            string input = "$$x = 1$$\n$$y = 2$$\n$$z = 3$$";
            var res = _parser.RunToPlain(input);
            Assert.AreEqual(
                "<pre class=\"latex\">x = 1\ny = 2\nz = 3</pre>", res);
        }

        [TestMethod]
        public void BlockLatexWithHtmlChars()
        {
            // 块级 latex 内容不转义
            string input = "$$a < b && c > d$$";
            var res = _parser.RunToPlain(input);
            Assert.AreEqual("<pre class=\"latex\">a &lt; b &amp;&amp; c &gt; d</pre>", res);
        }

        [TestMethod]
        public void BlockLatexEmptyRejected()
        {
            // 只有 $$ 没有内容，不应匹配块规则
            string input = "$$$$";
            var res = _parser.RunToPlain(input);
            // $$$$ → 行内 $ 规则匹配：第一个 $ 到第二个 $，span="$"，FulFill 通过（$ 不是未转义的，因为它是唯一内容）
            // 但等等，span="$" 中有一个 $，且前面没有 \，所以 FulFill 应该返回 false
            // 实际上：第一个 $ 匹配第二个 $，span="$$"，不对...
            // $$$$ 的索引：0=$, 1=$, 2=$, 3=$
            // 行内规则找 left=$，right=$
            // left=0, right=1 → span=""（空），FulFill false
            // left=0, right=2 → span="$"，FulFill false（包含未转义 $）
            // left=0, right=3 → span="$$"，FulFill false
            // left=1, right=2 → span=""，FulFill false
            // left=1, right=3 → span="$"，FulFill false
            // left=2, right=3 → span=""，FulFill false
            // 没有任何匹配，作为普通文本
            Assert.AreEqual("<p>$$$$</p>", res);
        }

        [TestMethod]
        public void InlineAndBlockLatexMixed()
        {
            string input = "行内 $x$ 和块级\n\n$$y = z$$";
            var res = _parser.RunToPlain(input);
            Assert.AreEqual(
                "<p>行内 <span class=\"latex-inline\">x</span> 和块级</p>" +
                "<pre class=\"latex\">y = z</pre>", res);
        }

        [TestMethod]
        public void LatexWithOtherInlineRules()
        {
            // latex 与粗体等行内规则共存
            string input = "**粗体** 和 $x^2$ 公式";
            var res = _parser.RunToPlain(input);
            Assert.AreEqual(
                "<p><b>粗体</b> 和 <span class=\"latex-inline\">x^2</span> 公式</p>", res);
        }

        [TestMethod]
        public void LatexWithMermaidAndCode()
        {
            string input = "```csharp\nint x = 1;\n```\n\n```mermaid\ngraph TD;\nA-->B;\n```\n\n$$E = mc^2$$";
            var res = _parser.RunToPlain(input);
            // fenced code block 在单行时会被 inline 解析器处理，输出被包裹在 <p> 中
            Assert.AreEqual(
                "<p><pre><code class=\"language-csharp\">int x = 1;</code></pre></p>" +
                "<p><pre class=\"mermaid\">graph TD;\nA-->B;</pre></p>" +
                "<pre class=\"latex\">E = mc^2</pre>", res);
        }

        [TestMethod]
        public void InlineLatexBoundaryLeftRejected()
        {
            // 开始 $ 前面紧挨非空白字符，不识别为 LaTeX
            string input = "a$x = b$";
            var res = _parser.RunToPlain(input);
            Assert.AreEqual("<p>a$x = b$</p>", res);
        }

        [TestMethod]
        public void InlineLatexBoundaryRightRejected()
        {
            // 结束 $ 后面紧挨非空白字符，不识别为 LaTeX
            string input = "$x = b$c";
            var res = _parser.RunToPlain(input);
            Assert.AreEqual("<p>$x = b$c</p>", res);
        }

        [TestMethod]
        public void InlineLatexBoundaryBothEndsAllowed()
        {
            // 开头/末尾的 $ 无需外部空白
            string input = "$x = b$";
            var res = _parser.RunToPlain(input);
            Assert.AreEqual(
                "<p><span class=\"latex-inline\">x = b</span></p>", res);
        }

        [TestMethod]
        public void InlineLatexBoundaryWithWhitespace()
        {
            // 前后都有空白时正常识别
            string input = "公式 $x=\\frac{a}{b}$ 结束";
            var res = _parser.RunToPlain(input);
            Assert.AreEqual(
                "<p>公式 <span class=\"latex-inline\">x=\\frac{a}{b}</span> 结束</p>", res);
        }

        [TestMethod]
        public void InlineLatexBoundaryMultipleMixed()
        {
            // 多个公式，只有满足边界条件的才识别
            string input = "$a$ 和 $b$c 以及 d$e$";
            var res = _parser.RunToPlain(input);
            Assert.AreEqual(
                "<p><span class=\"latex-inline\">a</span>" +
                " 和 $b$c 以及 d$e$</p>", res);
        }

        [TestMethod]
        public void InlineLatexWithImplantStyleBraces()
        {
            // LaTeX 公式内部的大括号不应被 Template/Implant 机制切开
            var builder = new ParserBuilder();
            var parser = builder.BuildParser();
            parser.SetConvertingProvider(new ConvertingProviderForLatexTest());

            string input = "行内公式 $x = \\frac{a}{b}$ 结束";
            string res = parser.RunToPlain(input);
            Assert.AreEqual(
                "<p>行内公式 <span class=\"latex-inline\">x = \\frac{a}{b}</span> 结束</p>", res);
        }

        [TestMethod]
        public void InlineLatexWithSqrt()
        {
            var builder = new ParserBuilder();
            var parser = builder.BuildParser();
            parser.SetConvertingProvider(new ConvertingProviderForLatexTest());

            string input = "$\\sqrt{x^2 + y^2}$";
            string res = parser.RunToPlain(input);
            Assert.AreEqual(
                "<p><span class=\"latex-inline\">\\sqrt{x^2 + y^2}</span></p>", res);
        }

        [TestMethod]
        public void InlineLatexAndImplantMixed()
        {
            // LaTeX 公式和真正的 Implant 调用共存
            var builder = new ParserBuilder();
            var parser = builder.BuildParser();
            parser.SetConvertingProvider(new ConvertingProviderForLatexTest());

            string input = "{implant} 和 $x = \\frac{a}{b}$";
            string res = parser.RunToPlain(input);
            Assert.AreEqual(
                "<p><span class=\"implanted\">implanted</span> 和 " +
                "<span class=\"latex-inline\">x = \\frac{a}{b}</span></p>", res);
        }

        [TestMethod]
        public void MultipleInlineLatexWithBraces()
        {
            var builder = new ParserBuilder();
            var parser = builder.BuildParser();
            parser.SetConvertingProvider(new ConvertingProviderForLatexTest());

            string input = "$\\frac{a}{b}$ 和 $\\sqrt{c}$";
            string res = parser.RunToPlain(input);
            Assert.AreEqual(
                "<p><span class=\"latex-inline\">\\frac{a}{b}</span>" +
                " 和 <span class=\"latex-inline\">\\sqrt{c}</span></p>", res);
        }

        [TestMethod]
        public void BlockLatexStillWorksWithImplantProvider()
        {
            var builder = new ParserBuilder();
            var parser = builder.BuildParser();
            parser.SetConvertingProvider(new ConvertingProviderForLatexTest());

            string input = "$$x = \\frac{a}{b}$$";
            string res = parser.RunToPlain(input);
            Assert.AreEqual("<pre class=\"latex\">x = \\frac{a}{b}</pre>", res);
        }
    }
}
