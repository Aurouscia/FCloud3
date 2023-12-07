using FCloud3.HtmlGen.Models;
using FCloud3.HtmlGen.Options;
using FCloud3.HtmlGen.Rules;
using FCloud3.HtmlGen.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static FCloud3.HtmlGen.Rules.SepBlockRule;

namespace FCloud3.HtmlGen.Mechanics
{
    public interface IInlineParser
    {
        public IHtmlable Run(string input, bool mayContainTemplateCall = true);
        public IHtmlable RunForLine(string input);
        public IHtmlable SplitByMarks(string input, InlineMarkList marks);
    }
    public class InlineParser:IInlineParser
    {
        private readonly ParserContext _ctx;
        private readonly Lazy<TemplateParser> _templateParser;
        private readonly bool _useExclusiveCache;
        private readonly bool _useInclusiveCache;

        public InlineParser(ParserContext ctx) 
        {
            _ctx = ctx;
            _templateParser = new(() => new(ctx));
            _useExclusiveCache = ctx.Options.CacheOptions.UseExclusiveCache;
            _useInclusiveCache = ctx.Options.CacheOptions.UseInclusiveCache;
        }

        public IHtmlable Run(string input,bool mayContainTemplateCall = true)
        {
            if (_useInclusiveCache)
            {
                var res = _ctx.Caches.ReadParseResult(input);
                if (res is not null)
                {
                    _ctx.RuleUsage.ReportUsage(res.UsedRules);
                    return new CachedElement(res.Content,res.UsedRules);
                }
            }
            if (_useExclusiveCache)
            {
                if (_ctx.Caches.IsNonMarkString(input)) 
                {
                    return new TextElement(input);
                }
            }
            try
            {
                IHtmlable? res = null;
                if (mayContainTemplateCall)
                {
                    if (_useExclusiveCache && _ctx.Caches.IsNonTemplateStr(input))
                    {
                        //无需送去templateParser
                    }
                    else
                        res = _templateParser.Value.Run(input);
                }
                if (res is null)
                {
                    var marks = MakeMarks(input);
                    if (marks.Count == 0)
                    {
                        if (_useExclusiveCache)
                            _ctx.Caches.ReportNonMarkString(input);
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
                if (_useInclusiveCache)
                {
                    string content = res.ToHtml();
                    List<IRule> usedRules = res.ContainRules() ?? new();
                    _ctx.Caches.SaveParseResult(input, content, usedRules);
                    return new CachedElement(content,usedRules);
                }
                return res;
            }
            catch(Exception ex)
            {
                return new ErrorElement($"{ex.Message}");
            }
        }

        public IHtmlable RunForLine(string input)
        {
            IHtmlable lineContent = Run(input);
            LineElement line = new(lineContent);
            return line;
        }

        public InlineMarkList MakeMarks(string input)
        {
            if (_useExclusiveCache)
            {
                InlineMarkList? cache = _ctx.Caches.GetInlineMarks(input);
                if (cache is not null)
                    return cache;
            }

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
                    int right;
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

            if (_useExclusiveCache && res.Count > 0)
            {
                _ctx.Caches.SetInlineMarks(input, res);
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
            public LineElement(IHtmlable content)
                : base(content, "<p>", "</p>")
            {
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
