using System.Drawing;

namespace FCloud3.WikiPreprocessor.Util
{
    public static class ColorUtil
    {
        public static string ToCssColorString(Color c)
        {
            return $"rgb({c.R},{c.G},{c.B})";
        }

        public const float gamma = 2.2f;
        public static bool IsLightColor(Color c)
        {
            float rr = c.R / 255f;
            float gr = c.G / 255f;
            float br = c.B / 255f;
            float numer = 
                MathF.Pow(rr, gamma) +
                MathF.Pow(1.5f * gr, gamma) +
                MathF.Pow(0.6f * br, gamma);
            float denom =
                1f +
                MathF.Pow(1.5f, gamma) +
                MathF.Pow(0.6f, gamma);
            float l = MathF.Pow(numer / denom, 1 / gamma);
            return l > 0.7f;
        }
    }
}