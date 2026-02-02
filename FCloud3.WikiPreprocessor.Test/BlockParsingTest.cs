using FCloud3.WikiPreprocessor.Mechanics;
using FCloud3.WikiPreprocessor.Options;
using FCloud3.WikiPreprocessor.Rules;
using FCloud3.WikiPreprocessor.Test.Support;

namespace FCloud3.WikiPreprocessor.Test
{
    internal class CuteBunnyReplaceConvertingProvider : ConvertingProviderBase
    {
        public override string? Replace(string replaceTarget)
        {
            if (replaceTarget == "可爱兔兔")
                return "cute-bunny";
            if (replaceTarget == "114514")
                return "恶臭";
            return replaceTarget;
        }

        public static string[] Targets => ["可爱兔兔", "114514"];
    }

    [TestClass]
    public class BlockParsingTest
    {
        private readonly Parser _parser;
        private readonly CuteBunnyReplaceConvertingProvider _convertingProvider;

        public BlockParsingTest()
        {
            var parserBuilder = new ParserBuilder()
                .Block.AddMoreRule(
                    new PrefixBlockRule("&gt;", "<div q>", "</div>", "引用")
                ).AutoReplace.AddReplacingTargets(
                    CuteBunnyReplaceConvertingProvider.Targets,
                    false
                );
            _parser = parserBuilder.BuildParser();
            _convertingProvider = new CuteBunnyReplaceConvertingProvider();
        }

        public static IEnumerable<object[]> TitledBlockTestData => new object[][]
        {
            new object[] {
                "#哈喽大家好\r\n我是sb，大sb\n>哼哼唧唧\n> 哼哼唧唧\n \t \n",
                "<h1>哈喽大家好</h1><div class=\"indent\"><p>我是sb，大sb</p><div q><p>哼哼唧唧</p><p>哼哼唧唧</p></div></div>"
            },
            new object[] {
                "#哈喽大家好\r\n我是sb，大sb\n>哼哼唧唧\n> 哼哼唧唧\n#大家再见 \t \n",
                "<h1>哈喽大家好</h1><div class=\"indent\"><p>我是sb，大sb</p><div q><p>哼哼唧唧</p><p>哼哼唧唧</p></div></div><h1>大家再见</h1><div class=\"indent\"></div>"
            },
            new object[] {
                "#哈喽大家好\r\n我是sb，大sb\n>哼哼唧唧1\n>>哼哼唧唧2\n>哼哼唧唧3",
                "<h1>哈喽大家好</h1><div class=\"indent\"><p>我是sb，大sb</p><div q><p>哼哼唧唧1</p><div q><p>哼哼唧唧2</p></div><p>哼哼唧唧3</p></div></div>"
            },
            new object[] {
                "内容1\n#一级标题\t\n 内容2\r\n ##二级标题1\n内容3\n##二级标题2",
                "<p>内容1</p><h1>一级标题</h1><div class=\"indent\"><p>内容2</p><h2>二级标题1</h2><div class=\"indent\"><p>内容3</p></div><h2>二级标题2</h2><div class=\"indent\"></div></div>"
            },
            new object[] {
                "\n#一级标题\t\n 内容2\r\n ##二级标题1\n内容3\n##二级标题2",
                "<h1>一级标题</h1><div class=\"indent\"><p>内容2</p><h2>二级标题1</h2><div class=\"indent\"><p>内容3</p></div><h2>二级标题2</h2><div class=\"indent\"></div></div>"
            },
            new object[] {
                "#一级标题\t\n 内容2\r\n ##二级标题1\n\n##二级标题2",
                "<h1>一级标题</h1><div class=\"indent\"><p>内容2</p><h2>二级标题1</h2><div class=\"indent\"></div><h2>二级标题2</h2><div class=\"indent\"></div></div>"
            },
            new object[] {
                "#一级标题\t\n 内容2\r\n ##二级标题1\n\n##二级标题#2",
                "<h1>一级标题</h1><div class=\"indent\"><p>内容2</p><h2>二级标题1</h2><div class=\"indent\"></div><h2>二级标题#2</h2><div class=\"indent\"></div></div>"
            },
            new object[] {
                "#一级标题\t\n 内容2\r\n ##二级标题1\n\n#red#",
                "<h1>一级标题</h1><div class=\"indent\"><p>内容2</p><h2>二级标题1</h2><div class=\"indent\"><p><span class=\"coloredBlock\" style=\"background-color:rgb(255,0,0)\"></span></p></div></div>"
            },
            new object[] {
                "#一级标题\t\n 内容2\r\n ##二级标题1\n\n##二级标题2\n哼唧哼",
                "<h1>一级标题</h1><div class=\"indent\"><p>内容2</p><h2>二级标题1</h2><div class=\"indent\"></div><h2>二级标题2</h2><div class=\"indent\"><p>哼唧哼</p></div></div>"
            },
            new object[] {
                "#一级标题\t\n 内容2\r\n ##二级标题1\n\n##\n哼唧哼",
                "<h1>一级标题</h1><div class=\"indent\"><p>内容2</p><h2>二级标题1</h2><div class=\"indent\"></div><p>哼唧哼</p></div>"
            },
            new object[] {
                "#一级标题\t\n 内容2\r\n ##二级标题1\n\n#\n哼唧哼",
                "<h1>一级标题</h1><div class=\"indent\"><p>内容2</p><h2>二级标题1</h2><div class=\"indent\"></div></div><p>哼唧哼</p>"
            },
            new object[] {
                "内容1\n#一级*标*题\t\n 内容2\r\n ##二级**标**题1\n内容3\n##二级标题2",
                "<p>内容1</p><h1>一级<i>标</i>题</h1><div class=\"indent\"><p>内容2</p><h2>二级<b>标</b>题1</h2><div class=\"indent\"><p>内容3</p></div><h2>二级标题2</h2><div class=\"indent\"></div></div>"
            },
            new object[] {
                "# #111111\\@标题1#",
                "<h1><span class=\"coloredText\" style=\"color:rgb(17,17,17)\">标题1</span></h1><div class=\"indent\"></div>"
            },
            new object[] {
                "# 标题111111\n内容1\n# 带有颜色块的标题测试#111111#",
                "<h1>标题111111</h1><div class=\"indent\"><p>内容1</p></div><h1>带有颜色块的标题测试<span class=\"coloredBlock\" style=\"background-color:rgb(17,17,17)\"></span></h1><div class=\"indent\"></div>"
            }
        };

