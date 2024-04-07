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
            var settings = new JsonSerializerSettings();
            settings.Formatting = Formatting.Indented;
            return JsonConvert.SerializeObject(obj, settings);
        }
    }
}
