using FCloud3.HtmlGen.Mechanics;
using FCloud3.HtmlGen.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.HtmlGen.Options
{
    /// <summary>
    /// 块规则，表示用户用某种方法为行做了标记(LineMatched)，并希望相邻的同规则行成为一个块
    /// </summary>
    public interface IHtmlBlockRule
    {
        /// <summary>
        /// 定义规则最后应该如何应用
        /// </summary>
        /// <param name="htmlable"></param>
        /// <returns></returns>
        public string Apply(ElementCollection content);
        /// <summary>
        /// 检测某行是否属于该类型的块
        /// </summary>
        /// <param name="line">要检测的行</param>
        /// <returns>是否属于该类型的块</returns>
        public bool LineMatched(string line);
        /// <summary>
        /// 去除某行的块类型标记
        /// </summary>
        /// <param name="line">要去除标记的行</param>
        /// <returns>去除了标记的行</returns>
        public string GetPureContentOf(string line);
        /// <summary>
        /// 将一系列属于该类型的行的内容转换为块元素
        /// </summary>
        /// <param name="lines">行内容</param>
        /// <param name="inlineParser">行内解析器</param>
        /// <param name="blockParser">块解析器</param>
        /// <returns>按本规则解析得的块元素</returns>
        public RuledBlockElement MakeBlockFromLines(IEnumerable<string> lines,IInlineParser inlineParser,IRuledBlockParser blockParser);
    }

    /// <summary>
    /// 表示没有任何标记的默认块规则
    /// </summary>
    public class HtmlEmptyBlockRule : IHtmlBlockRule
    {
        public string Apply(ElementCollection content)
        {
            return content.ToHtml();
        }
        public string GetPureContentOf(string line)
        {
            return line;
        }
        public bool LineMatched(string line)
        {
            return false;
        }
        public RuledBlockElement MakeBlockFromLines(IEnumerable<string> lines, IInlineParser inlineParser, IRuledBlockParser blockParser)
        {
            //可以确定lines是没有块标记的，直接分别解析每行
            var resContent = lines.ToList().ConvertAll(inlineParser.RunForLine);
            var res = new ElementCollection(resContent);
            //构造Rule为空的RuledBlockElement
            return new RuledBlockElement(res,genByRule:null);
        }
        public override bool Equals(object? obj)
        {
            return (obj is HtmlEmptyBlockRule);
        }
        public override int GetHashCode()
        {
            return nameof(HtmlEmptyBlockRule).GetHashCode();
        }
    }

    /// <summary>
    /// 表示在块的每一行前都加了标记的块规则，例如">"
    /// </summary>
    public class HtmlPrefixBlockRule : IHtmlBlockRule
    {
        public string Mark { get; }
        public string PutLeft { get; }
        public string PutRight { get; }
        public string Name { get; }
        public HtmlPrefixBlockRule(string mark, string putLeft, string putRight, string name)
        {
            Mark = mark;
            PutLeft = putLeft;
            PutRight = putRight;
            Name = name;
        }

        public string Apply(ElementCollection content)
        {
            return $"{PutLeft}{content.ToHtml()}{PutRight}";
        }

        public virtual bool LineMatched(string line) 
        {
            return line.Trim().StartsWith(Mark);
        }
        public string GetPureContentOf(string line)
        {
            if (LineMatched(line))
                return line.Trim().Substring(Mark.Length).Trim();
            return line;
        }
        public virtual RuledBlockElement MakeBlockFromLines(IEnumerable<string> lines, IInlineParser inlineParser, IRuledBlockParser blockParser)
        {
            //lines仅去除了本规则的块标记，不清楚里面是否有第二层块标记，需要调用blockParser得到内容ElementCollection
            var resContent = blockParser.Run(lines.ToList());
            //构造Rule为本规则的RuledBlockElement
            return new RuledBlockElement(resContent,genByRule:this);
        }
        

        public override bool Equals(object? obj)
        {
            if (obj is HtmlPrefixBlockRule rule)
                return rule.Mark == this.Mark;
            return false;
        }
        public override int GetHashCode()
        {
            return Mark.GetHashCode();
        }
    }

    /// <summary>
    /// 表示每一行前加了"-"的列表
    /// </summary>
    public class HtmlListBlockRule:HtmlPrefixBlockRule
    {
        public HtmlListBlockRule()
            : base("-","<ul>","</ul>","列表")
        {
        }
        public override RuledBlockElement MakeBlockFromLines(IEnumerable<string> lines, IInlineParser inlineParser, IRuledBlockParser blockParser)
        {
            var items = new ElementCollection();
            foreach (var line in lines)
            {
                ListItemElement item = new(inlineParser.Run(line));
                items.Add(item);
            }
            return new(items,this);
        }
        public override bool LineMatched(string line)
        {
            if (!base.LineMatched(line))
                return false;
            line = line.Trim();
            if (line.StartsWith("---"))
                return false;
            return true;
        }
        public class ListItemElement : SimpleBlockElement
        {
            public ListItemElement(ElementCollection content) 
                : base(content, "<li>","</li>")
            {
            }
        }
    }

    /// <summary>
    /// 表示一个分隔线，一行内只有"---"
    /// </summary>
    public class HtmlSepBlockRule: IHtmlBlockRule
    {
        public const char aLineOfChar = '-';
        public string Apply(ElementCollection content)
        {
            return content.ToHtml();
        }

        public string GetPureContentOf(string line)
        {
            return string.Empty;
        }

        public bool LineMatched(string line)
        {
            line = line.Trim();
            if (line.Length >= 3 && line.All(c => c == aLineOfChar))
                return true;
            return false;
        }

        public RuledBlockElement MakeBlockFromLines(IEnumerable<string> lines, IInlineParser inlineParser, IRuledBlockParser blockParser)
        {
            var res = new ElementCollection();
            lines.ToList().ForEach(x => res.Add(new SepElement()));
            return new RuledBlockElement(res,this);
        }
        public class SepElement:BlockElement
        {
            public SepElement() : base(new()) { }
            public override string ToHtml()
            {
                return "<div class=\"sep\"></div>";
            }
        }
    }
    /// <summary>
    /// 表示一个迷你表格，例子：
    /// |姓名|年龄|
    /// |Au|20|
    /// |Br|38|
    /// </summary>
    public class HtmlMiniTableBlockRule : IHtmlBlockRule
    {
        public const char tableSep = '|';

        public string Apply(ElementCollection content)
        {
            return $"<table>{content.ToHtml()}</table>";
        }

        public bool LineMatched(string line)
        {
            return line.Length>=2 && line.StartsWith(tableSep) && line.EndsWith(tableSep);
        }
        public string GetPureContentOf(string line)
        {
            if (LineMatched(line))
                return line.Substring(1,line.Length-2).Trim();
            return line;
        }
        public RuledBlockElement MakeBlockFromLines(IEnumerable<string> lines, IInlineParser inlineParser, IRuledBlockParser blockParser)
        {
            ElementCollection rows = new();

            List<string[]> text = lines.ToList().ConvertAll(x => x.Split(tableSep));
            int width = text.Select(x => x.Length).Max();
            int headSepRowIndex = text.FindIndex(x => x.All(y => y.Length>=3 && y.All(c => c == '-')));

            for(int lineIndex=0; lineIndex<text.Count; lineIndex++)
            {
                if (lineIndex == headSepRowIndex)
                    continue;
                bool isHead = lineIndex < headSepRowIndex;

                string[] line = text[lineIndex];
                ElementCollection rowCells = new();
                foreach(var cell in line)
                {
                    var cellContent = inlineParser.Run(cell);
                    rowCells.Add(new TableCellElement(cellContent,isHead));
                }
                int left = width - rowCells.Count;
                for (int i = 0; i < left; i++)
                    rowCells.Add(new TableCellElement(new(),isHead));
                rows.Add(new TableRowElement(rowCells));
            }
            return new RuledBlockElement(rows, genByRule: this);
        }
        public override bool Equals(object? obj)
        {
            return (obj is HtmlMiniTableBlockRule);
        }
        public override int GetHashCode()
        {
            return nameof(HtmlMiniTableBlockRule).GetHashCode();
        }

        public class TableRowElement : SimpleBlockElement
        {
            public TableRowElement(ElementCollection content)
                : base(content, "<tr>", "</tr>")
            {
            }
        }
        public class TableCellElement : BlockElement
        {
            public bool IsHead { get; }
            public TableCellElement(ElementCollection content, bool isHead = false) : base(content)
            {
                IsHead = isHead;
            }
            public override string ToHtml()
            {
                if (!IsHead)
                    return $"<td>{base.ToHtml()}</td>";
                return $"<th>{base.ToHtml()}</th>";
            }
        }
    }

    /// <summary>
    /// 系统提供的特殊块规则实例
    /// </summary>
    public static class InternalBlockRules
    {
        public static List<IHtmlBlockRule> GetInstances()
        {
            return new List<IHtmlBlockRule>()
            {
                new HtmlListBlockRule(),
                new HtmlSepBlockRule(),
                new HtmlMiniTableBlockRule()
            };
        }
    }
}
