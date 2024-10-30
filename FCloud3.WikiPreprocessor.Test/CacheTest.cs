using FCloud3.WikiPreprocessor.Context;
using FCloud3.WikiPreprocessor.Mechanics;
using FCloud3.WikiPreprocessor.Options;
using FCloud3.WikiPreprocessor.Test.Support;

namespace FCloud3.WikiPreprocessor.Test
{
    [TestClass]
    public class CacheTest
    {
        [DataRow(
            ">怎么的？想打架？\n他这么**恶狠狠**地说到\n---\n没过多久，他就被抬上了救护车", 0, 3,
            ">怎么的？想打架？\n他这么**恶狠狠**地说到\n---\n没过多久，他就被抬上了救护车", 3, 3,
            ">怎么的？想打架？\n他这么**狠狠**地说到\n----\n没过多久，他就被抬上了救护车", 2, 3,
            ">怎么的？想打架？\n他这么**恶狠狠**地说到\n----\n没过多久，他就被送往附近医院", 1, 3)]
        [DataRow(
            ">怎么的？想打架？\n他这么**恶狠狠**地说到", 0, 2,
            ">怎么的？想打架？\n他这么**恶狠狠**地说到\n---\n没过多久，他就被抬上了救护车", 2, 3,
            ">怎么的？想打架？\n他这么**狠狠**地说到\n----\n没过多久，他就被抬上了救护车", 2, 3,
            ">怎么的？想打架？\n他这么**恶狠狠**地说到", 1, 2)]
        [TestMethod]
        public void ReadCountAndEntryCount(
            string input1, int readCount1, int entryCount1,
            string input2, int readCount2, int entryCount2,
            string input3, int readCount3, int entryCount3,
            string input4, int readCount4, int entryCount4)
        {
            var builder = new ParserBuilder()
                .Cache.EnableCache();
            var options = builder.GetCurrentOptions();
            var ctx = new ParserContext(options);
            var parser = new Parser(ctx);
            parser.RunToPlain(input1);
            Assert.AreEqual(readCount1, ctx.Caches.CacheReadCount);
            Assert.AreEqual(entryCount1, ctx.Caches.CacheEntryCount);
            parser.RunToPlain(input2);
            Assert.AreEqual(readCount2, ctx.Caches.CacheReadCount);
            Assert.AreEqual(entryCount2, ctx.Caches.CacheEntryCount);
            parser.RunToPlain(input3);
            Assert.AreEqual(readCount3, ctx.Caches.CacheReadCount);
            Assert.AreEqual(entryCount3, ctx.Caches.CacheEntryCount);
            parser.RunToPlain(input4);
            Assert.AreEqual(readCount4, ctx.Caches.CacheReadCount);
            Assert.AreEqual(entryCount4, ctx.Caches.CacheEntryCount);
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
