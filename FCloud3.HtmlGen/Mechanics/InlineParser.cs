using FCloud3.HtmlGen.Models;
using FCloud3.HtmlGen.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FCloud3.HtmlGen.Mechanics
{
    public interface IInlineParser
    {
        public ElementCollection Run(string input, bool mayContainTemplateCall = true);
        public BlockElement RunForLine(string input);
        public InlineElement SplitByMarks(string input, InlineMarkList marks, IHtmlInlineRule? rule = null);
    }
    public class InlineParser:IInlineParser
    {
        private readonly HtmlGenOptions _options;
        private readonly Lazy<TemplateParser> _templateParser;
        public InlineParser(HtmlGenOptions options) 
        {
            _options = options;
            _templateParser = new(() => new(options));
        }

        public ElementCollection Run(string input,bool mayContainTemplateCall = true)
        {
            try
            {
                if (mayContainTemplateCall)
                    return _templateParser.Value.Run(input);
                var marks = MakeMarks(input);
                if (marks.Count == 0)
                    return new TextElement(input);
                return SplitByMarks(input,marks);
            }
            catch(Exception ex)
            {
                return new(new ErrorElement($"行内解析出错:{ex.Message}"));
            }
        }

        public BlockElement RunForLine(string input)
        {
            ElementCollection lineContent = Run(input);
            LineElement line = new(lineContent);
            return line;
        }

        public InlineMarkList MakeMarks(string input)
        {
            int pointer = 0;
            InlineMarkList res = new();
            foreach(var r in _options.InlineRules)
            {
                //对于每个行内规则
                if (!input.Contains(r.MarkLeft) || !input.Contains(r.MarkRight))
                    continue ;

                pointer = 0;
                while (true)
                {
                    //一次性标记字符串中所有该规则

                    bool noMoreForThisRule = false;
                    int left;
                    while(true){
                        left = input.IndexOf(r.MarkLeft, pointer);
                        if (left == -1 || left >= input.Length - 1)
                        {
                            noMoreForThisRule = true;//规则不存在或者已经被找完了，可以结束本规则的搜索了
                            break;
                        }
                        //检查该位置是否被占
                        if (res.Any(x => x.OccupiedAt(left)))
                            pointer = left + 1;//如果被占了，那就从下一个开始找
                        else
                            break;//如果没被占用，那left就是正确的左标记索引
                        if (pointer >= input.Length - 1)
                        {
                            noMoreForThisRule = true;
                            break;
                        }
                    }
                    if (noMoreForThisRule)
                        break;

                    pointer = left + 1;
                    int right;
                    while (true)
                    {
                        right = input.IndexOf(r.MarkRight, pointer);
                        if (right == -1)
                        {
                            noMoreForThisRule = true;
                            break;
                        }
                        //检查该位置是否被占
                        if (res.Any(x => x.OccupiedAt(right)))
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
                    if(r.FulFill(span))
                        res.Add(m);

                    pointer = right + 1;
                    if (pointer >= input.Length - 1)
                        break;
                }
            }
            return res;
        }

        public InlineElement SplitByMarks(string input, InlineMarkList marks, IHtmlInlineRule? rule = null)
        {
            var first = marks
                .Where(x=>x.LeftIndex >= 0 && x.RightIndex<= input.Length)
                .MinBy(x => x.LeftIndex);

            var res = new RuledInlineElement(rule);
            if (first is null) {
                var text = new TextElement(input);
                if (rule is null)
                    return text;
                res.Add(text);
                return res;
            }

            int middleStartIndex = first.LeftIndex + first.LeftMarkLength;
            int rightStartIndex = first.LeftIndex  + first.TotalLength;
            string left = input.Substring(0, first.LeftIndex);
            string middle = input.Substring(middleStartIndex, first.ContentLength);
            string right = input.Substring(rightStartIndex);

            if (!string.IsNullOrEmpty(left))
                res.Add(new TextElement(left));
            var middleSplitted = first.Rule.MakeElementFromSpan(middle, new(marks,middleStartIndex), this);
            res.Add(middleSplitted);

            var rightSplitted = SplitByMarks(right,new(marks,rightStartIndex));
            if (rightSplitted is RuledInlineElement ruled)
                res.AddRange(ruled.Content);
            else
                res.Add(rightSplitted);
            return res;
        }

        public class LineElement : SimpleBlockElement
        {
            public LineElement(ElementCollection content)
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
        public IHtmlInlineRule Rule { get; }
        public InlineMark(IHtmlInlineRule rule, int leftIndex, int rightIndex)
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
