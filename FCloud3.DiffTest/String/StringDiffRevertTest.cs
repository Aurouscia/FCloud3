using FCloud3.Diff.String;
using FCloud3.DiffTest.String.Support;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.DiffTest.String
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
        public void UpDown(string oldStr, string newStr)
        {
            var diffs = StringDiffSearch.Run(oldStr, newStr);
            var reverted = diffs.RevertAll(newStr);
            Assert.AreEqual(oldStr, reverted);
        }
    }
}
