using System.Text;
using System.Text.RegularExpressions;
using TinyPinyin;

namespace FCloud3.Utils.Utils.UrlPath
{
    public static class UrlPathNameUtil
    {
        public static string ToUrlName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new Exception("名称不能为空(url名转换)");
            List<string> clusters = new();
            StringBuilder buildingCluster = new();
            for(int i = 0; i < name.Length; i++)
            {
                char c = name[i];
                if (PinyinHelper.IsChinese(c))
                {
                    if (buildingCluster.Length > 0)
                    {
                        clusters.Add(buildingCluster.ToString());
                        buildingCluster.Clear();
                    }
                    clusters.Add(PinyinHelper.GetPinyin(c).ToLower());
                }
                else if (IsUrlValidChar(c))
                {
                    buildingCluster.Append(c);
                }
            }
            if (buildingCluster.Length > 0)
            {
                clusters.Add(buildingCluster.ToString().ToLower());
            }
            string res = string.Join('-',clusters);
            res = res.Replace("---", "-").Replace("--","-");
            return res;
        }
        private static bool IsUrlValidChar(char c)
        {
            return (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c>='0'&&c<='9') || c=='-';
        }
    }
}
