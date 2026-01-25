using FCloud3.Diff.String;
using FCloud3.Diff.Test.String.Support;

namespace FCloud3.Diff.Test.String
{
    [TestClass]
    public class StringDiffSearchTest
    {
        public static IEnumerable<object[]> IdenticalTestData()
        {
            yield return new object[] { "012345", "012345", "" };
            yield return new object[] { "abc", "abc", "" };
            yield return new object[] { "", "", "" };
        }

        [TestMethod]
        [DynamicData(nameof(IdenticalTestData))]
        public void Identical(string a, string b, string strDiffs)
        {
            var diffs = StringDiffSearch.Run(a, b, 1);
            var answers = StrStringDiff.ParseList(strDiffs);
            AssertDiff.SameList(answers, diffs);
        }


        public static IEnumerable<object[]> MutatedTestData()
        {
            yield return new object[] { "012345", "01AB45", "2-23-2" }; // single
            yield return new object[] { "012345", "01ABC45", "2-23-3" }; // single_length_up
            yield return new object[] { "012345", "01A45", "2-23-1" }; // single_length_down
            yield return new object[] { "012345678", "01AB4CD78", "2-23-2|5-56-2" }; // mutiple
            yield return new object[] { "012345678", "01ABZ4CD78", "2-23-3|5-56-2" }; // mutiple_length_up
            yield return new object[] { "012345678", "01A4CD78", "2-23-1|5-56-2" }; // mutiple_length_down
            yield return new object[] { "012345678", "A234CD78", "0-01-1|5-56-2" }; // at_start
            yield return new object[] { "012345678", "01AB456ABCD", "2-23-2|7-78-4" }; // at_end
        }

        [TestMethod]
        [DynamicData(nameof(MutatedTestData))]
        public void Mutated(string a, string b, string strDiffs)
        {
            var diffs = StringDiffSearch.Run(a, b, 1);
            var answers = StrStringDiff.ParseList(strDiffs);
            AssertDiff.SameList(answers, diffs);
        }


        public static IEnumerable<object[]> InsertedTestData()
        {
            yield return new object[] { "012345", "012ABC345", "3--3" }; // single
            yield return new object[] { "012345", "012ABC34K5", "3--3|5--1" }; // mutiple
            yield return new object[] { "012345", "012345ABC", "6--3" }; // at_end
            yield return new object[] { "012345", "ABC012345", "0--3" }; // at_start
        }

        [TestMethod]
        [DynamicData(nameof(InsertedTestData))]
        public void Inserted(string a, string b, string strDiffs)
        {
            var diffs = StringDiffSearch.Run(a, b, 1);
            var answers = StrStringDiff.ParseList(strDiffs);
            AssertDiff.SameList(answers, diffs);
        }

        public static IEnumerable<object[]> RemovedTestData()
        {
            yield return new object[] { "0123456", "01256", "3-34-0" }; // single
            yield return new object[] { "0123456", "01346", "2-2-0|5-5-0" }; // mutiple
            yield return new object[] { "0123456", "23456", "0-01-0" }; // from_end
            yield return new object[] { "0123456", "01234", "5-56-0" }; // from_end
        }

        [TestMethod]
        [DynamicData(nameof(RemovedTestData))]
        public void Removed(string a, string b, string strDiffs)
        {
            var diffs = StringDiffSearch.Run(a, b, 1);
            var answers = StrStringDiff.ParseList(strDiffs);
            AssertDiff.SameList(answers, diffs);
        }

        public static IEnumerable<object[]> ExchangeMisunderstandingAvoidingTestData()
        {
            yield return new object[] { "XX1230XXXX", "XX0123XXXX", "2--1|5-0-0" };
            yield return new object[] { "XX0123XXXX", "XX1230XXXX", "2-0-0|6--1" };
            yield return new object[] { "0123456789ABC", "ABC0123456789", "0--3|10-ABC-0" };
            //                  ^^^    ^^^
            yield return new object[] { "ABC0123456789", "0123456789ABC", "0-ABC-0|13--3" };
            //        ^^^                        ^^^
            yield return new object[] { "0123456789ABCxxxxxxxxx", "ABC0123456789xxxxxxxxx", "0--3|10-ABC-0" };
            //                  ^^^             ^^^
            yield return new object[] { "ABC0123456789xxxxxxxxx", "0123456789ABCxxxxxxxxx", "0-ABC-0|13--3" };
            //        ^^^                                 ^^^
        }

