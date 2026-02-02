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
        public static IEnumerable<object[]> CommonData => new object[][]
        {
            new object[] { "\n123\n\r456\n789", 3 },
            new object[] { "\n12{{3\n45}}6\n789", 2 },
            new object[] { "\n12{{3\n\r456\n78}}9", 1 }
        };

        [TestMethod]
        [DynamicData(nameof(CommonData))]
        public void Common(string content, int answer)
        {
            var res = LineSplitter.Split(content, null);
            Assert.AreEqual(answer, res.Count);
        }

        public static IEnumerable<object[]> HtmlAreaRangeData => new object[][]
        {
            new object[] { "01234567890123456789", "", "无html" },
            new object[] { "01<b>56789</b>456789", "2,13", "单个标签" },
            new object[] { "01<b>56</b>12345<b>901234</b>9", "2,10;16,28", "两个标签" },
            new object[] { "01<b>56<a>12345</a>901234</b>9", "2,28", "嵌套标签" },
            new object[] { "01<b>56<b>12345</b>901234</b>9", "2,28", "嵌套同名标签" },
            new object[] { "01<b>56<a>12345</a>90123456789", "2,18", "外标签未闭合" },
            new object[] { "01<b>56<img>2345678901234</b>9", "2,28", "内标签未闭合" },
            new object[] { "01<b at=\"val\">4567890123</b>89", "2,27", "有属性单个标签" },
            new object[] { "01<b at=\"val\">4<a>8</a>3</b>89", "2,27", "有属性嵌套标签" }
        };

        [TestMethod]
        [DynamicData(nameof(HtmlAreaRangeData))]
        public void HtmlAreaRange(string content, string rangesStr, string displayName)
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
