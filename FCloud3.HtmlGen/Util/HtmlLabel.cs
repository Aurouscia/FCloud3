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
        public static string Custom(string content, string name,string attrName,string attrValue)
        {
            return $"<{name} {attrName}=\"{attrValue}\">{content}</{name}>";
        }
        public static void CustomWrite(StringBuilder sb, IHtmlable content, string name,string attrName,string attrValue)
        {
            sb.Append('<');
            sb.Append(name);
            sb.Append(' ');
            sb.Append(attrName);
            sb.Append("=\"");
            sb.Append(attrValue);
            sb.Append("\">");
            content.WriteHtml(sb);
            sb.Append("</");
            sb.Append(name);
            sb.Append('>');
        }
        public static void CustomWrite(StringBuilder sb, string content, string name, string attrName, string attrValue)
        {
            sb.Append('<');
            sb.Append(name);
            sb.Append(' ');
            sb.Append(attrName);
            sb.Append("=\"");
            sb.Append(attrValue);
            sb.Append("\">");
            sb.Append(content);
            sb.Append("</");
            sb.Append(name);
            sb.Append('>');
        }
        public static void CustomWrite(
            StringBuilder sb, IHtmlable content, string name, 
            string attr1Name, string attr1Value, string attr2Name, string attr2Value)
        {
            sb.Append('<');
            sb.Append(name);
            sb.Append(' ');
            sb.Append(attr1Name);
            sb.Append("=\"");
            sb.Append(attr1Value);
            sb.Append('\"');
            sb.Append(' ');
            sb.Append(attr2Name);
            sb.Append("=\"");
            sb.Append(attr2Value);
            sb.Append('\"');
            sb.Append('>');
            content.WriteHtml(sb);
            sb.Append("</");
            sb.Append(name);
            sb.Append('>');
        }
    }
}
