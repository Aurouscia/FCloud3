using FCloud3.Diff.String;
using FCloud3.Diff.Test.String.Support;

namespace FCloud3.Diff.Test.String
{
    [TestClass]
    public class StringDiffRevertTest
    {
        public static IEnumerable<object[]> SingleTestData()
        {
            yield return new object[] { "1234567", "12ABC67", "2-345-3" }; // length_same
            yield return new object[] { "1234567", "12AB67", "2-345-2" }; // longer_ori_1
            yield return new object[] { "1234567", "12ABCD67", "2-345-4" }; // longer_new_1
            yield return new object[] { "1234567", "12A67", "2-345-1" }; // longer_ori_2
            yield return new object[] { "1234567", "12ABCDE67", "2-345-5" }; // longer_new_2
            yield return new object[] { "1234ABCDEFGHI", "12345", "4-ABCDEFGHI-1" }; // longer_ori_3
            yield return new object[] { "12345", "1234ABCDEFGHI", "4-5-9" }; // longer_new_3
        }

        [TestMethod]
        [DynamicData(nameof(SingleTestData))]
        public void Single(string oldStr, string newStr, string strDiff)
        {
            var diff = StrStringDiff.Parse(strDiff);
            var charList = newStr.ToList();
            diff.Revert(charList);
            var reverted = new string(charList.ToArray());
            Assert.AreEqual(oldStr, reverted);
        }

        public static IEnumerable<object[]> MutipleTestData()
        {
            yield return new object[] { "1234567", "1A345B7", "1-2-1|5-6-1" };
            yield return new object[] { "1234567", "1AAA345B7", "1-2-3|5-6-1" };
            yield return new object[] { "1234567", "1A5B7", "1-234-1|5-6-1" };
            yield return new object[] { "1234567", "1A4BBB7", "1-23-1|4-56-3" };
        }

        [TestMethod]
        [DynamicData(nameof(MutipleTestData))]
        public void Mutiple(string oldStr, string newStr, string strDiff)
        {
            var diffs = StrStringDiff.ParseList(strDiff);
            var reverted = diffs.RevertAll(newStr);
            Assert.AreEqual(oldStr, reverted);
        }

        public static IEnumerable<object[]> UpDownTestData()
        {
            yield return new object[] { "1234567", "1A345B7" };
            yield return new object[] { "1234567", "1AAA345B7" };
            yield return new object[] { "1234567", "1A5B7" };
            yield return new object[] { "1234567", "1A4BBB7" };
            yield return new object[] { "1234567", "1234\n567" };
            yield return new object[] { "1234567", "1234567\n" };
            yield return new object[] { "1234567", "123456X\n" };
            yield return new object[] { "1234567", "\n1234567" };
            yield return new object[] { "1234567", "\nX234567" };
            yield return new object[] { "\n1234567", "1234567" };
            yield return new object[] { "\n1234567", "X234567" };
            yield return new object[] { "1234567\n", "1234567" };
            yield return new object[] { "1234567\n", "123456X" };
            yield return new object[] { "123ABC", "123\nX\nX\nABC" };
        }

        [TestMethod]
        [DynamicData(nameof(UpDownTestData))]
        public void UpDown(string oldStr, string newStr)
        {
            var diffs = StringDiffSearch.Run(oldStr, newStr);
            var reverted = diffs.RevertAll(newStr);
            Assert.AreEqual(oldStr, reverted);
        }

        public static IEnumerable<object[]> MutiStepsTestData()
        {
            yield return new object[] { "123456", "123AA56", "12CCA56" };
            yield return new object[] { "123456789", "1AA4567AAA9", "12CC456CCCC" };
        }

        [TestMethod]
        [DynamicData(nameof(MutiStepsTestData))]
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
