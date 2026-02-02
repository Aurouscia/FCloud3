using FCloud3.WikiPreprocessor.Mechanics;
using FCloud3.WikiPreprocessor.Options;
using FCloud3.WikiPreprocessor.Test.Support;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FCloud3.WikiPreprocessor.ConvertingProvider.Models;

namespace FCloud3.WikiPreprocessor.Test
{
    [TestClass]
    public class WriteBodyTest
    {
        private readonly Parser _parser;
        public WriteBodyTest()
        {
            var pb = new ParserBuilder();
            pb.AutoReplace.AddReplacingTargets(["abc", "efg"], false);
            _parser = pb.BuildParser();
            _parser.SetConvertingProvider(new FakeConvertingProvider());
        }
        internal class FakeConvertingProvider : ConvertingProviderBase
        {
            public override string? Replace(string replaceTarget)
            {
                return $"<a>{replaceTarget}</a>";
            }
            public override LinkItem Link(string link)
            {
                return new(link, link);
            }
        }

        public static IEnumerable<object[]> CommonTestData()
        {
            yield return new object[] {
                "你好，很高兴认识你",
                "你好，很高兴认识你", 100
            };
            yield return new object[] {
                "你好，很*高兴*认识你",
                "你好，很高兴认识你", 100
            };
            yield return new object[] {
                "你好\n很高兴认识你",
                "你好 很高兴认识你", 100
            };
            yield return new object[] {
                "你好\n很高兴认识你",
                "你", 1
            };
            yield return new object[] {
                "你好\n很高兴认识你",
                "你好", 2
            };
            yield return new object[] {
                "你好\n很高兴认识你",
                "你好 ", 3
            };
            yield return new object[] {
                "你好\n很高兴认识你",
                "你好 很", 4
            };
            yield return new object[] {
                "> 你好\n > 很高兴认识你\n...   ——朋友",
                "你好 很高兴认识你 ——朋友", 100
            };
            yield return new object[] {
                "# 来自朋友的问候\n> 你好\n > 很高兴认识你\n...   ——朋友",
                "你好 很高兴认识你 ——朋友", 100
            };
            yield return new object[] {
                "# 来自*朋友*的问候\n> 你好\n > 很*高兴*认识你\n...   ——朋友",
                "你好 很高兴认识你 ——朋友", 100
            };
            yield return new object[] {
                "# 来自朋友的问候\n> 你好\n > 很高兴认识你\n...   ——朋友",
                "你好 很高兴认识", 8
            };
            yield return new object[] {
                "# 来自朋友的问候\n> 你好\n > 很高兴认识你\n...   ——朋友",
                "你好 很高兴认识你", 9
            };
            yield return new object[] {
                "# 来自朋友的问候\n> 你好\n > 很高兴认识你\n...   ——朋友",
                "你好 很高兴认识你 ", 10
            };
            yield return new object[] {
                "# 来自朋友的问候\n> 你好\n > 很高兴认识你\n...   ——朋友",
                "你好 很高兴认识你 —", 11
            };
        }

        [TestMethod]
        [DynamicData(nameof(CommonTestData))]
        public void Common(string input, string expectBody, int maxLength)
        {
            var sb = new StringBuilder();
            var obj = _parser.RunToObject(input);
            obj.WriteBody(sb, maxLength);
            Assert.AreEqual(expectBody, sb.ToString());
        }

        public static IEnumerable<object[]> AutoReplaceTestData()
        {
            yield return new object[] {"123abc456", "123abc456"};
            yield return new object[] {"123abcefg456", "123abcefg456"};
        }

        [TestMethod]
        [DynamicData(nameof(AutoReplaceTestData))]
        public void AutoReplace(string input, string expectBody)
        {
            var sb = new StringBuilder();
            var obj = _parser.RunToObject(input);
            obj.WriteBody(sb, int.MaxValue);
            Assert.AreEqual(expectBody, sb.ToString());
        }

        public static IEnumerable<object[]> ManualLinkTestData()
        {
            yield return new object[] {"123[xxx]456", "123xxx456"};
            yield return new object[] {"123[xxx](yyy)456", "123xxx456"};
        }

        [TestMethod]
        [DynamicData(nameof(ManualLinkTestData))]
        public void ManualLink(string input, string expectBody)
        {
            var sb = new StringBuilder();
            var obj = _parser.RunToObject(input);
            obj.WriteBody(sb, int.MaxValue);
            Assert.AreEqual(expectBody, sb.ToString());
        }
    }
}
