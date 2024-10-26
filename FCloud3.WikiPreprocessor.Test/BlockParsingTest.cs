using FCloud3.WikiPreprocessor.Mechanics;
using FCloud3.WikiPreprocessor.Options;
using FCloud3.WikiPreprocessor.Rules;
using FCloud3.WikiPreprocessor.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FCloud3.WikiPreprocessor.Test.Support;

namespace FCloud3.WikiPreprocessor.Test
{
    [TestClass]
    public class BlockParsingTest
    {
        private readonly Parser _parser;

        public BlockParsingTest()
        {
            var parserBuilder = new ParserBuilder()
                .Block.AddMoreRule(
                    new PrefixBlockRule("&gt;", "<div q>", "</div>", "引用")
                )
                .Cache.UseCacheInstance(CacheInstance.Get());;
            _parser = parserBuilder.BuildParser();
        }

        [TestMethod]
        [DataRow(
            "#哈喽大家好\r\n我是sb，大sb\n>哼哼唧唧\n> 哼哼唧唧\n \t \n",
            "<h1>哈喽大家好</h1><div class=\"indent\"><p>我是sb，大sb</p><div q><p>哼哼唧唧</p><p>哼哼唧唧</p></div></div>")]
        [DataRow(
            "#哈喽大家好\r\n我是sb，大sb\n>哼哼唧唧\n> 哼哼唧唧\n#大家再见 \t \n",
            "<h1>哈喽大家好</h1><div class=\"indent\"><p>我是sb，大sb</p><div q><p>哼哼唧唧</p><p>哼哼唧唧</p></div></div><h1>大家再见</h1><div class=\"indent\"></div>")]
        [DataRow(
            "#哈喽大家好\r\n我是sb，大sb\n>哼哼唧唧1\n>>哼哼唧唧2\n>哼哼唧唧3",
            "<h1>哈喽大家好</h1><div class=\"indent\"><p>我是sb，大sb</p><div q><p>哼哼唧唧1</p><div q><p>哼哼唧唧2</p></div><p>哼哼唧唧3</p></div></div>")]
        [DataRow(
            "内容1\n#一级标题\t\n 内容2\r\n ##二级标题1\n内容3\n##二级标题2",
            "<p>内容1</p><h1>一级标题</h1><div class=\"indent\"><p>内容2</p><h2>二级标题1</h2><div class=\"indent\"><p>内容3</p></div><h2>二级标题2</h2><div class=\"indent\"></div></div>")]
        [DataRow(
            "\n#一级标题\t\n 内容2\r\n ##二级标题1\n内容3\n##二级标题2",
            "<h1>一级标题</h1><div class=\"indent\"><p>内容2</p><h2>二级标题1</h2><div class=\"indent\"><p>内容3</p></div><h2>二级标题2</h2><div class=\"indent\"></div></div>")]
        [DataRow(
            "#一级标题\t\n 内容2\r\n ##二级标题1\n\n##二级标题2",
            "<h1>一级标题</h1><div class=\"indent\"><p>内容2</p><h2>二级标题1</h2><div class=\"indent\"></div><h2>二级标题2</h2><div class=\"indent\"></div></div>")]
        [DataRow(
            "#一级标题\t\n 内容2\r\n ##二级标题1\n\n##二级标题#2",//看起来像是标题，但其实不是标题的东西
            "<h1>一级标题</h1><div class=\"indent\"><p>内容2</p><h2>二级标题1</h2><div class=\"indent\"><p>##二级标题#2</p></div></div>")]
        [DataRow(
            "#一级标题\t\n 内容2\r\n ##二级标题1\n\n#red#",//看起来像是标题，但其实不是标题的东西
            "<h1>一级标题</h1><div class=\"indent\"><p>内容2</p><h2>二级标题1</h2><div class=\"indent\"><p><span class=\"coloredBlock\" style=\"background-color:rgb(255,0,0)\"></span></p></div></div>")]
        [DataRow(
            "#一级标题\t\n 内容2\r\n ##二级标题1\n\n##二级标题2\n哼唧哼",
            "<h1>一级标题</h1><div class=\"indent\"><p>内容2</p><h2>二级标题1</h2><div class=\"indent\"></div><h2>二级标题2</h2><div class=\"indent\"><p>哼唧哼</p></div></div>")]
        [DataRow(
            "#一级标题\t\n 内容2\r\n ##二级标题1\n\n##\n哼唧哼",//用仅有##的行来脱离上一个同级标题
            "<h1>一级标题</h1><div class=\"indent\"><p>内容2</p><h2>二级标题1</h2><div class=\"indent\"></div><p>哼唧哼</p></div>")]
        [DataRow(
            "#一级标题\t\n 内容2\r\n ##二级标题1\n\n#\n哼唧哼",//用仅有#的行来脱离上一个同级标题
            "<h1>一级标题</h1><div class=\"indent\"><p>内容2</p><h2>二级标题1</h2><div class=\"indent\"></div></div><p>哼唧哼</p>")]
        [DataRow(
            "内容1\n#一级*标*题\t\n 内容2\r\n ##二级**标**题1\n内容3\n##二级标题2",
            "<p>内容1</p><h1>一级<i>标</i>题</h1><div class=\"indent\"><p>内容2</p><h2>二级<b>标</b>题1</h2><div class=\"indent\"><p>内容3</p></div><h2>二级标题2</h2><div class=\"indent\"></div></div>")]
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
        [TestMethod]
        [DataRow(
            "# 恭喜\n ## 中奖名单\n- 小张\n -小王",
            "<h1>恭喜</h1><div class=\"indent\">" +
                "<div class=\"titledList\">" +
                    "<div class=\"titledListTitle\">中奖名单</div>" +
                    "<ul><li>小张</li><li>小王</li></ul>" +
                "</div>" +
            "</div>")]
        public void TitledListBlockTest(string content, string answer)
        {
            string html = _parser.RunToPlain(content);
            Assert.AreEqual(answer, html);
            string html2 = _parser.RunToPlain(content);
            Assert.AreEqual(answer, html2);
            string html3 = _parser.RunToPlain(content);
            Assert.AreEqual(answer, html3);
        }

