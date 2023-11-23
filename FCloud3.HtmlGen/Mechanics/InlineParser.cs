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
    public class InlineParser
    {
        private readonly HtmlGenOptions _options;
        private readonly Lazy<TemplateParser> _templateParser;
        private List<InlineMark> _marks;
        public InlineParser(HtmlGenOptions options) 
        {
            _options = options;
            _templateParser = new(() => new(options));
            _marks = new();
        }

        public ElementCollection Run(string input,bool mayContainTemplateCall = true)
        {
            try
            {
                if (mayContainTemplateCall)
                    return _templateParser.Value.Run(input);
                _marks = MakeMarks(input);
                return SplitByMarks(input);
            }
            catch(Exception ex)
            {
                return new(new ErrorElement($"行内解析出错:{ex.Message}"));
            }
        }

        public LineElement RunForLine(string input)
        {
            ElementCollection lineContent = Run(input);
            LineElement line = new(lineContent);
            return line;
        }

        public List<InlineMark> MakeMarks(string input)
        {
            int pointer = 0;
            List<InlineMark> res = new();
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
                    res.Add(m);

                    pointer = right + 1;
                    if (pointer >= input.Length - 1)
                        break;
                }
            }
            return res;
        }

        public ElementCollectionWithStyle SplitByMarks(string input, int offset = 0, HtmlInlineRule? rule = null)
        {
            var res = new ElementCollectionWithStyle(rule);
            var first = _marks
                .Where(x=>x.LeftIndex-offset >= 0 && x.RightIndex-offset<= input.Length)
                .MinBy(x => x.LeftIndex);
            if (first is null) 
            {
                res.Add(new TextElement(input));
                return res;
            }
            int middleStartIndex = first.LeftIndex - offset + first.LeftMarkLength;
            int rightStartIndex = first.LeftIndex - offset + first.TotalLength;
            string left = input.Substring(0, first.LeftIndex - offset);
            string middle = input.Substring(middleStartIndex, first.ContentLength);
            string right = input.Substring(rightStartIndex);

            if (!string.IsNullOrEmpty(left))
                res.Add(new TextElement(left));
            var middleSplitted = SplitByMarks(middle, middleStartIndex,first.Rule);
            res.Add(middleSplitted);
            var rightSplitted = SplitByMarks(right, rightStartIndex);
            res.AddRange(rightSplitted.Children);
            return res;
        }

        public class InlineMark
        {
            public int LeftIndex { get; }
            public int RightIndex { get; }
            public int LeftMarkLength { get; }
            public int RightMarkLength { get; }
            public int ContentLength { get; }
            public int TotalLength { get; }
            public HtmlInlineRule Rule { get; }
            public InlineMark(HtmlInlineRule rule,int leftIndex, int rightIndex)
            {
                Rule = rule;
                LeftIndex = leftIndex;
                RightIndex = rightIndex;
                LeftMarkLength = rule.MarkLeft.Length;
                RightMarkLength = rule.MarkRight.Length;
                ContentLength = RightIndex - LeftIndex - LeftMarkLength;
                TotalLength = ContentLength + LeftMarkLength + RightMarkLength;
            }
            public bool OccupiedAt(int index)
            {
                return (index >= LeftIndex && index < LeftIndex + Rule.MarkLeft.Length)
                    || (index >= RightIndex && index < RightIndex + Rule.MarkRight.Length);
            }
        }
    }
}
