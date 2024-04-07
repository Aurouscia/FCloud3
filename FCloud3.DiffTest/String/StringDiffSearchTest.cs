using FCloud3.Diff.String;
using FCloud3.DiffTest.String.Support;

namespace FCloud3.DiffTest.String
{
    [TestClass]
    public class StringDiffSearchTest
    {
        [TestMethod]
        [DataRow("012345", "012345", "")]
        [DataRow("abc", "abc", "")]
        [DataRow("", "", "")]
        public void Identical(string a, string b, string strDiffs)
        {
            var diffs = StringDiffSearch.Run(a, b, 1);
            var answers = StrStringDiff.ParseList(strDiffs);
            AssertDiff.SameList(answers, diffs);
        }


        [TestMethod]
        [DataRow("012345", "01AB45", "2-23-2", DisplayName = "single")]
        [DataRow("012345", "01ABC45", "2-23-3", DisplayName = "single_length_up")]
        [DataRow("012345", "01A45", "2-23-1", DisplayName = "single_length_down")]

        [DataRow("012345678", "01AB4CD78", "2-23-2|5-56-2", DisplayName = "mutiple")]
        [DataRow("012345678", "01ABZ4CD78", "2-23-3|5-56-2", DisplayName = "mutiple_length_up")]
        [DataRow("012345678", "01A4CD78", "2-23-1|5-56-2", DisplayName = "mutiple_length_down")]

        [DataRow("012345678", "A234CD78", "0-01-1|5-56-2", DisplayName = "at_start")]
        [DataRow("012345678", "01AB456ABCD", "2-23-2|7-78-4", DisplayName = "at_end")]
        public void Mutated(string a, string b, string strDiffs)
        {
            var diffs = StringDiffSearch.Run(a, b, 1);
            var answers = StrStringDiff.ParseList(strDiffs);
            AssertDiff.SameList(answers, diffs);
        }


        [TestMethod]
        [DataRow("012345", "012ABC345", "3--3", DisplayName = "single")]
        [DataRow("012345", "012ABC34K5", "3--3|5--1", DisplayName = "mutiple")]
        [DataRow("012345", "012345ABC", "6--3", DisplayName = "at_end")]
        [DataRow("012345", "ABC012345", "0--3", DisplayName = "at_start")]
        public void Inserted(string a, string b, string strDiffs)
        {
            var diffs = StringDiffSearch.Run(a, b, 1);
            var answers = StrStringDiff.ParseList(strDiffs);
            AssertDiff.SameList(answers, diffs);
        }

        [TestMethod]
        [DataRow("0123456", "01256", "3-34-0", DisplayName = "single")]
        [DataRow("0123456", "01346", "2-2-0|5-5-0", DisplayName = "mutiple")]
        [DataRow("0123456", "23456", "0-01-0", DisplayName = "from_end")]
        [DataRow("0123456", "01234", "5-56-0", DisplayName = "from_end")]
        public void Removed(string a, string b, string strDiffs)
        {
            var diffs = StringDiffSearch.Run(a, b, 1);
            var answers = StrStringDiff.ParseList(strDiffs);
            AssertDiff.SameList(answers, diffs);
        }

        [TestMethod]
        [DataRow("XX1230", "XX0123", 1, "2-123-0|6--3")]
        [DataRow("XX1230", "XX0123", 2, "2--1|5-0-0")]
        public void Ambiguous_NotAvoided(string a, string b, int thrs, string strDiffs)
        {
            var diffs = StringDiffSearch.Run(a, b, thrs);
            var answers = StrStringDiff.ParseList(strDiffs);
            AssertDiff.SameList(answers, diffs);
        }

        [TestMethod]
        [DataRow("ABCDEFG", "12345", "0-ABCDEFG-5")]
        public void Completely(string a, string b, string strDiffs)
        {
            var diffs = StringDiffSearch.Run(a, b, 1);
            var answers = StrStringDiff.ParseList(strDiffs);
            AssertDiff.SameList(answers, diffs);
        }

        [TestMethod]
        [DataRow("0123\n4567", "0123\n4567", "")]
        [DataRow("0123\n4567", "012A\nB567", "3-3\n4-3")]
        [DataRow("0123\n4567\n", "0123\n4567", "9-\n-0")]
        [DataRow("0123\n4567", "0123\n4567\n", "9--1")]
        [DataRow("0123\n4567\n", "0123\n456K", "8-7\n-1")]
        [DataRow("0123\n456K", "0123\n4567\n", "8-K-2")]
        [DataRow("\n0123\n4567", "0123\n4567", "0-\n-0")]
        [DataRow("0123\n4567", "\n0123\n4567", "0--1")]
        [DataRow("01234567", "0123\n4567", "4--1")]
        [DataRow("0123\n4567", "01234567", "4-\n-0")]
        public void MutiLine(string a, string b, string strDiffs)
        {
            var diffs = StringDiffSearch.Run(a, b, 3);
            var answers = StrStringDiff.ParseList(strDiffs);
            AssertDiff.SameList(answers, diffs);
        }
    }
}