using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.HtmlGen.Util
{
    public class ErrMsg
    {
        public static string Inline(string errmsg)
        {
            return $"<b style=\"color:red\">{errmsg}</b>";
        }
        public static string Block(string errmsg)
        {
            return $"<div style=\"color:red\">{errmsg}</div>";
        }
    }
}
