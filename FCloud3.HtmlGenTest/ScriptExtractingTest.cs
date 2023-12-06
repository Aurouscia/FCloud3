using FCloud3.HtmlGen.Mechanics;
using FCloud3.HtmlGen.Options;
using FCloud3.HtmlGen.Options.SubOptions;
using FCloud3.HtmlGen.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.HtmlGenTest
{
    [TestClass]
    public class ScriptExtractingTest
    {
        private readonly Parser _parser;
        public ScriptExtractingTest() 
        {
            List<Template> templates = new()
            {
                new Template("模板A","ABCDE<script>需要填写[[[__m__]]]的代码</script>FGHIJ","样式样式","前代码","后代码"),
                new Template("模板B","ABCDE<script type=\"javascript\">需要填写[[[__m__]]]的代码[[__%id%__]]号</script>FGHIJ","样式样式B","前代码B","后代码")
            };
            _parser = new ParserBuilder()
                .Template.AddTemplates(templates)
                .BuildParser();

        }
        [TestMethod]
        [DataRow(
            "12345{{模板A}M}67890",
            "<p>12345ABCDEFGHIJ67890</p>",
            "前代码","需要填写M的代码\n后代码","样式样式"
            )]
        [DataRow(
            "123{{模板A}M1}456{{模板B}M2}789",
            "<p>123ABCDEFGHIJ456ABCDEFGHIJ789</p>",
            "前代码\n前代码B","需要填写M1的代码\n需要填写M2的代码id_1号\n后代码","样式样式样式样式B"
            )]
        [DataRow(
            "123{{模板A}M3}456<script>自定义代码</script>789",
            "<p>123ABCDEFGHIJ456789</p>",
            "前代码","需要填写M3的代码\n自定义代码\n后代码","样式样式"
            )]
        public void Test1(string input,string content,string preScripts,string postScripts,string styles)
        {
            var res = _parser.RunToStructured(input);
            Assert.AreEqual(content, res.Content);
            Assert.AreEqual(preScripts, res.PreScript);
            Assert.AreEqual(postScripts, res.PostScript);
            Assert.AreEqual(styles, res.Style);
        }
    }
}
