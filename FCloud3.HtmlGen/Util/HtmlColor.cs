using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.HtmlGen.Util
{
    public class HtmlColor
    {
        public static string Formalize(string raw)
        {
            raw = raw.Replace('，', ',');
            if(raw.Contains(','))
            {
                string[] rgb = raw.Split(",");
                if (rgb.Length != 3)
                    throw new Exception("颜色格式异常，RGB参数应有三个");
                foreach(var num in rgb)
                {
                    if (!int.TryParse(num, out var n) || n<0 || n>255)
                        throw new Exception("颜色格式异常，RGB应该是0-255的整数");
                }
                return $"rgb({raw})";
            }
            return raw;
        }
    }
}
