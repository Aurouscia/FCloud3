using FCloud3.Diff.String;

namespace FCloud3.DiffTest.String.Support
{
    public static class AssertDiff
    {
        public static void Same(StringDiff expected,  StringDiff actual)
        {
            Assert.AreEqual(expected.Index, actual.Index);
            Assert.AreEqual(expected.Ori, actual.Ori);
            Assert.AreEqual(expected.New, actual.New);
        }
        public static void SameList(List<StringDiff> expected, List<StringDiff> actual)
        {
            Assert.AreEqual(expected.Count, actual.Count);
            for(int i=0;i<expected.Count; i++)
            {
                Same(expected[i], actual[i]);
            }
        }
    }
}
