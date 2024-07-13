using System.Drawing;

namespace FCloud3.WikiPreprocessor.Util
{
    public static class ColorUtil
    {
        public static string ToCssColorString(Color c)
        {
            return $"rgb({c.R},{c.G},{c.B})";
        }
    }
}