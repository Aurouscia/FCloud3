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
        //[TestMethod]
        //[DataRow(
        //    "123456789", 0,
        //    "123456789", 1,
        //    "123456789", 1)]
        //[DataRow(
        //    "12345\nab*c*de\n67890",0,
        //    "12345\nab*c*de\n67891",3,
        //    "12345\nab*c*de\n67890",4
        //    )]
        //public void Exclusive(
        //    string input1, int count1,
        //    string input2, int count2,
        //    string input3, int count3)
        //{
        //    var builder = new ParserBuilder();
        //    var options = builder.Cache.SwitchToExclusiveCache().GetCurrentOptions();
        //    var ctx = new ParserContext(options);
        //    var parser = new Parser(ctx);
        //    parser.RunToPlain(input1);
        //    Assert.AreEqual(count1, ctx.Caches.CacheReadCount);
        //    parser.RunToPlain(input2);
        //    Assert.AreEqual(count2, ctx.Caches.CacheReadCount);
        //    parser.RunToPlain(input3);
        //    Assert.AreEqual(count3, ctx.Caches.CacheReadCount);
        //}

        [DataRow(
            ">怎么的？想打架？\n他这么**恶狠狠**地说到\n---\n没过多久，他就被抬上了救护车",0,
            ">怎么的？想打架？\n他这么**恶狠狠**地说到\n---\n没过多久，他就被抬上了救护车",1,
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
    }
}
