using FCloud3.WikiPreprocessor.Context;
using FCloud3.WikiPreprocessor.Mechanics;
using FCloud3.WikiPreprocessor.Options;
using FCloud3.WikiPreprocessor.Rules;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.WikiPreprocessor.Test
{
    [TestClass]
    public class InlineParsingTest
    {
        private readonly ParserContext _ctx;

        public InlineParsingTest()
        {
            var options = new ParserBuilder().GetCurrentOptions();
            _ctx = new(options);
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
        [DataRow("小王 \\bd 小李 \\bd 小张", "小王 <span class=\"bordered\"> 小李 </span> 小张")]
        public void Parse(string input,string answer)
        {
            _ctx.SetInitialFrameCount();
            InlineParser parser = new(_ctx);
            var res = parser.Run(input);
            var html = res.ToHtml();
            Assert.AreEqual(answer,html);
            var html2 = res.ToHtml();
            Assert.AreEqual(answer, html2);
            var html3 = res.ToHtml();
            Assert.AreEqual(answer, html3);
        }

        [TestMethod]
        [DataRow("小王 #255,0,0\\@小李# 小张", "小王 <span class=\"coloredText\" style=\"color:rgb(255,0,0)\">小李</span> 小张")]
        [DataRow("小王 #rgb(255,0,0)\\@小李# 小张", "小王 <span class=\"coloredText\" style=\"color:rgb(255,0,0)\">小李</span> 小张")]
        [DataRow("小王 #255,0,0# 小张", "小王 <span class=\"coloredBlock\" style=\"background-color:rgb(255,0,0)\"></span> 小张")]
        [DataRow("小王 #rgb(255,0,0)# 小张", "小王 <span class=\"coloredBlock\" style=\"background-color:rgb(255,0,0)\"></span> 小张")]
        [DataRow("小王 #ffff00\\@小李# 小张", "小王 <span class=\"coloredText\" style=\"color:rgb(255,255,0)\">小李</span> 小张")]
        [DataRow("小王 #ffff00# 小张", "小王 <span class=\"coloredBlock\" style=\"background-color:rgb(255,255,0)\"></span> 小张")]
        [DataRow("小王 #rgbbbbb# 小张", "小王 #rgbbbbb# 小张")]
        [DataRow("小王 #aeu*i*hea# 小张", "小王 #aeu<i>i</i>hea# 小张")]
        [DataRow("小王 #aeu*i*hea\\@嘿嘿*嘿*嘿# 小张", "小王 #aeu<i>i</i>hea@嘿嘿<i>嘿</i>嘿# 小张")]
        [DataRow("小王 #cornFlowerBlue\\@嘿嘿*嘿*嘿# 小张", "小王 <span class=\"coloredText\" style=\"color:rgb(100,149,237)\">嘿嘿<i>嘿</i>嘿</span> 小张")]
        [DataRow("小王 #beige# 小张", "小王 <span class=\"coloredBlock\" style=\"background-color:rgb(245,245,220)\"></span> 小张")]
        [DataRow("#111111\\@\\#111111#", "<span class=\"coloredText\" style=\"color:rgb(17,17,17)\">#111111</span>")]
        public void ColorTextParse(string input,string answer)
        {
            _ctx.SetInitialFrameCount();
            InlineParser parser = new(_ctx);
            var res = parser.Run(input);
            var html = res.ToHtml();
            Assert.AreEqual(answer, html);
            var html2 = res.ToHtml();
            Assert.AreEqual(answer, html2);
            var html3 = res.ToHtml();
            Assert.AreEqual(answer, html3);
        }

        [TestMethod]
        [DataRow("看看这个[ http://img.png]", "看看这个<img src=\"http://img.png\" style=\"float:right;height:5em;\"/>")]
        [DataRow("看看这个[http://img.png|8]", "看看这个<img src=\"http://img.png\" style=\"float:right;height:8em;\"/>")]
        [DataRow("看看这个[http://img.png|9 |xxx]", "看看这个<img src=\"http://img.png\" style=\"float:right;height:9em;\"/>")]
        [DataRow("看看这个[http://img.png|9 | leFt]", "看看这个<img src=\"http://img.png\" style=\"float:left;height:9em;\"/>")]
        [DataRow("看看这个[http://img.svg|100px]", "看看这个<img src=\"http://img.svg\" style=\"float:right;height:100px;\"/>")]
        [DataRow("看看这个[http://ad.mp3]", "看看这个<audio controls src=\"http://ad.mp3\" style=\"float:right;height:5em;\"></audio>")]
        [DataRow("看看这个[http://vd.webm]", "看看这个<video controls src=\"http://vd.webm\" style=\"float:right;height:5em;\"></video>")]
        public void InlineImageParse(string input,string answer)
        {
            _ctx.SetInitialFrameCount();
            InlineParser parser = new(_ctx);
            var res = parser.Run(input);
            var html = res.ToHtml();
            Assert.AreEqual(answer, html);
            var html2 = res.ToHtml();
            Assert.AreEqual(answer, html2);
            var html3 = res.ToHtml();
            Assert.AreEqual(answer, html3);
        }
    }
}
