using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.HtmlGen.Util
{
    internal class HtmlLabel
    {
        public static string Style(string style)
        {
            return $"<style>{style}</style>";
        }
        public static string Script(string script)
        {
            return $"<script>{script}</script>";
        }
        public static string DebugInfo(string info)
        {
            return $"<div style=\"color:gray;text-decoration:underline\">{info}</div>";
        }
    }
}
