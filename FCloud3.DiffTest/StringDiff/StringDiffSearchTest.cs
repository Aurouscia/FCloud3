using FCloud3.Diff.StringDiff;

namespace FCloud3.DiffTest.StringDiff
{
    [TestClass]
    public class StringDiffSearchTest
    {
        [TestMethod]
        [DataRow("012345", "012345")]
        public void Identical(string a, string b)
        {
            var diffs = StringDiffSearch.Run(a, b);
            Assert.AreEqual(diffs.Count, 0);
        }

        [TestMethod]
        [DataRow("012345", "01AB45")]
        public void Mutated(string a, string b)
        {
            var diffs = StringDiffSearch.Run(a, b);
            Assert.AreEqual(diffs.Count, 1);
            var d = diffs[0];
            Assert.AreEqual(d.Index, 2);
            Assert.AreEqual(d.Ori, "23");
            Assert.AreEqual(d.New, "AB");
        }

        [TestMethod]
        [DataRow("012345678", "01AB4CD78")]
        public void Mutated_Mutiple(string a, string b)
        {
            var diffs = StringDiffSearch.Run(a, b);
            Assert.AreEqual(diffs.Count, 2);
            var d1 = diffs[0];
            Assert.AreEqual(d1.Index, 2);
            Assert.AreEqual(d1.Ori, "23");
            Assert.AreEqual(d1.New, "AB");
            var d2 = diffs[1];
            Assert.AreEqual(d2.Index, 5);
            Assert.AreEqual(d2.Ori, "56");
            Assert.AreEqual(d2.New, "CD");
        }

        [TestMethod]
        [DataRow("012345678", "01ABC4DE78")]
        public void Mutated_Mutiple2(string a, string b)
        {
            var diffs = StringDiffSearch.Run(a, b);
            Assert.AreEqual(diffs.Count, 2);
            var d1 = diffs[0];
            Assert.AreEqual(d1.Index, 2);
            Assert.AreEqual(d1.Ori, "23");
            Assert.AreEqual(d1.New, "ABC");
            var d2 = diffs[1];
            Assert.AreEqual(d2.Index, 5);
            Assert.AreEqual(d2.Ori, "56");
            Assert.AreEqual(d2.New, "DE");
        }

        [TestMethod]
        [DataRow("012345678", "A234CD78")]
        public void Mutated_AtStart(string a, string b)
        {
            var diffs = StringDiffSearch.Run(a, b);
            Assert.AreEqual(diffs.Count, 2);
            var d1 = diffs[0];
            Assert.AreEqual(d1.Index, 0);
            Assert.AreEqual(d1.Ori, "01");
            Assert.AreEqual(d1.New, "A");
            var d2 = diffs[1];
            Assert.AreEqual(d2.Index, 5);
            Assert.AreEqual(d2.Ori, "56");
            Assert.AreEqual(d2.New, "CD");
        }

        [TestMethod]
        [DataRow("012345678", "01AB456ABCD")]
        public void Mutated_AtEnd(string a, string b)
        {
            var diffs = StringDiffSearch.Run(a, b);
            Assert.AreEqual(diffs.Count, 2);
            var d1 = diffs[0];
            Assert.AreEqual(d1.Index, 2);
            Assert.AreEqual(d1.Ori, "23");
            Assert.AreEqual(d1.New, "AB");
            var d2 = diffs[1];
            Assert.AreEqual(d2.Index, 7);
            Assert.AreEqual(d2.Ori, "78");
            Assert.AreEqual(d2.New, "ABCD");
        }



        [TestMethod]
        [DataRow("012345", "012ABC345")]
        public void Inserted(string a, string b)
        {
            var diffs = StringDiffSearch.Run(a, b);
            Assert.AreEqual(diffs.Count, 1);
            var d = diffs[0];
            Assert.AreEqual(d.Index, 3);
            Assert.AreEqual(d.Ori, "");
            Assert.AreEqual(d.New, "ABC");
        }

        [TestMethod]
        [DataRow("012ABC345", "012345")]
        public void Removed_FromMiddle(string a, string b)
        {
            var diffs = StringDiffSearch.Run(a, b);
            Assert.AreEqual(diffs.Count, 1);
            var d = diffs[0];
            Assert.AreEqual(d.Index, 3);
            Assert.AreEqual(d.Ori, "ABC");
            Assert.AreEqual(d.New, "");
        }


        [TestMethod]
        [DataRow("012", "012ABC")]
        public void Appended(string a, string b)
        {
            var diffs = StringDiffSearch.Run(a, b);
            Assert.AreEqual(diffs.Count, 1);
            var d = diffs[0];
            Assert.AreEqual(d.Index, 3);
            Assert.AreEqual(d.Ori, "");
            Assert.AreEqual(d.New, "ABC");
        }
        [TestMethod]
        [DataRow("012345", "012")]
        public void Removed_FromEnd(string a, string b)
        {
            var diffs = StringDiffSearch.Run(a, b);
            Assert.AreEqual(diffs.Count, 1);
            var d = diffs[0];
            Assert.AreEqual(d.Index, 3);
            Assert.AreEqual(d.Ori, "345");
            Assert.AreEqual(d.New, "");
        }


        [TestMethod]
        [DataRow("012", "ABC012")]
        public void Appended_ToStart(string a, string b)
        {
            var diffs = StringDiffSearch.Run(a, b);
            Assert.AreEqual(diffs.Count, 1);
            var d = diffs[0];
            Assert.AreEqual(d.Index, 0);
            Assert.AreEqual(d.Ori, "");
            Assert.AreEqual(d.New, "ABC");
        }
        [TestMethod]
        [DataRow("012345", "345")]
        public void Removed_FromStart(string a, string b)
        {
            var diffs = StringDiffSearch.Run(a, b);
            Assert.AreEqual(diffs.Count, 1);
            var d = diffs[0];
            Assert.AreEqual(d.Index, 0);
            Assert.AreEqual(d.Ori, "012");
            Assert.AreEqual(d.New, "");
        }


        [TestMethod]
        [DataRow("XX1230", "XX0123")]
        public void Ambiguous_NotAvoided(string a, string b)
        {
            var dfs_withThrs_1 = StringDiffSearch.Run(a, b, 1);
            Assert.AreEqual(2, dfs_withThrs_1.Count);
            var d1 = dfs_withThrs_1[0];
            Assert.AreEqual(2, d1.Index);
            Assert.AreEqual("123", d1.Ori);
            Assert.AreEqual("", d1.New);

            var d2 = dfs_withThrs_1[1];
            Assert.AreEqual(6, d2.Index);
            Assert.AreEqual("", d2.Ori);
            Assert.AreEqual("123", d2.New);
        }

        [TestMethod]
        [DataRow("XX1230", "XX0123")]
        public void Ambiguous_Avoided(string a, string b)
        {
            var dfs_withThrs_2 = StringDiffSearch.Run(a, b, 2);
            Assert.AreEqual(2, dfs_withThrs_2.Count);
            var d1 = dfs_withThrs_2[0];
            Assert.AreEqual(2, d1.Index);
            Assert.AreEqual("", d1.Ori);
            Assert.AreEqual("0", d1.New);

            var d2 = dfs_withThrs_2[1];
            Assert.AreEqual(5, d2.Index);
            Assert.AreEqual("0", d2.Ori);
            Assert.AreEqual("", d2.New);
        }
    }
}