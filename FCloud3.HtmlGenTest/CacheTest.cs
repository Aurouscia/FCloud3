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
        [TestMethod]
        [DataRow(
            "123456789", 0,
            "123456789", 1,
            "123456789", 2)]
        [DataRow(
            "12345\nab*c*de\n67890",0,
            "12345\nab*c*de\n67891",1,
            "12345\nab*c*de\n67890",3
            )]
        public void Identical(
            string input1, int count1,
            string input2, int count2,
            string input3, int count3)
        {
            var builder = new ParserBuilder();
            var options = builder.GetCurrentOptions();
            var ctx = new ParserContext(options);
            var parser = new Parser(ctx);
            parser.RunToPlain(input1);
            Assert.AreEqual(count1, ctx.CacheReadCount);
            parser.RunToPlain(input2);
            Assert.AreEqual(count2, ctx.CacheReadCount);
            parser.RunToPlain(input3);
            Assert.AreEqual(count3, ctx.CacheReadCount);
        }
    }
}
