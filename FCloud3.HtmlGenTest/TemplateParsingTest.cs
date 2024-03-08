using FCloud3.HtmlGen.Context;
using FCloud3.HtmlGen.Mechanics;
using FCloud3.HtmlGen.Options;
using FCloud3.HtmlGen.Rules;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.HtmlGenTest
{
    [TestClass]
    public class TemplateParsingTest
    {
        private readonly ParserOptions _options;
        private readonly ParserContext _ctx;
        public string? GenLinkForWiki(string name)
        {
            if (name == "哼哼")
                return "<a href=\"/w/114514\">恶臭</a>";
            if (name == "efg")
                return "<a href=\"/w/666\">efg666</a>";
            return null;
        }
        public TemplateParsingTest()
        {
            var templates = new List<Template>()
                {
                    new("重点强调","<b>!![[__文字__]]!!</b>"),
                    new("名称信息", "<div><b>[[__中文名__]]</b><i>[[__英文名__]]</i></div>"),
                    new("好看的图表","<script>const rawData='[[[__数据__]]]'</script>"),
                    new("唯一id测试","<div id=\"[[__%id%__]]\"><script>doc.get('[[__%id%__]]')</script>"),
                    new("唯一id测试2","<div id=\"[[__%id%__]]\"><script>doc.get('[[__%id%__]]')</script>"),
                    new("唯一id测试3","<div id=\"[[__%ik%__]]\"><script>doc.get('[[__%ik%__]]')</script>")
                };
            _options = new ParserBuilder()
                .Template.AddTemplates(templates)
                .Implant.AddImplantsHandler(GenLinkForWiki)
                .GetCurrentOptions();
            _ctx = new(_options);
        }
        [TestMethod]
        [DataRow(
            "123{{456}789}000",
            "123,{{456}789},000")]
        [DataRow(
            "AA{{1}2}BB{{3}4}CC",
            "AA,{{1}2},BB,{{3}4},CC")]
        [DataRow(
            "{{1}2}BB{{3}4}CC",
            ",{{1}2},BB,{{3}4},CC")]
        [DataRow(
            "AA{{1}2}BB{{3}4}",
            "AA,{{1}2},BB,{{3}4},")]
        [DataRow(
            "AA{{1}2}{{3}4}CC",
            "AA,{{1}2},,{{3}4},CC")]
        [DataRow(
            "{{1}2}{{3}4}",
            ",{{1}2},,{{3}4},")]
        [DataRow(
            "123{{45{MM}6}789}000",
            "123,{{45{MM}6}789},000")]
        [DataRow(
            "ABC",
            "ABC")]
        [DataRow(
            "123{aaa}456",
            "123,{aaa},456")]
        public void CallSplit(string input, string answer)
        {
            var res = TemplateParser.SplitByCalls(input);
            var answers = answer.Split(',');
            var result = res.Frags.ConvertAll(x => x.Content);
            CollectionAssert.AreEqual(answers, result);
        }

        [TestMethod]
        [DataRow(
            "{123}456","123,456")]
        [DataRow(
            "{123}", "123,")]
        public void CallNameExtract(string input,string answer)
        {
            TemplateParser.ExtractCallName(input, out string name, out string content);
            var answers = answer.Split(',');
            Assert.AreEqual(answers[0], name);
            Assert.AreEqual(answers[1], content);
        }

        [TestMethod]
        [DataRow(
            "这里我要说一件事，{{重点强调} 文字 ::\t不准写\n敏感内容\t}，明白了吗",
            "这里我要说一件事，<b>!!<p>不准写</p><p>敏感内容</p>!!</b>，明白了吗"
            )]
        [DataRow(
            "这里我要说一件事，{{重点强调} 文字 ::\t不准写敏感内容\t}，明白了吗",
            "这里我要说一件事，<b>!!不准写敏感内容!!</b>，明白了吗"
            )]
        [DataRow(
            "{{名称信息}\n中文名::充电宝}",
            "<div><b>充电宝</b><i></i></div>")]
        [DataRow(
            "{{名称信息}\n中文名::充电宝\n &amp;&amp; 英文名：：Power Baby}",
            "<div><b>充电宝</b><i>Power Baby</i></div>")]
        [DataRow(
            "{{好看的图表}\n数据::172,163,105*144*97}",
            "<script>const rawData='172,163,105*144*97'</script>")]
        [DataRow(
            "{{好看的图表}172,163,105*144*97}",
            "<script>const rawData='172,163,105*144*97'</script>")]
        [DataRow(
            "{{好看的图表}\n数据::172,163\n105*144*97}",
            "<script>const rawData='172,163\n105*144*97'</script>")]
        public void ParseTemplate(string input, string answer)
        {
            TemplateParser parser = new(_ctx);
            string output = parser.Run(input).ToHtml();
            Assert.AreEqual(answer, output);
            string output2 = parser.Run(input).ToHtml();
            Assert.AreEqual(answer, output2);
            string output3 = parser.Run(input).ToHtml();
            Assert.AreEqual(answer, output3);
        }



        [TestMethod]
        [DataRow(
            "{{唯一id测试}哼哼~}",
            "<div id=\"id_0\"><script>doc.get('id_0')</script>"
            )]
        [DataRow(
            "{{唯一id测试}}哼哼哼{{唯一id测试}}",
            "<div id=\"id_0\"><script>doc.get('id_0')</script>哼哼哼<div id=\"id_1\"><script>doc.get('id_1')</script>"
            )]
        [DataRow(
            "{{唯一id测试}}哼哼哼{{唯一id测试}}哼哼{{唯一id测试}}",
            "<div id=\"id_0\"><script>doc.get('id_0')</script>哼哼哼" +
            "<div id=\"id_1\"><script>doc.get('id_1')</script>哼哼" +
            "<div id=\"id_2\"><script>doc.get('id_2')</script>"
            )]
        [DataRow(
            "{{唯一id测试}}哼哼哼{{唯一id测试2}}",
            "<div id=\"id_0\"><script>doc.get('id_0')</script>哼哼哼<div id=\"id_1\"><script>doc.get('id_1')</script>"
            )]
        [DataRow(
            "{{唯一id测试}}哼哼哼{{唯一id测试3}}",
            "<div id=\"id_0\"><script>doc.get('id_0')</script>哼哼哼<div id=\"ik_1\"><script>doc.get('ik_1')</script>"
            )]
        public void UniqueSlotTest(string input,string answer)
        {
            TemplateParser parser = new(_ctx);
            string output = parser.Run(input).ToHtml();
            Assert.AreEqual(answer, output);
            _ctx.Reset();
            string output2 = parser.Run(input).ToHtml();
            Assert.AreEqual(answer, output2);
            _ctx.Reset();
            string output3 = parser.Run(input).ToHtml();
            Assert.AreEqual(answer, output3);
        }

        [TestMethod]
        [DataRow(
            "123{efg}456",
            "123<a href=\"/w/666\">efg666</a>456")]
        [DataRow(
            "123{哼哼}456",
            "123<a href=\"/w/114514\">恶臭</a>456")]
        [DataRow(
            "{{名称信息}\n中文名::充电宝\n &amp;&amp; 英文名：：Power{哼哼}Baby}",
            "<div><b>充电宝</b><i>Power<a href=\"/w/114514\">恶臭</a>Baby</i></div>")]
        public void ParseImplant(string input,string answer)
        {
            TemplateParser parser = new(_ctx);
            string output = parser.Run(input).ToHtml();
            Assert.AreEqual(answer, output);
            string output2 = parser.Run(input).ToHtml();
            Assert.AreEqual(answer, output2);
            string output3 = parser.Run(input).ToHtml();
            Assert.AreEqual(answer, output3);
        }
    }
}
