using FCloud3.Diff.String;

namespace FCloud3.Diff.Object
{
    public static class ObjectDiffSearch
    {
        public static ObjectDiff Run(object a, object b, int alighThrs = default)
        {
            string jsonA = ObjectDiff.ObjectReadable(a);
            string jsonB = ObjectDiff.ObjectReadable(b);
            return new(StringDiffSearch.Run(jsonA, jsonB, alighThrs));
        }
    }
}
