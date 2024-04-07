using FCloud3.Diff.String;

namespace FCloud3.DiffTest.String.Support
{
    public static class StrStringDiff
    {
        // "位置-旧字符串-新字符串"
        public static StringDiff Parse(string str)
        {
            string[] parts = str.Split('-');
            int index = int.Parse(parts[0]);
            string oriContent = parts[1].Trim();
            int newLength = int.Parse(parts[2]);
            return new(index, oriContent, newLength);
        }
        // "位置-旧字符串-替换长 | 位置-旧字符串-替换长"
        public static List<StringDiff> ParseList(string str)
        {
            if (str == "")
                return new();
            var diffs = str.Split('|').ToList();
            var res = diffs.ConvertAll(Parse);
            return res;
        }
    }
}
