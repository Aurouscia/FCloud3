using FCloud3.HtmlGen.Context;
using FCloud3.HtmlGen.Models;
using FCloud3.HtmlGen.Rules;
using FCloud3.HtmlGen.Util;
using System.Text;

namespace FCloud3.HtmlGen.Mechanics
{
    public interface IInlineParser
    {
        public IHtmlable Run(string input, bool mayContainTemplateCall = true);
        public IHtmlable RunForLine(LineAndHash input);
        public IHtmlable SplitByMarks(string input, InlineMarkList marks);
    }
    public class InlineParser:IInlineParser
    {
        private readonly ParserContext _ctx;
        private readonly Lazy<TemplateParser> _templateParser;
        private readonly bool _useCache;

        public InlineParser(ParserContext ctx) 
        {
            _ctx = ctx;
            _templateParser = new(() => new(ctx));
            _useCache = ctx.Options.CacheOptions.UseCache;
        }

        public IHtmlable Run(string input,bool mayContainTemplateCall = true)
        {
            if (_ctx.FrameCountCheck() is IHtmlable err)
                return err;
            if (input.Length <= 5)
                mayContainTemplateCall = false;
            if (_useCache && !mayContainTemplateCall)
            {
                var cache = _ctx.Caches.ReadParsedElement(input);
                if (cache is not null)
                    return cache;
            }
            try
            {
                IHtmlable? res = null;
                if (mayContainTemplateCall)
                {
                     res = _templateParser.Value.Run(input);
                }
                if (res is null)
                {
                    var marks = MakeMarks(input);
                    if (marks.Count == 0)
                    {
                        res = new TextElement(input);
                    }
                    else
                    {
                        marks.ForEach(x =>
                        {
                            _ctx.RuleUsage.ReportUsage(x.Rule);
                        });
                        res = SplitByMarks(input, marks);
                    }
                }
                if (_useCache && !mayContainTemplateCall)
                    res = _ctx.Caches.SaveParsedElement(input, res);
                return res;
            }
            catch(Exception ex)
            {
                return new ErrorElement($"{ex.Message}");
            }
        }

        public IHtmlable RunForLine(LineAndHash line)
        {
            IHtmlable lineContent = Run(line.Text);
            LineElement res = new(lineContent,line.RawLineHash);
            return res;
        }

        public InlineMarkList MakeMarks(string input)
        {
            int pointer = 0;
            InlineMarkList res = new();
            foreach(var r in _ctx.Options.InlineParsingOptions.InlineRules)
            {
                //对于每个行内规则
                //如果是一次性规则，而且之前用过，那就直接跳过
                if (r.IsSingleUse && _ctx.RuleUsage.RuleUsedTime(r) > 0)
                    continue;

                pointer = 0;
                while (true)
                {
                    //一次性标记字符串中所有该规则

                    bool noMoreForThisRule = false;
                    int left;
                    while(true){
                        //试图找到最靠左的左规则
                        left = input.IndexOf(r.MarkLeft, pointer);
                        if (left == -1 || left >= input.Length - 1)
                        {
                            noMoreForThisRule = true;//规则不存在或者已经被找完了，可以结束本规则的搜索了
                            break;
                        }
                        //检查该左规则是否被escape
                        if (left > 0 && input[left - 1] == '\\')
                            pointer = left + 1;
                        //检查该位置是否被占
                        else if (res.Any(x => x.OccupiedAt(left)))
                            pointer = left + 1;//如果被占了，那就从下一个开始找
                        else
                            break;//如果没被escape，也没被占用，那left就是正确的左标记索引
                        if (pointer >= input.Length - 1)
                        {
                            noMoreForThisRule = true;
                            break;
                        }
                    }
                    if (noMoreForThisRule)
                        break;

                    pointer = left + r.MarkLeft.Length;
                    int right = pointer;
                    if (r.MarkRight.Length != 0)
                    {
                        while (true)
                        {
                            right = input.IndexOf(r.MarkRight, pointer);
                            if (right == -1)
                            {
                                noMoreForThisRule = true;
                                break;
                            }
                            //检查该右规则是否被escape
                            if (input[right - 1] == '\\')
                                pointer = left + 1;
                            //检查该位置是否被占
                            else if (res.Any(x => x.OccupiedAt(right)))
                                pointer = right + 1;//如果被占了，那就从下一个开始找
                            else
                                break;//如果没被占用，那right就是正确的右标记索引
                            if (pointer >= input.Length - 1)
                            {
                                noMoreForThisRule = true;
                                break;
                            }
                        }
                        if (noMoreForThisRule)
                            break;
                    }
                    InlineMark m = new(r, left, right);
                    string span = input.Substring(m.LeftIndex + m.LeftMarkLength, m.ContentLength);
                    if (r.FulFill(span))
                    {
                        res.Add(m);
                        //如果规则是一次性的，那就不找了
                        if (r.IsSingleUse)
                            break;
                    }

                    pointer = right + 1;
                    if (pointer >= input.Length - 1)
                        break;
                }
            }
            return res;
        }