        [TestMethod]
        [DataRow(
            "|名称|年龄|\r\n|Au|20|\t\n|旋头|38|",
            "<table><tr><td>名称</td><td>年龄</td></tr><tr><td>Au</td><td>20</td></tr><tr><td>旋头</td><td>38</td></tr></table>")]
        [DataRow(
            "|名称|年龄|\r\n|Au|20|\t\n|旋头|",
            "<table><tr><td>名称</td><td>年龄</td></tr><tr><td>Au</td><td>20</td></tr><tr><td>旋头</td><td></td></tr></table>")]
        [DataRow(
            "|名称|年龄|\n|---|---|\n|Au|20|\n|旋头|38|",
            "<table><tr><th>名称</th><th>年龄</th></tr><tr><td>Au</td><td>20</td></tr><tr><td>旋头</td><td>38</td></tr></table>")]
        [DataRow(
            "|一号线/-c-/blue | 89km |\n|二号线/-c-/rgb(255,190,190) | 130km |",
            "<table><tr><td style=\"background-color:rgb(0,0,255);color:white\">一号线</td><td>89km</td></tr>" +
            "<tr><td style=\"background-color:rgb(255,190,190);color:black\">二号线</td><td>130km</td></tr></table>")]
        [DataRow(
            "|一号线/-c-/哼唧咪咕 | 89km |\n|二号线/-c-/rgb(255,190,190) | 130km |",
            "<table><tr><td>一号线/-c-/哼唧咪咕</td><td>89km</td></tr>" +
            "<tr><td style=\"background-color:rgb(255,190,190);color:black\">二号线</td><td>130km</td></tr></table>")]
        public void MiniTableTest(string content, string answer)
        {
            string html = _parser.RunToPlain(content);
            Assert.AreEqual(answer, html);
        }

        [TestMethod]
        [DataRow(
            "以下是获奖人员名单：\n-Au先生\n- hcm先生\n-兔兔子小姐\r\n让我们用热烈掌声祝贺他们！",
            "<p>以下是获奖人员名单：</p><ul><li>Au先生</li><li>hcm先生</li><li>兔兔子小姐</li></ul><p>让我们用热烈掌声祝贺他们！</p>")]
        public void ListTest(string content,string answer)
        {
            string html = _parser.RunToPlain(content);
            Assert.AreEqual(answer, html);
        }
        [TestMethod]
        [DataRow(
            "AAA\nBBB\n---\nCCC\nDDD",
            "<p>AAA</p><p>BBB</p><div class=\"sep\"></div><p>CCC</p><p>DDD</p>")]
        [DataRow(
            "AAA\nBBB\n------\n----\nCCC\nDDD",
            "<p>AAA</p><p>BBB</p><div class=\"sep\"></div><div class=\"sep\"></div><p>CCC</p><p>DDD</p>")]
        public void SepTest(string content, string answer)
        {
            string html = _parser.RunToPlain(content);
            Assert.AreEqual(answer, html);
        }

        [TestMethod]
        [DataRow(
            "abc[^1]de \n [^1]:c是第三个*英文*字母",
            "<p>abc<sup><a id=\"refentry_1\" class=\"refentry\">[1]</a></sup>de</p>",
            "<div class=\"refbodies\"><div><a id=\"ref_1\" class=\"ref\">[1]</a>c是第三个<i>英文</i>字母</div></div>"
            )]
        [DataRow(
            "abc[^1]d[^2b]e \n [^1]:c是第三个*英文*字母 \n 666 \n [^2b]:d是第四个英文字母",

            "<p>abc<sup><a id=\"refentry_1\" class=\"refentry\">[1]</a></sup>" +
            "d<sup><a id=\"refentry_2b\" class=\"refentry\">[2b]</a></sup>e</p>" +
            "<p>666</p>",

            "<div class=\"refbodies\">" +
            "<div><a id=\"ref_1\" class=\"ref\">[1]</a>c是第三个<i>英文</i>字母</div>" +
            "<div><a id=\"ref_2b\" class=\"ref\">[2b]</a>d是第四个英文字母</div>" +
            "</div>"
            )]
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
