namespace FCloud3.Repos.Test.TestSupport
{
    public class TestStrParse
    {
        public static List<int> IntList(string str, char sep=',')
        {
            if (string.IsNullOrEmpty(str))
                return [];
            return str.Split(sep).ToList().ConvertAll(int.Parse);
        }
        public static List<List<int>> IntListList(string str, string majorSep = "  ", char sep=',')
        {
            if (string.IsNullOrEmpty(str))
                return [];
            return str.Split(majorSep).ToList().ConvertAll(x=>x.Split(sep).ToList().ConvertAll(int.Parse));
        }
        public static Dictionary<int, List<int>> IntDictInt(string str)
        {
            return str.Split("  ").ToList().ConvertAll(x =>
            {
                var kv = x.Split(':');
                var k = int.Parse(kv[0]);
                var v = IntList(kv[1], ',');
                return new KeyValuePair<int, List<int>>(k, v);
            }).ToDictionary();
        }
    }
}