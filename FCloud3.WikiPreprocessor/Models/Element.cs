using FCloud3.WikiPreprocessor.Context.SubContext;
using FCloud3.WikiPreprocessor.Rules;
using FCloud3.WikiPreprocessor.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.WikiPreprocessor.Models
{
    public abstract class Element : IHtmlable
    {
        public abstract string ToHtml();
        public virtual List<IRule>? ContainRules() => null;
        public virtual List<IHtmlable>? ContainFootNotes() => null;
        public virtual List<ParserTitleTreeNode>? ContainTitleNodes() => null;
        public virtual void WriteHtml(StringBuilder sb)
        { 
            sb.Append(ToHtml());
        }
        public virtual void WriteBody(StringBuilder sb, int maxLength) { }
    }

    public class ErrorElement : Element
    {
        private readonly string _message;
        public ErrorElement(string? msg)
        {
            _message = "[[解析错误]]__"+msg;
        } 
        public override string ToHtml()
        {
            return ErrMsg.Inline(_message);
        }
    }

    public class ElementCollection : List<IHtmlable>, IHtmlable
    {
        public ElementCollection(IHtmlable onlyItem)
        {
            base.Add(onlyItem);
        }
        public ElementCollection(params IHtmlable[] items)
        {
            base.AddRange(items);
        }
        public ElementCollection(IEnumerable<IHtmlable> items)
        {
            base.AddRange(items);
        }
        public ElementCollection() { }
        public void AddFlat(IHtmlable htmlable)
        {
            if (htmlable is Element ele)
                base.Add(ele);
            else if (htmlable is ElementCollection collection)
                base.AddRange(collection);
            else
                throw new NotImplementedException();
        }
        public virtual string ToHtml()
        {
            return string.Concat(this.ConvertAll(x => x.ToHtml()));
        }
        public virtual void WriteHtml(StringBuilder sb)
        {
            this.ForEach(x => x.WriteHtml(sb));
        }
        public virtual void WriteBody(StringBuilder sb, int maxLength)
        {
            bool isFirst = true;
            List<(bool isBlock, string body)> bodies = [];
            int bodiesCurrentTotalLength = 0;
            StringBuilder tempSb = new();
            foreach(var ele in this)
            {
                tempSb.Clear();
                int leftLength = maxLength - sb.Length - bodiesCurrentTotalLength;
                if (leftLength <= 0)
                    break;
                ele.WriteBody(tempSb, leftLength);
                bodiesCurrentTotalLength += tempSb.Length;
                bodies.Add((ele is BlockElement, tempSb.ToString()));
            }
            foreach(var body in bodies)
            {
                if (body.body.Length == 0)
                    continue;
                int leftLength = maxLength - sb.Length;
                if (leftLength <= 0)
                    break;
                if (isFirst)
                    isFirst = false;
                else
                {
                    if (body.isBlock)
                    {
                        sb.Append(' ');
                        leftLength -= 1;
                    }
                }
                if (body.body.Length <= leftLength)
                    sb.Append(body.body);
                else
                    sb.Append(body.body.AsSpan()[..leftLength]);
            }
        }
        public virtual List<IRule> ContainRules()
        {
            List<IRule> res = new();
            foreach(var e in this)
            {
                var r = e.ContainRules();
                if (r is not null)
                    res.AddRange(r);
            }
            return res;
        }
        public virtual List<IHtmlable> ContainFootNotes()
        {
            List<IHtmlable> res = new();
            foreach (var e in this)
            {
                var r = e.ContainFootNotes();
                if (r is not null)
                    res.AddRange(r);
            }
            return res;
        }
        public virtual List<ParserTitleTreeNode> ContainTitleNodes()
        {
            List<ParserTitleTreeNode> res = new();
            foreach (var e in this)
            {
                var r = e.ContainTitleNodes();
                if (r is not null)
                    res.AddRange(r);
            }
            return res;
        }
        public IHtmlable Simplify()
        {
            if (this.Count == 1)
                return this[0];
            else if (this.Count > 1)
                return this;
            else
                return new EmptyElement();
        }

        public static implicit operator ElementCollection(Element element)
        {
            return new ElementCollection(element);
        }
    }
}