        [TestMethod]
        [DynamicData(nameof(TitledBlockTestData))]
        public void TitledBlockTest(string content, string answer)
        {
            string html = _parser.RunToPlain(content);
            Assert.AreEqual(answer, html);
            string html2 = _parser.RunToPlain(content);
            Assert.AreEqual(answer, html2);
            string html3 = _parser.RunToPlain(content);
            Assert.AreEqual(answer, html3);
        }
        
        /// <summary>
        /// 当一个标题的势力范围仅有一个列表(ul)元素，则特殊处理，不输出为hx
        /// </summary>
        public static IEnumerable<object[]> TitledListBlockTestData => new object[][]
        {
            new object[] {
                "# 恭喜\n ## 中奖名单\n- 小张\n -小王",
                "<h1>恭喜</h1><div class=\"indent\">" +
                    "<div class=\"titledList\">" +
                        "<div class=\"titledListTitle\">中奖名单</div>" +
                        "<ul><li>小张</li><li>小王</li></ul>" +
                    "</div>" +
                "</div>"
            }
        };

        [TestMethod]
        [DynamicData(nameof(TitledListBlockTestData))]
        public void TitledListBlockTest(string content, string answer)
        {
            string html = _parser.RunToPlain(content);
            Assert.AreEqual(answer, html);
            string html2 = _parser.RunToPlain(content);
            Assert.AreEqual(answer, html2);
            string html3 = _parser.RunToPlain(content);
            Assert.AreEqual(answer, html3);
        }

        public static IEnumerable<object[]> MiniTableTestData => new object[][]
        {
            new object[] {
                "|名称|年龄|\r\n|Au|20|\t\n|旋头|38|",
                "<table><tr><td>名称</td><td>年龄</td></tr><tr><td>Au</td><td>20</td></tr><tr><td>旋头</td><td>38</td></tr></table>"
            },
            new object[] {
                "|名称|年龄|\r\n|Au|20|\t\n|旋头|",
                "<table><tr><td>名称</td><td>年龄</td></tr><tr><td>Au</td><td>20</td></tr><tr><td>旋头</td><td></td></tr></table>"
            },
            new object[] {
                "|名称|年龄|\n|---|---|\n|Au|20|\n|旋头|38|",
                "<table><tr><th>名称</th><th>年龄</th></tr><tr><td>Au</td><td>20</td></tr><tr><td>旋头</td><td>38</td></tr></table>"
            },
            new object[] {
                "|一号线/-c-/blue | 89km |\n|二号线/-c-/rgb(255,190,190) | 130km |",
                "<table><tr><td style=\"background-color:rgb(0,0,255);color:white\">一号线</td><td>89km</td></tr>" +
                "<tr><td style=\"background-color:rgb(255,190,190);color:black\">二号线</td><td>130km</td></tr></table>"
            },
            new object[] {
                "|一号线/-c-/哼唧咪咕 | 89km |\n|二号线/-c-/rgb(255,190,190) | 130km |",
                "<table><tr><td>一号线/-c-/哼唧咪咕</td><td>89km</td></tr>" +
                "<tr><td style=\"background-color:rgb(255,190,190);color:black\">二号线</td><td>130km</td></tr></table>"
            },
            new object[] {
                "|114514/-c-/#114514|",
                "<table><tr><td style=\"background-color:rgb(17,69,20);color:white\">恶臭</td></tr></table>"
            }
        };

