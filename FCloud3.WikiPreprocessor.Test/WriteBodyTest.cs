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
    public class WriteBodyTest
    {
        private readonly Parser _parser;
        public WriteBodyTest()
        {
            var pb = new ParserBuilder();
            _parser = pb.BuildParser();
        }

        [TestMethod]
        [DataRow(
            "你好\n很高兴认识你",
            "你好 很高兴认识你", 100)]
        [DataRow(
            "你好\n很高兴认识你",
            "你", 1)]
        [DataRow(
            "你好\n很高兴认识你",
            "你好", 2)]
        [DataRow(
            "你好\n很高兴认识你",
            "你好 ", 3)]
        [DataRow(
            "你好\n很高兴认识你",
            "你好 很", 4)]
        [DataRow(
            "> 你好\n > 很高兴认识你\n...   ——朋友",
            "你好 很高兴认识你 ——朋友", 100)]
        [DataRow(
            "# 来自朋友的问候\n> 你好\n > 很高兴认识你\n...   ——朋友",
            "你好 很高兴认识你 ——朋友", 100)]
        [DataRow(
            "# 来自朋友的问候\n> 你好\n > 很高兴认识你\n...   ——朋友",
            "你好 很高兴认识", 8)]
        [DataRow(
            "# 来自朋友的问候\n> 你好\n > 很高兴认识你\n...   ——朋友",
            "你好 很高兴认识你", 9)]
        [DataRow(
            "# 来自朋友的问候\n> 你好\n > 很高兴认识你\n...   ——朋友",
            "你好 很高兴认识你 ", 10)]
        [DataRow(
            "# 来自朋友的问候\n> 你好\n > 很高兴认识你\n...   ——朋友",
            "你好 很高兴认识你 —", 11)]
        public void Common(string input, string expectBody, int maxLength)
        {
            var sb = new StringBuilder();
            var obj = _parser.RunToObject(input);
            obj.WriteBody(sb, maxLength);
            Assert.AreEqual(expectBody, sb.ToString());
        }
    }
}
