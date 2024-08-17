using FCloud3.WikiPreprocessor.Mechanics;
using FCloud3.WikiPreprocessor.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.WikiPreprocessor.Test
{
    [TestClass]
    public class AutoReplaceTest
    {
        private readonly Parser _parser;
        private readonly Dictionary<string,int> wikis_1 = new()
        {
            {  "3C教育体系大纲",6 },
            {  "咪么", 28 },
            {  "咪么么么" ,14 },
            {  "拍拍拍拿放", 73 }
        };
        private readonly Dictionary<string,int> wikis_2 = new()
        {
            {  "夜莺",5 },
            {  "帝企鹅", 388 }
        };
        private string MakeUrlForWiki(string title)
        {
            if(wikis_1.TryGetValue(title,out int id1))
                return $"<a href=\"/w/{id1}\">{title}</a>";
            if(wikis_2.TryGetValue(title,out int id2))
                return $"<a href=\"/w/{id2}\">{title}</a>";
            return title ;
        }
        public AutoReplaceTest()
        {
            var optionsBuilder = new ParserBuilder()
                .AutoReplace.AddReplacing(
                    wikis_1.Select(x => x.Key).ToList(),
                    MakeUrlForWiki
                );
            _parser = optionsBuilder.BuildParser();
        }

        [TestMethod]
        [DataRow(
            "更多有趣内容见3C教育体系大纲等词条",
            "<p>更多有趣内容见<a href=\"/w/6\">3C教育体系大纲</a>等词条</p>")]
        [DataRow(
            "更多有趣内容*见3C教育体系大纲等词条*吧",
            "<p>更多有趣内容<i>见<a href=\"/w/6\">3C教育体系大纲</a>等词条</i>吧</p>")]
        [DataRow(
            "更多有趣内容见3C教育体系大纲和3C教育体系大纲第二版等词条",
            "<p>更多有趣内容见<a href=\"/w/6\">3C教育体系大纲</a>和3C教育体系大纲第二版等词条</p>")]
        [DataRow(
            "更多有趣内容*见3C教育体系大纲*和3C教育体系大纲第二版等词条",
            "<p>更多有趣内容<i>见<a href=\"/w/6\">3C教育体系大纲</a></i>和3C教育体系大纲第二版等词条</p>")]
        [DataRow(
            "Au一边喊着\"咪咪么么么么\"，一边把小兔子拍拍拍拿放",
            "<p>Au一边喊着\"咪<a href=\"/w/14\">咪么么么</a>么\"，一边把小兔子<a href=\"/w/73\">拍拍拍拿放</a></p>")]
        [DataRow(
            "咪么",
            "<p><a href=\"/w/28\">咪么</a></p>")]
        public void Test(string input, string answer)
        {
            string res = _parser.RunToPlain(input);
            Assert.AreEqual(answer, res);
            string res2 = _parser.RunToPlain(input);
            Assert.AreEqual(answer, res2);
            string res3 = _parser.RunToPlain(input);
            Assert.AreEqual(answer, res3);
        }

        [TestMethod]
        [DataRow(
            "更多有趣内容见3C教育体系大纲等词条",
            "<p>更多有趣内容见3C教育体系大纲等词条</p>", true)]
        [DataRow(
            "更多有趣内容见3C教育体系大纲等词条",
            "<p>更多有趣内容见<a href=\"/w/6\">3C教育体系大纲</a>等词条</p>", false)]
        [DataRow(
            "夜莺是人类伴生种，夜莺会吃城市里的其他小鸟",
            "<p><a href=\"/w/5\">夜莺</a>是人类伴生种，夜莺会吃城市里的其他小鸟</p>", true)]
        public void ChangeTargets(string input, string answer, bool clear)
        {
            _parser.Context.AutoReplace.Register(wikis_2.Keys.ToList(), true, clear);
            string res = _parser.RunToPlain(input);
            Assert.AreEqual(answer, res);
            string res2 = _parser.RunToPlain(input);
            Assert.AreEqual(answer, res2);
            string res3 = _parser.RunToPlain(input);
            Assert.AreEqual(answer, res3);
        }
        
        [TestMethod]
        [DataRow(
            "夜莺是人类伴生种，夜莺会吃城市里的其他小鸟",
            "<p><a href=\"/w/5\">夜莺</a>是人类伴生种，<a href=\"/w/5\">夜莺</a>会吃城市里的其他小鸟</p>")]
        public void NoSingleUse(string input, string answer)
        {
            _parser.Context.AutoReplace.Register(wikis_2.Keys.ToList(), false, true);
            string res = _parser.RunToPlain(input);
            Assert.AreEqual(answer, res);
            string res2 = _parser.RunToPlain(input);
            Assert.AreEqual(answer, res2);
            string res3 = _parser.RunToPlain(input);
            Assert.AreEqual(answer, res3);
        }
    }
}
