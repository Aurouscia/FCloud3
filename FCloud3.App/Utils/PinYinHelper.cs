using System.Text;
using System.Text.RegularExpressions;
using FCloud3.Entities.Wiki;
using TinyPinyin;

namespace FCloud3.App.Utils
{
    public static class PinYinHelper
    {
        /// <summary>
        /// 将字符串中的中文转换为拼音形式
        /// 你好 => ni-hao
        /// Hello世界 => hello-shi-jie
        /// </summary>
        /// <param name="name">要转换的字符串</param>
        /// <returns>转换完的字符串</returns>
        public static string ToUrlName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "";
            List<string> clusters = new();
            StringBuilder buildingCluster = new();
            for (int i = 0; i < name.Length; i++)
            {
                char c = name[i];
                if (PinyinHelper.IsChinese(c))
                {
                    if (buildingCluster.Length > 0)
                    {
                        clusters.Add(buildingCluster.ToString().ToLower());
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
            clusters.RemoveAll(string.IsNullOrWhiteSpace);
            string res;
            if (clusters.Sum(c => c.Length) + clusters.Count > WikiItem.urlPathNameMaxLength)
            {
                var firstLetters = clusters.ConvertAll(c => c[0]).ToArray();
                res = new string(firstLetters);
            }
            else
            {
                res = string.Join('-', clusters);
            }
            res = Regex.Replace(res, "-{2,}", "-");
            return res;
        }
        public static bool IsUrlValidChar(char c)
        {
            return (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') || c == '-';
        }
    }
}
