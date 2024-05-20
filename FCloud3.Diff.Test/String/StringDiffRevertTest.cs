using FCloud3.Diff.String;
using FCloud3.Diff.Test.String.Support;

namespace FCloud3.Diff.Test.String
{
    [TestClass]
    public class StringDiffRevertTest
    {
        [TestMethod]
        [DataRow("1234567", "12ABC67", "2-345-3", DisplayName = "length_same")]
        [DataRow("1234567", "12AB67", "2-345-2", DisplayName = "longer_ori_1")]
        [DataRow("1234567", "12ABCD67", "2-345-4", DisplayName = "longer_new_1")]
        [DataRow("1234567", "12A67", "2-345-1", DisplayName = "longer_ori_2")]
        [DataRow("1234567", "12ABCDE67", "2-345-5", DisplayName = "longer_new_2")]
        public void Single(string oldStr, string newStr, string strDiff)
        {
            var diff = StrStringDiff.Parse(strDiff);
            var charList = newStr.ToList();
            diff.Revert(charList);
            var reverted = new string(charList.ToArray());
            Assert.AreEqual(oldStr, reverted);
        }

        [TestMethod]
        [DataRow("1234567","1A345B7", "1-2-1|5-6-1")]
        [DataRow("1234567", "1AAA345B7", "1-2-3|5-6-1")]
        [DataRow("1234567", "1A5B7", "1-234-1|5-6-1")]
        [DataRow("1234567", "1A4BBB7", "1-23-1|4-56-3")]
        public void Mutiple(string oldStr, string newStr, string strDiff)
        {
            var diffs = StrStringDiff.ParseList(strDiff);
            var reverted = diffs.RevertAll(newStr);
            Assert.AreEqual(oldStr, reverted);
        }

        [TestMethod]
        [DataRow("1234567", "1A345B7")]
        [DataRow("1234567", "1AAA345B7")]
        [DataRow("1234567", "1A5B7")]
        [DataRow("1234567", "1A4BBB7")]
        [DataRow("1234567", "1234\n567")]
        [DataRow("1234567", "1234567\n")]
        [DataRow("1234567", "123456X\n")]
        [DataRow("1234567", "\n1234567")]
        [DataRow("1234567", "\nX234567")]
        [DataRow("\n1234567", "1234567")]
        [DataRow("\n1234567", "X234567")]
        [DataRow("1234567\n", "1234567")]
        [DataRow("1234567\n", "123456X")]
        [DataRow("123ABC","123\nX\nX\nABC")]
        public void UpDown(string oldStr, string newStr)
        {
            var diffs = StringDiffSearch.Run(oldStr, newStr);
            var reverted = diffs.RevertAll(newStr);
            Assert.AreEqual(oldStr, reverted);
        }

        [TestMethod]
        [DataRow("123456", "123AA56", "12CCA56")]
        [DataRow("123456789", "1AA4567AAA9", "12CC456CCCC")]
        public void MutiSteps(string _1, string _2, string _3)
        {
            var diffs_1 = StringDiffSearch.Run(_1, _2, 1);
            var diffs_2 = StringDiffSearch.Run(_2, _3, 1);
            var list = _3.ToList();
            diffs_2.RevertAll(list);
            var rev_3_to_2 = new string(list.ToArray());
            Assert.AreEqual(_2, rev_3_to_2);

            diffs_1.RevertAll(list);
            var rev_2_to_1 = new string(list.ToArray());
            Assert.AreEqual(_1, rev_2_to_1);
        }
    }
}
