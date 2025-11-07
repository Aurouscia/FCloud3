using FCloud3.WikiPreprocessor.Context;
using FCloud3.WikiPreprocessor.Mechanics;
using FCloud3.WikiPreprocessor.Models;
using FCloud3.WikiPreprocessor.Util;
using System.Text;
using System.Text.RegularExpressions;
using DotNetColorParser;

namespace FCloud3.WikiPreprocessor.Rules
{
    /// <summary>
    /// 块规则，表示用户用某种方法为行做了标记(LineMatched)，并希望相邻的同规则行成为一个块
    /// </summary>
    public interface IBlockRule : IRule,IEquatable<IBlockRule>
    {
        /// <summary>
        /// 定义规则最后应该如何应用
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public string Apply(IHtmlable content);
        public void ApplyWrite(StringBuilder sb, IHtmlable content);
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
        /// <param name="context">上下文</param>
        /// <returns>按本规则解析得的块元素</returns>
        public IHtmlable MakeBlockFromLines(IEnumerable<LineAndHash> lines,IInlineParser inlineParser,IRuledBlockParser blockParser, ParserContext context);
    }

    public abstract class BlockRule : IBlockRule
    {
        public string Name { get; }
        public string PutLeft { get; protected set; }
        public string PutRight { get; protected set; }
        public string Style { get; }
        public bool IsSingleUse { get; }
        public BlockRule(string putLeft="",string putRight="", string style = "",string name="",bool isSingleUse = false)
        {
            Style = style;
            PutLeft = putLeft;
            PutRight = putRight;
            Name = name;
            IsSingleUse = isSingleUse;
        }
        public virtual string Apply(IHtmlable content) => $"{PutLeft}{content.ToHtml()}{PutRight}";
        public virtual void ApplyWrite(StringBuilder sb, IHtmlable content)
        {
            sb.Append(PutLeft);
            content.WriteHtml(sb);
            sb.Append(PutRight);
        }
        public abstract bool LineMatched(string line);
        public abstract string GetPureContentOf(string line);
        public abstract IHtmlable MakeBlockFromLines(IEnumerable<LineAndHash> lines, IInlineParser inlineParser, IRuledBlockParser blockParser, ParserContext context);
        public virtual string GetStyles() => Style;
        public virtual string GetPreScripts() => string.Empty;
        public virtual string GetPostScripts() => string.Empty;
        public abstract bool Equals(IBlockRule? other);
        public abstract string UniqueName { get; }
    }

    /// <summary>
    /// 表示没有任何标记的默认块规则
    /// </summary>
    public class EmptyBlockRule : BlockRule
    {
        public override string Apply(IHtmlable content)
        {
            return content.ToHtml();
        }
        public override string GetPureContentOf(string line)
        {
            return line;
        }
        public override bool LineMatched(string line)
        {
            return false;
        }
        public override IHtmlable MakeBlockFromLines(IEnumerable<LineAndHash> lines, IInlineParser inlineParser, IRuledBlockParser blockParser, ParserContext context)
        {
            //可以确定lines是没有块标记的，直接分别解析每行
            var resContent = lines.ToList().ConvertAll(inlineParser.RunForLine);
            if (resContent.Count == 1)
                return resContent[0];
            else
                return new ElementCollection(resContent);
        }
        public override bool Equals(object? obj)
        {
            return Equals(obj as IBlockRule);
        }
        public override int GetHashCode()
        {
            return nameof(EmptyBlockRule).GetHashCode();
        }
        public override bool Equals(IBlockRule? other)
        {
            return other is EmptyBlockRule;
        }
        public override string UniqueName => "空块规则";
    }

    /// <summary>
    /// 表示在块的每一行前都加了标记的块规则，例如">"
    /// </summary>
    public class PrefixBlockRule : BlockRule
    {
        public string Mark { get; }
        public PrefixBlockRule(string mark, string putLeft, string putRight, string name,string style = "") 
            : base(putLeft,putRight,style,name)
        {
            Mark = mark;
        }

