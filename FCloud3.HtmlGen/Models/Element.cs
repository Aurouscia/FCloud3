using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.HtmlGen.Models
{
    public abstract class Element : IHtmlable
    {
        public abstract string ToHtml();
    }

    public class ErrorElement : Element
    {
        private readonly string _message;
        public ErrorElement(string? msg)
        {
            if (string.IsNullOrEmpty(msg))
                _message = "[该段解析错误]";
            else
                _message = msg;
        } 
        public override string ToHtml()
        {
            return $"<b>{_message}</b>";
        }
    }

    public class ElementCollection : List<Element>, IHtmlable
    {
        public ElementCollection(Element onlyItem)
        {
            this.Add(onlyItem);
        }
        public ElementCollection() { }
        public string ToHtml()
        {
            return string.Concat(this.ConvertAll(x => x.ToHtml()));
        }
    }
}
