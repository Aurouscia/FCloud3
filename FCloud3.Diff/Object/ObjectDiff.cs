using FCloud3.Diff.String;
using Newtonsoft.Json;

namespace FCloud3.Diff.Object
{
    public class ObjectDiff : StringDiffCollection
    {
        public ObjectDiff(StringDiffCollection diffs)
        {
            this.AddRange(diffs);
        }
        public static string ObjectReadable(object obj)
        {
            return JsonConvert.SerializeObject(obj, Formatting.Indented);
        }
    }
}