        public override bool LineMatched(string line) 
        {
            return line.StartsWith(Mark);
        }
        public override string GetPureContentOf(string line)
        {
            if (LineMatched(line))
                return line.Substring(Mark.Length).Trim();
            return line;
        }
        public override IHtmlable MakeBlockFromLines(IEnumerable<LineAndHash> lines, IInlineParser inlineParser, IRuledBlockParser blockParser, ParserContext context)
        {
            //lines仅去除了本规则的块标记，不清楚里面是否有第二层块标记，需要调用blockParser得到内容ElementCollection
            var resContent = blockParser.Run(lines.ToList());
            //构造Rule为本规则的RuledBlockElement
            return new RuledBlockElement(resContent,genByRule:this);
        }
        

        public override bool Equals(object? obj)
        {
            return Equals(obj as IBlockRule);
        }
        public override int GetHashCode()
        {
            return Mark.GetHashCode();
        }
        public override bool Equals(IBlockRule? other)
        {
            if (other is PrefixBlockRule rule)
                return rule.Mark == this.Mark;
            return false;
        }
        public override string UniqueName => $"前缀块规则_{Mark}";
    }

    /// <summary>
    /// 表示每一行前加了"-"的列表
    /// </summary>
    public class ListBlockRule:PrefixBlockRule
    {
        public ListBlockRule()
            : base("-","<ul>","</ul>","列表")
        {
        }
        public override RuledBlockElement MakeBlockFromLines(IEnumerable<LineAndHash> lines, IInlineParser inlineParser, IRuledBlockParser blockParser, ParserContext context)
        {
            var items = new ElementCollection();
            foreach (var line in lines)
            {
                ListItemElement item = new(inlineParser.Run(line.Text));
                items.Add(item);
            }
            return new(items,this);
        }
        public override bool LineMatched(string line)
        {
            if (!base.LineMatched(line))
                return false;
            if (line.StartsWith("---"))
                return false;
            return true;
        }
        public class ListItemElement : SimpleBlockElement
        {
            public ListItemElement(IHtmlable content) 
                : base(content, "<li>","</li>")
            {
            }
        }

    }

    public class LineCommentRule : BlockRule
    {
        public const string lineCommentPrefix = "//";
        public override string UniqueName => "行注释";

        public override bool Equals(IBlockRule? other)
            => other is LineCommentRule;

        public override string GetPureContentOf(string line)
            => string.Empty;

        public override bool LineMatched(string line)
        {
            var firstNoWhite = -1;
            for (int i = 0; i < line.Length; i++)
            {
                if (!char.IsWhiteSpace(line[i]))
                {
                    firstNoWhite = i; break;
                }
            }
            if(firstNoWhite >= 0)
            {
                ReadOnlySpan<char> trimmed = line.AsSpan(firstNoWhite);
                return trimmed.StartsWith(lineCommentPrefix);
            }
            return false;
        }

        public override IHtmlable MakeBlockFromLines(
            IEnumerable<LineAndHash> lines, IInlineParser inlineParser, IRuledBlockParser blockParser, ParserContext context)
        {
            return new EmptyElement();
        }
    }

    /// <summary>
    /// 表示一个分隔线，一行内只有"---"
    /// </summary>
    public class SepBlockRule: BlockRule
    {
        public const char aLineOfChar = '-';
        public const string style = $".{SepElement.htmlClassName}{{background-color:black;height:2px;margin:10px 0px 10px 0px}}";
        public SepBlockRule() : base(style: style) { }
        public override string Apply(IHtmlable content)
        {
            return content.ToHtml();
        }

        public override string GetPureContentOf(string line)
        {
            return string.Empty;
        }

        public override bool LineMatched(string line)
        {
            if (line.Length >= 3 && line.All(c => c == aLineOfChar))
                return true;
            return false;
        }