        /// <summary>
        /// 将输入的字符串根据规则标记拆成元素
        /// </summary>
        /// <param name="input">输入字符串</param>
        /// <param name="marks">标记（内部索引由input的开头作为0开始）</param>
        /// <returns></returns>
        public IHtmlable SplitByMarks(string input, InlineMarkList marks)
        {
            //对于输入的标记，试图找到最靠左侧的，且没有越界的标记
            var first = marks
                .Where(x=>x.LeftIndex >= 0 && x.RightIndex<= input.Length)
                .MinBy(x => x.LeftIndex);

            if (first is null) {
                var text = new TextElement(input);
                return text;
            }
            marks.Remove(first);
            ElementCollection res = new();

            int middleStartIndex = first.LeftIndex + first.LeftMarkLength;
            int rightStartIndex = first.LeftIndex  + first.TotalLength;
            string left = input.Substring(0, first.LeftIndex);
            string middle = input.Substring(middleStartIndex, first.ContentLength);
            string right = input.Substring(rightStartIndex);

            if (!string.IsNullOrEmpty(left))
                res.Add(new TextElement(left));
            var middleSplitted = first.Rule.MakeElementFromSpan(middle, new(marks,middleStartIndex), this);
            res.AddFlat(middleSplitted);

            var rightSplitted = SplitByMarks(right,new(marks,rightStartIndex));
            res.AddFlat(rightSplitted);
            return res;
        }

        public class LineElement : SimpleBlockElement
        {
            private readonly string? _rawLineHash;
            public LineElement(IHtmlable content, string? rawLineHash)
                : base(content, "<p>", "</p>")
            {
                _rawLineHash = rawLineHash;
            }
            public override string ToHtml()
            {
                if(_rawLineHash is null)
                    return base.ToHtml();
                return HtmlLabel.Custom(Content.ToHtml(),"p", Consts.locatorAttrName, _rawLineHash);
            }
            public override void WriteHtml(StringBuilder sb)
            {
                if (_rawLineHash is null)
                    base.WriteHtml(sb);
                else
                    HtmlLabel.CustomWrite(sb, Content, "p", Consts.locatorAttrName, _rawLineHash);
            }
        }
    }
    public class InlineMark
    {
        public int LeftIndex { get; }
        public int RightIndex { get; }
        public int LeftMarkLength => Rule.MarkLeft.Length;
        public int RightMarkLength => Rule.MarkRight.Length;
        public int ContentLength => RightIndex - LeftIndex - LeftMarkLength;
        public int TotalLength => ContentLength + LeftMarkLength + RightMarkLength;
        public IInlineRule Rule { get; }
        public InlineMark(IInlineRule rule, int leftIndex, int rightIndex)
        {
            Rule = rule;
            LeftIndex = leftIndex;
            RightIndex = rightIndex;
        }
        public bool OccupiedAt(int index)
        {
            return (index >= LeftIndex && index < LeftIndex + Rule.MarkLeft.Length)
                || (index >= RightIndex && index < RightIndex + Rule.MarkRight.Length);
        }
        public InlineMark(InlineMark original,int offset)
        {
            Rule = original.Rule;
            LeftIndex = original.LeftIndex - offset;
            RightIndex = original.RightIndex - offset;
        }
    }
    public class InlineMarkList : List<InlineMark>
    {
        public InlineMarkList() { }
        public InlineMarkList(InlineMarkList original,int offset)
        {
            var res = original.ConvertAll(x => new InlineMark(x, offset));
            this.AddRange(res);
        }
    }
}
