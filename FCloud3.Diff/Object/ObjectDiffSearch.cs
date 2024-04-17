using FCloud3.Diff.String;

namespace FCloud3.Diff.Object
{
    public static class ObjectDiffSearch
    {
        public static ObjectDiff Run(object? a, object? b, int alighThrs = default)
        {
            string jsonA = a is not null ? ObjectDiff.ObjectReadable(a) : string.Empty;
            string jsonB = b is not null ? ObjectDiff.ObjectReadable(b) : string.Empty;
            return new(StringDiffSearch.Run(jsonA, jsonB, alighThrs));
        }
    }
}
