using FCloud3.HtmlGen.Mechanics;
using FCloud3.HtmlGen.Options;
using FCloud3.HtmlGen.Rules;
using FCloud3.HtmlGen.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.HtmlGenTest
{
    [TestClass]
    public class BlockParsingTest
    {
        private readonly HtmlGenOptions _options;

        public BlockParsingTest()
        {
            HtmlGenOptionsBuilder optionsBuilder = new(
                templateOptions: new(),
                blockOptions: new HtmlGen.Options.SubOptions.BlockParsingOptions(
                    blockRules:new()
                    {
                        new HtmlPrefixBlockRule(
                            ">","<div q>","</div>","引用"
                        )
                    },
                    titleLevelOffset:0
                ),
                inlineOptions:new()
                );
            _options = optionsBuilder.GetOptions();
        }
        [TestMethod]
        [DataRow(
            "\n123\n\r456\n789",3)]
        [DataRow(
            "\n12{{3\n45}}6\n789",2)]
        [DataRow(
            "\n12{{3\n\r456\n78}}9", 1)]
        public void LineSplitTest(string content,int answer)
        {
            var res = LineSplitter.Split(content);
            Assert.AreEqual(res.Count, answer);
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
            "<h1>一级标题</h1><div class=\"indent\"><p>内容2</p><h2>二级标题1</h2><div class=\"indent\"><p><span class=\"coloredBlock\" style=\"color:red;background-color:red\"></span></p></div></div>")]
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
        public void ParseTest(string content, string answer)
        {
            Parser parser = new(_options);
            string html = parser.Run(content);
            Assert.AreEqual(answer, html);
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
        public void MiniTableTest(string content, string answer)
        {
            Parser parser = new(_options);
            string html = parser.Run(content);
            Assert.AreEqual(answer, html);
        }

        [TestMethod]
        [DataRow(
            "以下是获奖人员名单：\n-Au先生\n- hcm先生\n-兔兔子小姐\r\n让我们用热烈掌声祝贺他们！",
            "<p>以下是获奖人员名单：</p><ul><li>Au先生</li><li>hcm先生</li><li>兔兔子小姐</li></ul><p>让我们用热烈掌声祝贺他们！</p>")]
        public void ListTest(string content,string answer)
        {
            Parser parser = new(_options);
            string html = parser.Run(content);
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
            Parser parser = new(_options);
            string html = parser.Run(content);
            Assert.AreEqual(answer, html);
        }
    }
}
