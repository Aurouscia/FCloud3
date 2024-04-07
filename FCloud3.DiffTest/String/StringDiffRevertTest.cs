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
    }
}
