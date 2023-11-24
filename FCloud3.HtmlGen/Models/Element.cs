using FCloud3.HtmlGen.Util;
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
            _message = "[[该段解析错误]]"+msg;
        } 
        public override string ToHtml()
        {
            return ErrMsg.Inline(_message);
        }
    }

    public class ElementCollection : List<Element>, IHtmlable
    {
        public ElementCollection(Element onlyItem)
        {
            this.Add(onlyItem);
        }
        public ElementCollection(params Element[] items)
        {
            this.AddRange(items);
        }
        public ElementCollection(IEnumerable<Element> items)
        {
            this.AddRange(items);
        }
        public ElementCollection() { }
        public virtual string ToHtml()
        {
            return string.Concat(this.ConvertAll(x => x.ToHtml()));
        }

        public static implicit operator ElementCollection(Element element)
        {
            return new ElementCollection(element);
        }
    }
}
