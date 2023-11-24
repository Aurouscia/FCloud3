using FCloud3.HtmlGen.Mechanics;
using FCloud3.HtmlGen.Options;
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
        private readonly HtmlGenOptions _options;
        public TemplateParsingTest()
        {
            HtmlGenOptionsProvider provider = new(
                templates: new()
                {
                    new()
                    {
                        Name = "重点强调",
                        Source = "<b>!![[文字]]!!</b>"
                    },
                    new()
                    {
                        Name = "名称信息",
                        Source = "<div><b>[[中文名]]</b><i>[[英文名]]</i></div>"
                    }
                },
                inlineRules:new(),
                blockRules:new()
            );
            _options = provider.GetOptions();
        }
        [TestMethod]
        [DataRow(
            "123{{456}789}000",
            "123,{456}789,000")]
        [DataRow(
            "AA{{1}2}BB{{3}4}CC",
            "AA,{1}2,BB,{3}4,CC")]
        [DataRow(
            "{{1}2}BB{{3}4}CC",
            ",{1}2,BB,{3}4,CC")]
        [DataRow(
            "AA{{1}2}BB{{3}4}",
            "AA,{1}2,BB,{3}4,")]
        [DataRow(
            "AA{{1}2}{{3}4}CC",
            "AA,{1}2,,{3}4,CC")]
        [DataRow(
            "{{1}2}{{3}4}",
            ",{1}2,,{3}4,")]
        [DataRow(
            "123{{45{MM}6}789}000",
            "123,{45{MM}6}789,000")]
        [DataRow("ABC","ABC")]
        public void CallSplit(string input, string answer)
        {
            var res = TemplateParser.SplitByCalls(input);
            var answers = answer.Split(',');
            var result = res.OrderedFrags.ConvertAll(x => x.Content);
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
            "{{名称信息}\n中文名::充电宝\n&&英文名：：Power Baby}",
            "<div><b>充电宝</b><i>Power Baby</i></div>")]
        public void ParseTemplate(string input, string answer)
        {
            TemplateParser parser = new(_options);
            string output = parser.Run(input).ToHtml();
            Assert.AreEqual(answer, output);
        }
    }
}
