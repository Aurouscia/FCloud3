using FCloud3.HtmlGen.Mechanics;
using FCloud3.HtmlGen.Options;
using FCloud3.HtmlGen.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.HtmlGenTest
{
    [TestClass]
    public class CommonPartTest
    {
        private readonly HtmlGenOptions _options;
        public CommonPartTest()
        {
            HtmlGenOptionsProvider optionsProvider = new(
                templates: new()
                {
                    new HtmlTemplate("打招呼","<div class=\"hello\">[[内容]]</div>",".hello{font-size:large}"),
                    new HtmlTemplate("哼唧","<div class=\"hj\">[[内容]]</div>",".hj{color:gray}","console.log('哼唧')")
                },
                customInlineRules: new()
                {
                    new HtmlCustomInlineRule(
                            "\\red","\\red","<span class=\"red\">","</span>","红色字体",".red{color:red}"
                        )
                },
                customBlockRules: new()
                {
                    new HtmlPrefixBlockRule(
                        ">","<div class=\"quote\">","</div>","引用",".quote{font-size:small}"
                    )
                },
                implantsHandler:x => null
            );
            _options = optionsProvider.GetOptions();
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
            "<style>.red{color:red}.hello{font-size:large}</style><p><div class=\"hello\">兄弟<span class=\"red\">你好</span></div>很高兴认识你！</p>")]
        [DataRow(
            "他说：\r\n > {{打招呼}内容::兄弟 \\red 你好 \\red} \r\n >很高兴认识你！",
            "<style>.quote{font-size:small}.red{color:red}.hello{font-size:large}</style>" +
            "<p>他说：</p><div class=\"quote\"><p><div class=\"hello\">兄弟<span class=\"red\">你好</span></div></p><p>很高兴认识你！</p></div>")]
        [DataRow(
            "{{哼唧}}",
            "<style>.hj{color:gray}</style><script>console.log('哼唧')</script><p><div class=\"hj\"></div></p>")]
        [DataRow(
            "{{哼唧}噜噜}",
            "<style>.hj{color:gray}</style><script>console.log('哼唧')</script><p><div class=\"hj\">噜噜</div></p>")]
        [DataRow(
            "{{哼唧}噜\\red噜\\red}",
            "<style>.red{color:red}.hj{color:gray}</style><script>console.log('哼唧')</script><p><div class=\"hj\">噜<span class=\"red\">噜</span></div></p>")]
        public void TestOne(string input,string answer)
        {
            var parser = new Parser(_options);
            var res = parser.Run(input,true);
            Assert.AreEqual(answer, res);
        }
    }
}
