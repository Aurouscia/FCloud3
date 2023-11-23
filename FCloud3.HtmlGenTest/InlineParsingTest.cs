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
    public class InlineParsingTest
    {
        private readonly HtmlGenOptions _options;

        public InlineParsingTest()
        {
            List<HtmlInlineRule> inlineRules = new()
                {
                    new()
                    {
                        Name = "斜体",
                        MarkLeft = "*",
                        MarkRight = "*",
                        PutLeft = "<i>",
                        PutRight = "</i>",
                    },
                    new()
                    {
                        Name = "粗体",
                        MarkLeft = "**",
                        MarkRight = "**",
                        PutLeft = "<b>",
                        PutRight = "</b>",
                    }
                };
            inlineRules.Sort((x, y) => y.MarkLeft.Length - x.MarkRight.Length);

            HtmlGenOptions options = new()
            {
                InlineRules = inlineRules,
                Templates = new(),
                TypedBlockRules = new()
            };
            _options = options;
        }

        [TestMethod]
        [DataRow("12**34**56","2,6")]
        [DataRow("1*2**34**5*6","3,7;1,10")]
        [DataRow("1**2*34*5**6", "1,9;4,7")]
        public void MakeMark(string input,string answer)
        {
            InlineParser parser = new(_options);
            var res = parser.MakeMarks(input);
            var resStrs = res.ConvertAll(x => $"{x.LeftIndex},{x.RightIndex}");
            string result = string.Join(';',resStrs);
            Assert.AreEqual(answer,result);
        }

        [TestMethod]
        [DataRow("12*34*56","12<i>34</i>56")]
        [DataRow("12**34**56","12<b>34</b>56")]
        [DataRow("1*2*34*5*6", "1<i>2</i>34<i>5</i>6")]
        [DataRow("1*2**34**5*6", "1<i>2<b>34</b>5</i>6")]
        [DataRow("1**2*34*5**6", "1<b>2<i>34</i>5</b>6")]
        public void Parse(string input,string answer)
        {
            InlineParser parser = new(_options);
            var res = parser.Run(input);
            var html = res.ToHtml();
            Assert.AreEqual(answer,html);
        }
    }
}
