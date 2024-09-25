using FCloud3.WikiPreprocessor.Mechanics;
using FCloud3.WikiPreprocessor.Options;
using FCloud3.WikiPreprocessor.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FCloud3.WikiPreprocessor.Test.Support;

namespace FCloud3.WikiPreprocessor.Test
{
    [TestClass]
    public class CommonPartTest
    {
        private readonly Parser _parser;
        public CommonPartTest()
        {
            ParserBuilder parserBuilder = new ParserBuilder()
                .Block.AddMoreRule(new PrefixBlockRule(
                        "&gt;", "<div class=\"quote\">", "</div>", "引用", ".quote{font-size:small}"
                    ))
                .Inline.AddMoreRule(new CustomInlineRule(
                        "\\red", "\\red", "<span class=\"red\">", "</span>", "红色字体", ".red{color:red}"
                    ))
                .Template.AddTemplates(new()
                {
                    new Template("打招呼","<div class=\"hello\">[[__内容__]]</div>",".hello{font-size:large}"),
                    new Template("哼唧","<div class=\"hj\">[[__内容__]]</div>",".hj{color:gray}","console.log('哼唧')")
                })
                .Cache.UseCacheInstance(CacheInstance.Get());;
            _parser = parserBuilder.BuildParser();
        }

        [TestMethod]
        [DataRow(
            "{{打招呼}内容::兄弟你好}很高兴认识你！",
            "<style>.hello{font-size:large}</style><p><div class=\"hello\">兄弟你好</div>很高兴认识你！</p>")]
        [DataRow(
            "{{打招呼}兄弟你好}很高兴认识你！",
            "<style>.hello{font-size:large}</style><p><div class=\"hello\">兄弟你好</div>很高兴认识你！</p>")]
        [DataRow(
            "{{打招呼}内容::兄弟 \\red 你好 \\red}很高兴认识你！",
            "<style>.hello{font-size:large}.red{color:red}</style><p><div class=\"hello\">兄弟 <span class=\"red\"> 你好 </span></div>很高兴认识你！</p>")]
        [DataRow(
            "他说：\r\n > {{打招呼}内容::兄弟 \\red 你好 \\red} \r\n >很高兴认识你！",
            "<style>.hello{font-size:large}.quote{font-size:small}.red{color:red}</style>" +
            "<p>他说：</p><div class=\"quote\"><p><div class=\"hello\">兄弟 <span class=\"red\"> 你好 </span></div></p><p>很高兴认识你！</p></div>")]
        [DataRow(
            "{{哼唧}}",
            "<style>.hj{color:gray}</style><script>console.log('哼唧')\n</script><p><div class=\"hj\"></div></p>")]
        [DataRow(
            "{{哼唧}噜噜}",
            "<style>.hj{color:gray}</style><script>console.log('哼唧')\n</script><p><div class=\"hj\">噜噜</div></p>")]
        [DataRow(
            "{{哼唧}噜\\red噜\\red}",
            "<style>.hj{color:gray}.red{color:red}</style><script>console.log('哼唧')\n</script><p><div class=\"hj\">噜<span class=\"red\">噜</span></div></p>")]
        public void TestOne(string input,string answer)
        {
            var res = _parser.RunToPlain(input,true);
            Assert.AreEqual(answer, res);
            var res2 = _parser.RunToPlain(input, true);
            Assert.AreEqual(answer, res2);
            var res3 = _parser.RunToPlain(input, true);
            Assert.AreEqual(answer, res3);
        }
    }
}
