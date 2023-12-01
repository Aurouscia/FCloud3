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
    public class InlineParsingTest
    {
        private readonly HtmlGenOptions _options;
        private readonly HtmlGenContext _ctx;

        public InlineParsingTest()
        {
            HtmlGenOptionsBuilder optionsBuilder = new(
                templates: new(),
                extraInlineRules: new(),
                extraBlockRules: new()
            );
            _options = optionsBuilder.GetOptions();
            _ctx = new(_options);
        }

        [TestMethod]
        [DataRow("12**34**56","2,6")]
        [DataRow("1*2**34**5*6","3,7;1,10")]
        [DataRow("1**2*34*5**6", "1,9;4,7")]
        public void MakeMark(string input,string answer)
        {
            InlineParser parser = new(_ctx);
            var res = parser.MakeMarks(input);
            var resStrs = res.ConvertAll(x => $"{x.LeftIndex},{x.RightIndex}");
            string result = string.Join(';',resStrs);
            Assert.AreEqual(answer,result);
        }

        [TestMethod]
        [DataRow("12*34*56","12<i>34</i>56")]
        [DataRow("12**34**56","12<b>34</b>56")]
        [DataRow("12\\*34\\*56", "12*34*56")]
        [DataRow("12\\\\*34\\\\*56", "12\\*34\\*56")]
        [DataRow("12**34**56", "12<b>34</b>56")]
        [DataRow("1*2*34*5*6", "1<i>2</i>34<i>5</i>6")]
        [DataRow("1*2**34**5*6", "1<i>2<b>34</b>5</i>6")]
        [DataRow("1**2*34*5**6", "1<b>2<i>34</i>5</b>6")]
        [DataRow("1[/234]5", "1<a href=\"/234\">/234</a>5")]
        [DataRow("1\\[/234\\]5", "1[/234]5")]
        [DataRow("1[/2*3*4]5","1<a href=\"/2*3*4\">/2*3*4</a>5")]
        [DataRow("1[哼唧](/234)5", "1<a href=\"/234\">哼唧</a>5")]
        [DataRow("小王 \\bd 小李 \\bd 小张", "小王<span class=\"bordered\">小李</span>小张")]
        public void Parse(string input,string answer)
        {
            InlineParser parser = new(_ctx);
            var res = parser.Run(input);
            var html = res.ToHtml();
            Assert.AreEqual(answer,html);
        }

        [TestMethod]
        [DataRow("小王 #255,0,0\\@小李# 小张", "小王<span class=\"coloredText\" style=\"color:rgb(255,0,0)\">小李</span>小张")]
        [DataRow("小王 #255,0,0# 小张", "小王<span class=\"coloredBlock\" style=\"color:rgb(255,0,0);background-color:rgb(255,0,0)\"></span>小张")]
        [DataRow("小王 #ffff00\\@小李# 小张", "小王<span class=\"coloredText\" style=\"color:#ffff00\">小李</span>小张")]
        [DataRow("小王 #ffff00# 小张", "小王<span class=\"coloredBlock\" style=\"color:#ffff00;background-color:#ffff00\"></span>小张")]
        [DataRow("小王 #aeu*i*hea# 小张", "小王#aeu<i>i</i>hea#小张")]
        [DataRow("小王 #aeu*i*hea\\@嘿嘿*嘿*嘿# 小张", "小王#aeu<i>i</i>hea@嘿嘿<i>嘿</i>嘿#小张")]
        [DataRow("小王 #cornFlowerBlue\\@嘿嘿*嘿*嘿# 小张", "小王<span class=\"coloredText\" style=\"color:cornflowerblue\">嘿嘿<i>嘿</i>嘿</span>小张")]
        [DataRow("小王 #beige# 小张", "小王<span class=\"coloredBlock\" style=\"color:beige;background-color:beige\"></span>小张")]
        public void ColorTextParse(string input,string answer)
        {
            InlineParser parser = new(_ctx);
            var res = parser.Run(input);
            var html = res.ToHtml();
            Assert.AreEqual(answer, html);
        }
    }
}