        public override IHtmlable MakeBlockFromLines(IEnumerable<LineAndHash> lines, IInlineParser inlineParser, IRuledBlockParser blockParser, ParserContext context)
        {
            var res = new ElementCollection();
            lines.ToList().ForEach(_ => res.Add(new SepElement(this)));
            return res;
        }
        public class SepElement:BlockElement
        {
            public const string htmlClassName = "sep";
            private readonly SepBlockRule _fromRule;

            public SepElement(SepBlockRule fromRule)
            {
                _fromRule = fromRule;
            }
            public override string ToHtml()
            {
                return $"<div class=\"{htmlClassName}\"></div>";
            }
            public override void WriteHtml(StringBuilder sb)
            {
                sb.Append($"<div class=\"{htmlClassName}\"></div>");
            }
            public override List<IRule> ContainRules()
            {
                return new() { _fromRule };
            }
        }

        public override bool Equals(IBlockRule? other)
        {
            return other is SepBlockRule;
        }
        public override bool Equals(object? obj)
        {
            return Equals(obj as IBlockRule);
        }
        public override int GetHashCode()
        {
            return nameof(SepBlockRule).GetHashCode();
        }
        public override string UniqueName => "内置_分隔线";
    }
    /// <summary>
    /// 表示一个迷你表格，例子：
    /// |姓名|年龄|
    /// |Au|20|
    /// |Br|38|
    /// </summary>
    public class MiniTableBlockRule : BlockRule
    {
        public const char tableSep = '|';

        public MiniTableBlockRule() : base("<table>", "</table>") { }

        public override bool LineMatched(string line)
        {
            return line.Length>=2 && line.StartsWith(tableSep) && line.EndsWith(tableSep);
        }
        public override string GetPureContentOf(string line)
        {
            if (LineMatched(line))
                return line.Substring(1, line.Length - 2);
            return line;
        }
        public override IHtmlable MakeBlockFromLines(IEnumerable<LineAndHash> lines, IInlineParser inlineParser, IRuledBlockParser blockParser, ParserContext context)
        {
            ElementCollection rows = new();

            List<string[]> text = lines.ToList().Select(x=>x.Text).Select(x => x.Split(tableSep)).ToList();
            int width = text.Select(x => x.Length).Max();
            int headSepRowIndex = text.FindIndex(x => x.All(y => y.Length>=3 && y.All(c => c == '-')));

            var colorParser = context.Options.ColorParser;
            for(int lineIndex=0; lineIndex<text.Count; lineIndex++)
            {
                if (lineIndex == headSepRowIndex)
                    continue;
                bool isHead = lineIndex < headSepRowIndex;

                string[] line = text[lineIndex];
                ElementCollection rowCells = new();
                foreach(var cell in line)
                {
                    var (cellStr, cellAttrs) = CellColorAttr(cell, colorParser);
                    var cellContent = inlineParser.Run(cellStr);
                    rowCells.Add(new TableCellElement(cellContent, cellAttrs, isHead ));
                }
                int left = width - rowCells.Count;
                for (int i = 0; i < left; i++)
                    rowCells.Add(new TableCellElement(null, null, isHead));
                rows.Add(new TableRowElement(rowCells));
            }
            return new RuledBlockElement(rows, genByRule: this);
        }
        public override bool Equals(object? obj)
        {
            return Equals(obj as IBlockRule);
        }
        public override int GetHashCode()
        {
            return nameof(MiniTableBlockRule).GetHashCode();
        }
        public override bool Equals(IBlockRule? other)
        {
            return (other is MiniTableBlockRule);
        }
        public override string UniqueName => "内置_表格";

