using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.HtmlGen.Util
{
    public static class HtmlColor
    {
        public static bool IsValidColorStr(string str)
        {
            return IsRgbValues(str) || IsHexColorRepr(str) || IsNamedColor(str);
        }
        public static bool TryFormalize(string raw,out string res)
        {
            raw = raw.Trim();
            if (IsRgbValues(raw))
            {
                res = $"rgb({raw})";
                return true;
            }
            if (IsHexColorRepr(raw))
            {
                res = $"#{raw}";
                return true;
            }
            if (IsNamedColor(raw))
            {
                res = raw.ToLower();
                return true;
            }
            res = raw;
            return false;
        }
        public static bool IsHexColorRepr(string str)
        {
            str = str.Trim();
            if (str.Length != 3 && str.Length != 6)
                return false;
            if (str.All(c => hexChars.Contains(c)))
                return true;
            return false;
        }
        public static bool IsRgbValues(string str)
        {
            str = str.Replace('，', ',');
            if (str.Contains(','))
            {
                string[] rgb = str.Split(",");
                if (rgb.Length != 3)
                    return false;
                foreach (var num in rgb)
                {
                    if (!int.TryParse(num, out var n) || n < 0 || n > 255)
                        return false;
                }
                return true;
            }
            return false;
        }
        public static bool IsNamedColor(string str)
        {
            return colorNames.Contains(str.Trim().ToLower());
        }

        public readonly static char[] hexChars
            = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f' };

        public readonly static string[] colorNames = { "aqua", "navy", "black", "olive", "blue", "purple", "fuchsia", "red", "aliceblue", "aquamarine", "beig" +
                "e", "black", "blue", "brown", "cadetblue", "chocolate", "cornflowerblue", "cyan", "darkcyan", "darkgray", "darkhaki", "darkolivegree" +
                "n", "darkorchid", "darksalmon", "darkslateblue", "darkturquoise", "deeppink", "dimgray", "firebrick", "forestgreen", "gostwhi" +
                "te", "golenrod", "green", "honeydew", "indianred", "khaki", "lavenderblush", "lemonchiffon", "lightcoral", "lightgodenr" +
                "od", "lightgray", "lightpink", "lightseagreen", "lightslateblue", "lightsteelblue", "limegreen", "magenta", "mediumaquam" +
                "arine", "mediumorchid", "mediumseagreen", "mediumspringgreen", "mediumvioletred", "mintcream", "moccasin", "navy", "oldlac" +
                "e", "orange", "orchid", "palegreen", "palevioletred", "peachpuff", "pink", "powderblue", "red", "royalblue", "salmon", "se" +
                "agreen", "sienna", "slateblue", "snow", "steelblue", "thistle", "turquoise", "violetred", "hite", "yellow", "gray", "si" +
                "lver", "green", "teal", "lime", "yellow", "maroon", "white", "antiquewith", "azure", "bisque", "blanchedalmond", "bluevi" +
                "olet", "burlywood", "chartreuse", "coral", "cornsilk", "darkblue", "darkgoldenrod", "darkgreen", "darkmagenta", "darkor" +
                "enge", "darkred", "darkseagreen", "darkslategray", "darkviolet", "deepskyblue", "dodgerblue", "floralwhite", "gainsbor" +
                "o", "gold", "gray", "greenyellow", "hotpink", "ivory", "lavender", "lawngreen", "lightblue", "lightcyan", "lightgodenr" +
                "odyellow", "lightgreen", "lightsalmon", "lightskyblue", "lightslategray", "lightyellow", "linen", "maroon", "mediumbl" +
                "ue", "mediumpurpul", "mediumslateblue", "mediumturquoise", "midnightblue", "mistyrose", "navajowhite", "navyblue", "oli" +
                "vedrab", "orengered", "palegodenrod", "paleturquoise", "papayawhip", "peru", "plum", "purple", "rosybrown", "saddleb" +
                "rown", "sandybrown", "seashell", "skyblue", "slategray", "springgreen", "tan", "tomato", "violet", "wheat", "whitesmoke", "yellowgreen" };
    }
}