        [TestMethod]
        [DynamicData(nameof(MiniTableTestData))]
        public void MiniTableTest(string content, string answer)
        {
            _parser.SetConvertingProvider(_convertingProvider);
            string html = _parser.RunToPlain(content);
            Assert.AreEqual(answer, html);
        }

        public static IEnumerable<object[]> ListTestData => new object[][]
        {
            new object[] {
                "以下是获奖人员名单：\n-Au先生\n- hcm先生\n-兔兔子小姐\r\n让我们用热烈掌声祝贺他们！",
                "<p>以下是获奖人员名单：</p><ul><li>Au先生</li><li>hcm先生</li><li>兔兔子小姐</li></ul><p>让我们用热烈掌声祝贺他们！</p>"
            }
        };

        [TestMethod]
        [DynamicData(nameof(ListTestData))]
        public void ListTest(string content,string answer)
        {
            string html = _parser.RunToPlain(content);
            Assert.AreEqual(answer, html);
        }

        public static IEnumerable<object[]> SepTestData => new object[][]
        {
            new object[] {
                "AAA\nBBB\n---\nCCC\nDDD",
                "<p>AAA</p><p>BBB</p><div class=\"sep\"></div><p>CCC</p><p>DDD</p>"
            },
            new object[] {
                "AAA\nBBB\n------\n----\nCCC\nDDD",
                "<p>AAA</p><p>BBB</p><div class=\"sep\"></div><div class=\"sep\"></div><p>CCC</p><p>DDD</p>"
            }
        };

        [TestMethod]
        [DynamicData(nameof(SepTestData))]
        public void SepTest(string content, string answer)
        {
            string html = _parser.RunToPlain(content);
            Assert.AreEqual(answer, html);
        }

        public static IEnumerable<object[]> CommentTestData => new object[][]
        {
            new object[] {
                "AAA\n//BBB\nCCC",
                "<p>AAA</p><p>CCC</p>"
            },
            new object[] {
                "AAA\n \t //BBB\nCCC",
                "<p>AAA</p><p>CCC</p>"
            }
        };

        [TestMethod]
        [DynamicData(nameof(CommentTestData))]
        public void CommentTest(string content, string answer)
        {
            string html = _parser.RunToPlain(content);
            Assert.AreEqual(answer, html);
        }

        public static IEnumerable<object[]> FootNoteData => new object[][]
        {
            new object[] {
                "abc[^1]de \n [^1]:c是第三个*英文*字母",
                "<p>abc<sup><a id=\"refentry_1\" class=\"refentry\">[1]</a></sup>de</p>",
                "<div class=\"refbodies\"><div><a id=\"ref_1\" class=\"ref\">[1]</a>c是第三个<i>英文</i>字母</div></div>"
            },
            new object[] {
                "abc[^1]d[^2b]e \n [^1]:c是第三个*英文*字母 \n 666 \n [^2b]:d是第四个英文字母",
                "<p>abc<sup><a id=\"refentry_1\" class=\"refentry\">[1]</a></sup>" +
                "d<sup><a id=\"refentry_2b\" class=\"refentry\">[2b]</a></sup>e</p>" +
                "<p>666</p>",
                "<div class=\"refbodies\">" +
                "<div><a id=\"ref_1\" class=\"ref\">[1]</a>c是第三个<i>英文</i>字母</div>" +
                "<div><a id=\"ref_2b\" class=\"ref\">[2b]</a>d是第四个英文字母</div>" +
                "</div>"
            }
        };

        [TestMethod]
        [DynamicData(nameof(FootNoteData))]
        public void FootNote(string content, string answerMain, string answerFootNotes)
        {
            var res = _parser.RunToParserResult(content);
            Assert.AreEqual(answerMain, res.Content);
            Assert.AreEqual(answerFootNotes, res.FootNotes);

            var res2 = _parser.RunToParserResult(content);
            Assert.AreEqual(answerMain, res2.Content);
            Assert.AreEqual(answerFootNotes, res2.FootNotes);
        }
    }
}