        public class TableRowElement : SimpleBlockElement
        {
            public TableRowElement(IHtmlable content)
                : base(content, "<tr>", "</tr>")
            {
            }
        }
        public class TableCellElement(
            IHtmlable? content,
            string? attr,
            bool isHead = false
            ) : BlockElement(content)
        {
            private string CellTagName => isHead ? "th" : "td";
            
            public override string ToHtml()
            {
                string contentHtml = Content.ToHtml();
                if (attr is not null && attr.Length > 0)
                    return $"<{CellTagName} {attr}>{contentHtml}</{CellTagName}>";
                return $"<{CellTagName}>{contentHtml}</{CellTagName}>";
            }

            public override void WriteHtml(StringBuilder sb)
            {
                sb.Append('<');
                sb.Append(CellTagName);
                if (attr is not null && attr.Length > 0)
                {
                    sb.Append(' ');
                    sb.Append(attr);
                }
                sb.Append('>');
                Content.WriteHtml(sb);
                sb.Append("</");
                sb.Append(CellTagName);
                sb.Append('>');
            }
        }
        
        public static (string s, string attrs) CellColorAttr(string s, IColorParser colorParser)
        {
            var tildeSplitted = s.Split("/-c-/", 2);
            if (tildeSplitted.Length == 2)
            {
                string sReplace = tildeSplitted[0];
                string colorStr = tildeSplitted[1];
                if (colorParser.TryParseColor(colorStr, out var c))
                {
                    string textColor;
                    var isLightColor = ColorUtil.IsLightColor(c);
                    if (isLightColor)
                        textColor = "black";
                    else
                        textColor = "white";
                    var attrs = $"style=\"background-color:{ColorUtil.ToCssColorString(c)};color:{textColor}\"";
                    return (sReplace, attrs);
                }
            }
            return (s, "");
        }
    }

    public partial class FootNoteBodyRule : BlockRule
    {
        public override string GetPureContentOf(string line)
        {
            //保留标记，否则名称信息丢失
            return line;
        }

        public override bool LineMatched(string line)
        {
            return FootBodyLineMarkRegex().IsMatch(line);
        }

        public override IHtmlable MakeBlockFromLines(IEnumerable<LineAndHash> lines, IInlineParser inlineParser, IRuledBlockParser blockParser, ParserContext context)
        {
            var resContent = lines.ToList().ConvertAll(x=> {
                var match = FootBodyLineMarkRegex().Match(x.Text);
                if(!match.Success)
                    return null;
                var head = match.Value;
                var body = x.Text.Remove(0,head.Length);
                var name = head.Trim()[2..^2];
                return new FootNoteBodyElement(name, inlineParser.Run(body, false), x.RawLineHash);
            });
            context.FootNote.AddFootNoteBodies(resContent);
            var placeholders = new List<FootNoteBodyPlaceholderElement>();
            resContent.ForEach(x =>
            {
                if(x is not null)
                    placeholders.Add(new FootNoteBodyPlaceholderElement(x,this));
            });
            return new ElementCollection(placeholders);
        }
        public override bool Equals(IBlockRule? other)
        {
            throw new NotImplementedException();
        }
        public override string UniqueName => "内置_脚注";

        [GeneratedRegex("^\\s*\\[\\^.{1,}\\][:：]{1}")]
        private static partial Regex FootBodyLineMarkRegex();
    }

    /// <summary>
    /// 一次性获取所有系统提供的特殊块规则实例
    /// </summary>
    public static class InternalBlockRules
    {
        public static List<IBlockRule> GetInstances()
        {
            return [
                new ListBlockRule(),
                new SepBlockRule(),
                new LineCommentRule(),
                new MiniTableBlockRule(),
                new PrefixBlockRule("&gt;","<div class=\"quote\">","</div>","引用"),
                new PrefixBlockRule("＞","<div class=\"quote\">","</div>","引用"),
                new PrefixBlockRule(".   ","<div style=\"text-align:center\">","</div>","中对齐"),
                new PrefixBlockRule("...   ","<div style=\"text-align:right\">","</div>","右对齐"),
                new FootNoteBodyRule()
            ];
        }
    }
}
