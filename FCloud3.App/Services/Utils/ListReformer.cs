namespace FCloud3.App.Services.Utils
{
    public static class ListReformer
    {
        public static Dictionary<string, object?[]> Run<T>(List<T> input)
        {
            Dictionary<string, object?[]> res = [];
            Type t = typeof(T);
            var props = t.GetProperties();
            foreach (var p in props)
            {
                object?[] thisPropValues = new object?[input.Count];
                int pointer = 0;
                foreach (var item in input)
                {
                    var val = p.GetValue(item);
                    if (val is DateTime datetime)
                        thisPropValues[pointer] = datetime.ToString("yy-MM-dd HH:mm");
                    else if (val is Boolean boolean)
                        thisPropValues[pointer] = boolean ? 1 : 0;
                    else
                        thisPropValues[pointer] = val;
                    pointer++;
                }
                res.Add(p.Name, thisPropValues);
            }
            return res;
        }
    }
}