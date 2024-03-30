using FCloud3.HtmlGen.Context;
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
    public class CacheTest
    {
        [DataRow(
            ">怎么的？想打架？\n他这么**恶狠狠**地说到\n---\n没过多久，他就被抬上了救护车",0,
            ">怎么的？想打架？\n他这么**恶狠狠**地说到\n---\n没过多久，他就被抬上了救护车",3,
            ">怎么的？想打架？\n他这么**狠狠**地说到\n----\n没过多久，他就被抬上了救护车",2)]
        [TestMethod]
        public void Inclusive(
            string input1, int count1,
            string input2, int count2,
            string input3, int count3)
        {
            var builder = new ParserBuilder();
            var options = builder.GetCurrentOptions();
            var ctx = new ParserContext(options);
            var parser = new Parser(ctx);
            parser.RunToPlain(input1);
            Assert.AreEqual(count1, ctx.Caches.CacheReadCount);
            parser.RunToPlain(input2);
            Assert.AreEqual(count2, ctx.Caches.CacheReadCount);
            parser.RunToPlain(input3);
            Assert.AreEqual(count3, ctx.Caches.CacheReadCount);
        }

        //[TestMethod]
        //[DataRow(
        //    "我喜欢吃苹果，苹果\n最好吃",
        //    "<p>我喜欢吃apple，苹果</p><p>最好吃</p>",
        //    "我喜欢吃苹果，苹果\n苹果最好吃",
        //    "<p>我喜欢吃apple，苹果</p><p>苹果最好吃</p>",
        //    1 )]
        //[DataRow(
        //    "我喜欢吃苹果\n苹果最好吃",
        //    "<p>我喜欢吃apple</p><p>苹果最好吃</p>",
        //    "我喜欢吃苹\n苹果最好吃",
        //    "<p>我喜欢吃苹</p><p>apple最好吃</p>",
        //    1)]
        //[DataRow(
        //    "我喜欢吃\n苹果最好吃",
        //    "<p>我喜欢吃</p><p>apple最好吃</p>",
        //    "我喜欢吃苹果\n苹果最好吃",
        //    "<p>我喜欢吃apple</p><p>苹果最好吃</p>",
        //    1 )]
        //[DataRow(
        //    "我喜欢吃\n*苹果最*好吃",
        //    "<p>我喜欢吃</p><p><i>apple最</i>好吃</p>",
        //    "我喜欢吃苹果\n*苹果最*好吃",
        //    "<p>我喜欢吃apple</p><p><i>苹果最</i>好吃</p>",
        //    1)]
        //public void SingleUseRule(string input1, string answer1, string input2, string answer2, int cacheReadCount)
        //{
        //    var builder = new ParserBuilder();
        //    static string sep(string s){
        //        if (s == "苹果") return "apple";return s;
        //    }
        //    builder.AutoReplace.AddReplacing(new List<string> { "苹果" }, sep, true);
        //    var options = builder.GetCurrentOptions();
        //    var ctx = new ParserContext(options);
        //    var parser = new Parser(ctx);

        //    string output1 = parser.RunToPlain(input1);
        //    Assert.AreEqual(answer1, output1);
            
        //    string output2 = parser.RunToPlain(input2);
        //    Assert.AreEqual(answer2, output2);
            
        //    Assert.AreEqual(cacheReadCount, ctx.Caches.CacheReadCount);
        //}
    }
}
