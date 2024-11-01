using FCloud3.WikiPreprocessor.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.WikiPreprocessor.Test
{
    [TestClass]
    public class LineSplitTest
    {
        [TestMethod]
        [DataRow(
            "\n123\n\r456\n789", 3)]
        [DataRow(
            "\n12{{3\n45}}6\n789", 2)]
        [DataRow(
            "\n12{{3\n\r456\n78}}9", 1)]
        public void Common(string content, int answer)
        {
            var res = LineSplitter.Split(content, null);
            Assert.AreEqual(answer, res.Count);
        }

        //[TestMethod]
        [DataRow(
            "\n123\n\r456\n789", 3)]
        [DataRow(
            "\n12<b>\n456\n</b>789", 1)]
        [DataRow(
            "\n12<b>\n4\n</b>\n5\n<b>\n6\n</b>\n789", 4)]
        [DataRow(
            "\n12<b>\n456<a href=\"666\">666</a>456\n</b>789", 1)]
        [DataRow(
            "\n12<b>\n456<img src=\"666\">456\n</b>789", 1)]
        public void WithHtml(string content, int answer)
        {
            var res = LineSplitter.Split(content, null);
            Assert.AreEqual(answer, res.Count);
        }

        [TestMethod]
        [DataRow(
            "01234567890123456789", "", DisplayName = "无html")]
        [DataRow(
            "01<b>56789</b>456789", "2,13", DisplayName = "单个标签")]
        [DataRow(
            "01<b>56</b>12345<b>901234</b>9", "2,10;16,28", DisplayName = "两个标签")]
        [DataRow(
            "01<b>56<a>12345</a>901234</b>9", "2,28", DisplayName = "嵌套标签")]
        [DataRow(
            "01<b>56<b>12345</b>901234</b>9", "2,28", DisplayName = "嵌套同名标签")]
        [DataRow(
            "01<b>56<a>12345</a>90123456789", "2,18", DisplayName = "外标签未闭合")]
        [DataRow(
            "01<b>56<img>2345678901234</b>9", "2,28", DisplayName = "内标签未闭合")]
        [DataRow(
            "01<b at=\"val\">4567890123</b>89", "2,27", DisplayName = "有属性单个标签")]
        [DataRow(
            "01<b at=\"val\">4<a>8</a>3</b>89", "2,27", DisplayName = "有属性嵌套标签")]
        public void HtmlAreaRange(string content, string rangesStr)
        {
            List<Range> ans = rangesStr.Split(';').ToList()
                .FindAll(x=>!string.IsNullOrEmpty(x))
                .ConvertAll(x =>
                {
                    var parts = x.Split(',');
                    return new Range(int.Parse(parts[0]), int.Parse(parts[1]));
                });
            var ranges = HtmlArea.FindRanges(content);
            for(int i = 0; i < ranges.Count; i++)
            {
                Assert.AreEqual(ans[i].Start, ranges[i].Start);
                Assert.AreEqual(ans[i].End, ranges[i].End);
            }
        }
    }
}
