using FCloud3.Diff.Display;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.DiffTest.Display
{
    [TestClass]
    public class CutSpanTest
    {
        [TestMethod]
        [DataRow("1,3;7,9", 10, 1, "0,4~1,3|6,10~1,3")]
        [DataRow("0,2;7,9", 10, 2, "0,4~0,2|5,10~2,4")]
        [DataRow("0,2;6,8", 10, 2, "0,10~0,2;6,8")]
        [DataRow("1,3;6,8", 10, 2, "0,10~1,3;6,8")]
        [DataRow("1,4;5,8", 10, 2, "0,10~1,4;5,8")]
        public void Create(string input, int totalLength, int thickness, string expected)
        {
            var from = ParseInput(input);
            var res = CutSpan.Make(from, totalLength, thickness);
            var expect = ParseExpected(expected);
            AssertCutSpanList(expect, res);
        }


        private static void AssertCutSpanList(List<CutSpan> a, List<CutSpan> b)
        {
            Assert.AreEqual(a.Count, b.Count);
            for (int i = 0; i < a.Count; i++) 
            {
                var ai = a[i];
                var bi = b[i];
                Assert.AreEqual(ai.Index, bi.Index);
                Assert.AreEqual(ai.End, bi.End);
                Assert.AreEqual(ai.Highlights.Count, bi.Highlights.Count);
                for(int j = 0; j < bi.Highlights.Count; j++)
                {
                    Assert.AreEqual(ai.Highlights[j].Length, bi.Highlights[j].Length);
                    Assert.AreEqual(ai.Highlights[j][0], bi.Highlights[j][0]);
                    Assert.AreEqual(ai.Highlights[j][1], bi.Highlights[j][1]);
                }
            }
        }
        private static List<int[]> ParseInput(string input)
        {
            return input.Split(';').ToList().ConvertAll(x => x.Split(',').Select(x=>int.Parse(x)).ToArray());
        }
        private static List<CutSpan> ParseExpected(string expected)
        {
            return expected.Split('|').ToList()
                .ConvertAll(x =>
                {
                    var parts = x.Split('~');
                    var head = parts[0].Split(',');
                    var start = int.Parse(head[0]);
                    var end = int.Parse(head[1]);
                    var body = parts[1].Split(';');
                    var highlights = body.Select(x => x.Split(',').Select(x => int.Parse(x)).ToArray()).ToList();
                    return new CutSpan()
                    {
                        Index = start,
                        End = end,
                        Highlights = highlights
                    };
                });
        }
    }
}
