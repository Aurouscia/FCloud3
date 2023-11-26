using FCloud3.HtmlGen.Options;
using FCloud3.HtmlGen.Models;
using FCloud3.HtmlGen.Rules;

namespace FCloud3.HtmlGenTest
{
    [TestClass]
    public class TemplateSourceTest
    {
        [TestMethod]
        [DataRow("<h1>[[名称]]</h1><p>[[介绍]]</p>[[备注]]","名称,介绍,备注")]
        [DataRow("[[第一段]][[第二段]][[第三段]]!", "第一段,第二段,第三段")]
        [DataRow("<[[%错误空]]><[[正确空]]>[[ 错误空2]]", "正确空")]
        [DataRow("[[AA]]-[[BB]]-[[AA]]-[[CC]]-[[D D]]", "AA,BB,CC")]
        public void GettingSlots(string source,string answer)
        {
            HtmlTemplate template = new("",source);
            var slots = template.GetSlots();
            var answers = answer.Split(',');
            CollectionAssert.AreEqual(answers, slots);
        }

        [TestMethod]
        [DataRow(
            "<h1>[[名称]]</h1>",
            "名称:哼唧",
            "<h1>哼唧</h1>"
            )]
        [DataRow(
            "<h1>[[名称]]</h1><p>[[介绍]]</p>[[备注]]",
            "名称:哼唧,介绍:世界上最可爱的哼唧,备注:数据截至昨天",
            "<h1>哼唧</h1><p>世界上最可爱的哼唧</p>数据截至昨天"
            )]
        [DataRow(
            "<div>[[AA]]</div><p>[[BB]]</p>[[CC]][[AA]]",
            "AA:哼唧,BB:咪咕,CC:噜噜",
            "<div>哼唧</div><p>咪咕</p>噜噜哼唧"
            )]
        public void Filling(string source,string values, string answer) 
        {
            HtmlTemplate template = new("模板名称", source);
            var valuesDic = values.Split(',').ToDictionary(x => x.Split(':')[0], x => new ElementCollection(new TextElement(x.Split(':')[1])));
            TemplateElement element = new(template,valuesDic,new());
            string result = element.ToHtml();
            Assert.AreEqual(answer, result);
        }
    }
}