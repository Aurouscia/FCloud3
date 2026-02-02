using FCloud3.WikiPreprocessor.Context;
using FCloud3.WikiPreprocessor.Mechanics;
using FCloud3.WikiPreprocessor.Options;
using FCloud3.WikiPreprocessor.Test.Support;

namespace FCloud3.WikiPreprocessor.Test
{
    [TestClass]
    public class CacheTest
    {
        public static IEnumerable<object[]> ReadCountAndEntryCountData => new object[][]
        {
            new object[] {
                ">怎么的？想打架？\n他这么**恶狠狠**地说到\n---\n没过多久，他就被抬上了救护车", 0, 3,
                ">怎么的？想打架？\n他这么**恶狠狠**地说到\n---\n没过多久，他就被抬上了救护车", 3, 3,
                ">怎么的？想打架？\n他这么**狠狠**地说到\n----\n没过多久，他就被抬上了救护车", 2, 3,
                ">怎么的？想打架？\n他这么**恶狠狠**地说到\n----\n没过多久，他就被送往附近医院", 1, 3
            },
            new object[] {
                ">怎么的？想打架？\n他这么**恶狠狠**地说到", 0, 2,
                ">怎么的？想打架？\n他这么**恶狠狠**地说到\n---\n没过多久，他就被抬上了救护车", 2, 3,
                ">怎么的？想打架？\n他这么**狠狠**地说到\n----\n没过多久，他就被抬上了救护车", 2, 3,
                ">怎么的？想打架？\n他这么**恶狠狠**地说到", 1, 2
            }
        };

        [TestMethod]
        [DynamicData(nameof(ReadCountAndEntryCountData))]
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
    }
}
