using FCloud3.WikiPreprocessor.Options;
using FCloud3.WikiPreprocessor.Models;
using FCloud3.WikiPreprocessor.Rules;
using FCloud3.WikiPreprocessor;

namespace FCloud3.WikiPreprocessor.Test
{
    [TestClass]
    public class TemplateSourceTest
    {
        [TestMethod]
        [DataRow("<h1>[[__名称__]]</h1><p>[[__介绍__]]</p>[[__备注__]]","名称,介绍,备注")]
        [DataRow("[[__第一段__]][[__第二段__]][[__第三段__]][[__第四段_]_]!", "第一段,第二段,第三段")]
        [DataRow("<[[__%错误空__]]><[[__正确空__]]>[[__ 错误空2__]]", "正确空")]
        [DataRow("<[[__%正确空%__]]><[[__正确空2__]]>[[__%错%误空%__]]", "正确空,正确空2")]
        [DataRow("[[__AA__]]-[[__BB__]]-[[__AA__]]-[[__CC__]]-[[__D D__]]", "AA,BB,AA,CC")]
        public void GettingSlots(string source,string answer)
        {
            Template template = new("模板名称",source);
            var slots = new TemplateSlotInfo().Get(template);
            var answers = answer.Split(',');
            CollectionAssert.AreEquivalent(answers, slots.Select(x=>x.PureValue).ToList());
        }

        [TestMethod]
        [DataRow(
            "<h1>[[__名称__]]</h1>",
            "名称:哼唧",
            "<h1>哼唧</h1>"
            )]
        [DataRow(
            "<h1>[[__名称__]]</h1><p>[[__介绍__]]</p><i>[[__备注__]]</i>",
            "名称:哼唧,介绍:世界上最可爱的哼唧,备注:数据截至昨天",
            "<h1>哼唧</h1><p>世界上最可爱的哼唧</p><i>数据截至昨天</i>"
            )]
        [DataRow(
            "<div>[[__AA__]]</div><p>[[__BB__]]</p>[[__CC__]][[__AA__]]",
            "AA:哼唧,BB:咪咕,CC:噜噜",
            "<div>哼唧</div><p>咪咕</p>噜噜哼唧"
            )]
        public void Filling(string source,string values, string answer) 
        {
            Template template = new("模板名称", source);
            var valuesDic = values.Split(',').ToDictionary(x => new ParseBlockSlot("[[__"+x.Split(':')[0]+"__]]",4) as TemplateSlot, x => new TextElement(x.Split(':')[1]) as IHtmlable);
            TemplateElement element = new(template,valuesDic,new());
            string result = element.ToHtml();
            Assert.AreEqual(answer, result);
        }
    }
}