        [TestMethod]
        [DynamicData(nameof(ExchangeMisunderstandingAvoidingTestData))]
        public void ExchangeMisunderstandingAvoiding(string a, string b, string strDiffs)
        {
            //当两部分交换位置时，应记录为更短的那部分被删后被加
            var diffs = StringDiffSearch.Run(a, b);
            var answers = StrStringDiff.ParseList(strDiffs);
            AssertDiff.SameList(answers, diffs);
        }

        public static IEnumerable<object[]> MergeMisunderstandingAvoidingTestData()
        {
            yield return new object[] { "0123456789", "0A234567Z9", "1-1-1|8-8-1" };
            //         ^      ^      ^      ^
            yield return new object[] { "0123456789", "0ABCD56789", "1-1234-4" };
            //         ^^^^          ^^^^
            yield return new object[] { "0123456789", "0ABC4567Z9", "1-123-3|8-8-1" };
            //         ^^^    ^      ^^^    ^
            yield return new object[] { "0123456789", "0ABCD567Z9", "1-1234-4|8-8-1" };
            //         ^^^^   ^      ^^^^   ^
            yield return new object[] { "0123456789", "0ABCDE67Z9", "1-12345-5|8-8-1" };
            //         ^^^^^  ^      ^^^^^  ^
            yield return new object[] { "0123456789", "0ABCD56YZ9", "1-12345678-8" };
            //         ^^^^--^^      ^^^^--^^
            yield return new object[] { "0123456789", "01BCD56YZ9", "2-234-3|7-78-2" };
            //          ^^^  ^^       ^^^  ^^
        }

        [TestMethod]
        [DynamicData(nameof(MergeMisunderstandingAvoidingTestData))]
        public void MergeMisunderstandingAvoiding(string a, string b, string strDiffs)
        {
            //在对齐阈值未被指定的情况下，应与当前指针前进的步数一致，最低为3
            //从0-阈值向后扫描，如果每一步的总匹配率都大于一半则视为对齐
            var diffs = StringDiffSearch.Run(a, b);
            var answers = StrStringDiff.ParseList(strDiffs);
            AssertDiff.SameList(answers, diffs);
        }

        public static IEnumerable<object[]> CompletelyTestData()
        {
            yield return new object[] { "ABCDEFG", "12345", "0-ABCDEFG-5" };
        }

        [TestMethod]
        [DynamicData(nameof(CompletelyTestData))]
        public void Completely(string a, string b, string strDiffs)
        {
            var diffs = StringDiffSearch.Run(a, b, 1);
            var answers = StrStringDiff.ParseList(strDiffs);
            AssertDiff.SameList(answers, diffs);
        }

        public static IEnumerable<object[]> MutiLineTestData()
        {
            yield return new object[] { "0123\n4567", "0123\n4567", "" };
            yield return new object[] { "0123\n4567", "012A\nB567", "3-3\n4-3" };
            yield return new object[] { "0123\n4567\n", "0123\n4567", "9-\n-0" };
            yield return new object[] { "0123\n4567", "0123\n4567\n", "9--1" };
            yield return new object[] { "0123\n4567\n", "0123\n456K", "8-7\n-1" };
            yield return new object[] { "0123\n456K", "0123\n4567\n", "8-K-2" };
            yield return new object[] { "\n0123\n4567", "0123\n4567", "0-\n-0" };
            yield return new object[] { "0123\n4567", "\n0123\n4567", "0--1" };
            yield return new object[] { "01234567", "0123\n4567", "4--1" };
            yield return new object[] { "0123\n4567", "01234567", "4-\n-0" };
        }

        [TestMethod]
        [DynamicData(nameof(MutiLineTestData))]
        public void MutiLine(string a, string b, string strDiffs)
        {
            var diffs = StringDiffSearch.Run(a, b, 3);
            var answers = StrStringDiff.ParseList(strDiffs);
            AssertDiff.SameList(answers, diffs);
        }
    }
}