using FCloud3.WikiPreprocessor.Context;
using FCloud3.WikiPreprocessor.Mechanics;
using FCloud3.WikiPreprocessor.Options;
using FCloud3.WikiPreprocessor.Rules;
using FCloud3.WikiPreprocessor.Test.Support;

namespace FCloud3.WikiPreprocessor.Test
{
    internal class DataSourceForInlineTest : DataSourceBase
    {
        public override string? Implant(string implantSpan)
        {
            return GenLinkForWiki(implantSpan);
        }
        private static string? GenLinkForWiki(string name)
        {
            if (name == "哼")
                return "<a href=\"/w/114514\">恶臭</a>";
            return null;
        }
    }
    
    [TestClass]
    public class InlineParsingTest
    {
        private readonly ParserContext _ctx;

        public InlineParsingTest()
        {
            var options = new ParserBuilder()
                    .GetCurrentOptions();
            _ctx = new ParserContext(options);
            _ctx.SetDataSource(new DataSourceForInlineTest());
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
        [DataRow("1*2*34*5*6", "1<i>2</i>34<i>5</i>6")]
        [DataRow("1*2**34**5*6", "1<i>2<b>34</b>5</i>6")]
        [DataRow("1**2*34*5**6", "1<b>2<i>34</i>5</b>6")]
        [DataRow("1[/234]5", "1<a href=\"/234\">/234</a>5")]
        [DataRow("1\\[/234\\]5", "1[/234]5")]
        [DataRow("1[/2*3*4]5","1<a href=\"/2*3*4\">/2*3*4</a>5")]
        [DataRow("1[哼唧](/234)5", "1<a href=\"/234\">哼唧</a>5")]
        [DataRow("小王 \\bd 小李 \\bd 小张", "小王 <span class=\"bordered\"> 小李 </span> 小张")]
        [DataRow("\\反斜杠不应该全部去掉\\，应该仅去掉\\*规则\\*前的","\\反斜杠不应该全部去掉\\，应该仅去掉*规则*前的")]
        [DataRow("\\[xx\\]，\\|\\-\\~\\a", "[xx]，|-~\\a")]
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
        [DataRow("小王 #aeu*i*hea\\@嘿嘿*嘿*嘿# 小张", "小王 #aeu<i>i</i>hea\\@嘿嘿<i>嘿</i>嘿# 小张")]
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

        [TestMethod]
        [DataRow("__%%12345%%__", "__<>12345</>__", 6)]
        [DataRow("__%%123456%%__", "__<>123456</>__", 6)]
        [DataRow("__%%1234567%%__", "__%%1234567%%__", 6)]
        [DataRow("__%%12345678%%__", "__%%12345678%%__", 6)]
        public void LengthRestriction(string input, string answer, int maxLengthBetween)
        {
            var customInlineRule = new CustomInlineRule(
                "%%", "%%", "<>", "</>","","", maxLengthBetween);
            _ctx.SetInitialFrameCount();
            _ctx.Options.InlineParsingOptions.AddMoreRule(customInlineRule);
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
        [DataRow("1234{哼}5678", "1234<a href=\"/w/114514\">恶臭</a>5678")]
        [DataRow("{哼}", "<a href=\"/w/114514\">恶臭</a>")]
        public void ShortImplantTest(string input, string answer)
        {
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
