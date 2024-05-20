using FCloud3.Diff.Display;
using FCloud3.Diff.String;

namespace FCloud3.Diff.Test.Display
{
    [TestClass]
    public class DiffDisplayTest
    {
        [TestMethod]
        [DataRow("01234567890", "0123abc7890", 2, "2345678~2,5", "23abc78~2,5")]
        [DataRow("01234567890", "0123abcd7890", 2, "2345678~2,5", "23abcd78~2,6")]
        [DataRow("01234567890", "0123ab7890", 2, "2345678~2,5", "23ab78~2,4")]
        [DataRow("01234567890", "0123abc7890", 1, "34567~1,4", "3abc7~1,4")]
        public void Single(string ori, string now, int thickness, string showOri, string showNow)
        {
            var diffs = StringDiffSearch.Run(ori, now);
            var content = now.ToList();
            var disp = DiffDisplay.Make(content, diffs, thickness);
            var expectedOri = ParseExpect(showOri);
            var expectedNow = ParseExpect(showNow);
            AssertDiffDisplays(expectedOri, disp.From);
            AssertDiffDisplays(expectedNow, disp.To);
        }

        [TestMethod]
        [DataRow("01234567890", "012a4567c90", 1, "234~1,2|789~1,2", "2a4~1,2|7c9~1,2")]
        [DataRow("01234567890", "012a45678c0", 1, "234~1,2|890~1,2", "2a4~1,2|8c0~1,2")]
        [DataRow("01234567890", "012a45678kkc0", 1, "234~1,2|890~1,2", "2a4~1,2|8kkc0~1,4")]
        [DataRow("01234567890", "012a45678c0", 2, "12345~2,3|7890~2,3", "12a45~2,3|78c0~2,3")]
        [DataRow("01234567890", "012a4567c90", 2, "1234567890~2,3;7,8", "12a4567c90~2,3;7,8")]
        [DataRow("01234567890", "012a4567c90", 3, "01234567890~3,4;8,9", "012a4567c90~3,4;8,9")]
        [DataRow("01234567890", "012a4567c90", 20, "01234567890~3,4;8,9", "012a4567c90~3,4;8,9")]
        public void Mutiple(string ori, string now, int thickness, string showOri, string showNow)
        {
            var diffs = StringDiffSearch.Run(ori, now);
            var content = now.ToList();
            var disp = DiffDisplay.Make(content, diffs, thickness);
            var expectedOri = ParseExpect(showOri);
            var expectedNow = ParseExpect(showNow);
            AssertDiffDisplays(expectedOri, disp.From);
            AssertDiffDisplays(expectedNow, disp.To);
        }

        [TestMethod]
        [DataRow("01234567890", "012a4567c90", "012A4567C90", 1,
            "234~1,2|789~1,2", "2a4~1,2|7c9~1,2", "2a4~1,2|7c9~1,2", "2A4~1,2|7C9~1,2")]
        [DataRow("01234567890", "012a4567c90", "012A4567C90", 2,
            "1234567890~2,3;7,8", "12a4567c90~2,3;7,8", "12a4567c90~2,3;7,8", "12A4567C90~2,3;7,8")]
        [DataRow("01234567890", "012a4567c90", "012a4c90", 1,
            "234~1,2|789~1,2", "2a4~1,2|7c9~1,2", "4567c~1,4", "4c~1,1")]
        [DataRow("01234567890", "012a4567c90", "012a4x67y90", 1,
            "234~1,2|789~1,2", "2a4~1,2|7c9~1,2", "4567c9~1,2;4,5", "4x67y9~1,2;4,5")]
        public void MutipleMutistep(string _1, string _2, string _3, int thickness,
            string show_12_1, string show_12_2, string show_23_2, string show_23_3)
        {
            var diffs_12 = StringDiffSearch.Run(_1, _2);
            var diffs_23 = StringDiffSearch.Run(_2, _3);
            var content = _3.ToList();
            var disp_23 = DiffDisplay.Make(content, diffs_23, thickness);
            var disp_12 = DiffDisplay.Make(content, diffs_12, thickness);
            var expected_23_2 = ParseExpect(show_23_2);
            var expected_23_3 = ParseExpect(show_23_3);
            AssertDiffDisplays(expected_23_2, disp_23.From);
            AssertDiffDisplays(expected_23_3, disp_23.To);
            var expected_12_1 = ParseExpect(show_12_1);
            var expected_12_2 = ParseExpect(show_12_2);
            AssertDiffDisplays(expected_12_1, disp_12.From);
            AssertDiffDisplays(expected_12_2, disp_12.To);
        }


        private static void AssertDiffDisplays(List<DiffDisplayFrag> expected, List<DiffDisplayFrag> actual)
        {
            Assert.AreEqual(expected.Count, actual.Count);
            for(int i = 0; i < expected.Count; i++)
            {
                var ai = actual[i];
                var ei = expected[i];
                Assert.AreEqual(ei.Text, ai.Text);
                Assert.AreEqual(ei.High.Count, ai.High.Count);
                for(int j = 0; j < ai.High.Count; j++)
                {
                    var ah = ai.High[j];
                    var eh = ei.High[j];
                    Assert.AreEqual(eh.Length, 2);
                    Assert.AreEqual(ah.Length, 2);
                    Assert.AreEqual(eh[0], ah[0]);
                    Assert.AreEqual(eh[1], ah[1]);
                }
            }
        }
        private static List<DiffDisplayFrag> ParseExpect(string expect)
        {
            var fragStrs = expect.Split('|');
            List<DiffDisplayFrag> frags = [];
            foreach(var fragStr in fragStrs)
            {
                var parts = fragStr.Split('~');
                var content = parts[0];
                var highlightStrs = parts[1];
                var highlightStrsParts = highlightStrs.Split(";");
                List<int[]> highlights = [];
                foreach(var highlightStr in highlightStrsParts)
                {
                    var fromTo = highlightStr.Split(',').Select(int.Parse).ToArray();
                    highlights.Add(fromTo);
                }
                frags.Add(new(content, highlights));
            }
            return frags;
        }
    }
}
