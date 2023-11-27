using FCloud3.HtmlGen.Models;
using FCloud3.HtmlGen.Options;
using FCloud3.HtmlGen.Rules;
using FCloud3.HtmlGen.Util;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;

namespace FCloud3.HtmlGen.Mechanics
{
    public class BlockParser
    {
        private readonly HtmlGenOptions _options;
        private readonly TitledBlockParser _titledBlockParser;
        private readonly Lazy<InlineParser> _inlineParser;

        public BlockParser(HtmlGenOptions options)
        {
            _titledBlockParser = new(options);
            _inlineParser = new(()=>new(options));
            _options = options;
        }

        public ElementCollection Run(string input, bool enforceBlock = true)
        {
            var lines = LineSplitter.Split(input);

            if (lines.Count == 0)
                return new();
            if (lines.Count == 1)
            {
                if (enforceBlock)
                    return _inlineParser.Value.RunForLine(lines[0]);
                else
                    return _inlineParser.Value.Run(lines[0]);
            }

            return _titledBlockParser.Run(lines);
        }
    }

    public class TitledBlockParser
    {
        private readonly HtmlGenOptions _options;
        private readonly RuledBlockParser _ruledBlockParser;
        private readonly InlineParser _inlineParser;

        public TitledBlockParser(HtmlGenOptions options)
        {
            _options = options;
            _ruledBlockParser = new RuledBlockParser(options);
            _inlineParser = new InlineParser(options);
        }
        public ElementCollection Run(List<string> inputLines)
        {
            List<LineWithTitleLevel> lines = inputLines.ConvertAll(x => new LineWithTitleLevel(x));
            return Run(lines);
        }
        private ElementCollection Run(List<LineWithTitleLevel> lines)
        {
            //为每一行标记其标题（如果有）的等级
            if (lines.All(x => x.Level == 0))
            {
                ElementCollection ruled = _ruledBlockParser.Run(lines.Select(x => x.PureContent).ToList());
                return ruled;
            }
            //从标题等级最高（数字最小）开始找
            int targetLevel = lines.Where(x => x.Level != 0).Select(x => x.Level).Min();

            ElementCollection res = new();

            List<LineWithTitleLevel> generating = new();
            string? title = null;
            foreach (var l in lines)
            {
                //遇到目标等级标题的行
                if (targetLevel == l.Level)
                {
                    //将其之前的部分纳入上一个同级标题麾下
                    if (!string.IsNullOrEmpty(title))
                    {
                        ElementCollection generated = Run(generating);
                        ElementCollection titleParsed = _inlineParser.Run(title);
                        TitledBlockElement titledBlock = new(titleParsed, targetLevel, generated);
                        res.Add(titledBlock);
                    }
                    else
                    {
                        ElementCollection ruled = _ruledBlockParser.Run(generating.Select(x => x.PureContent).ToList());
                        res.AddRange(ruled);
                    }
                    generating.Clear();
                    title = l.PureContent;
                }
                else
                {
                    generating.Add(l);
                }
            }
            if (!string.IsNullOrEmpty(title))
            {
                ElementCollection generated = Run(generating);
                ElementCollection titleParsed = _inlineParser.Run(title);
                TitledBlockElement titledBlock = new(titleParsed, targetLevel, generated);
                res.Add(titledBlock);
            }
            else
            {
                ElementCollection ruled = _ruledBlockParser.Run(generating.Select(x => x.PureContent).ToList());
                res.AddRange(ruled);
            }
            return res;
        }

        public class LineWithTitleLevel
        {
            /// <summary>
            /// 标题等级（有几个井号）
            /// </summary>
            public int Level { get; }
            /// <summary>
            /// 纯内容（如果有开头井号就去掉，没有就是原字符串）
            /// </summary>
            public string PureContent { get; }
            public LineWithTitleLevel(string line)
            {
                Level = 0;
                foreach(var t in TitleMark.OrderedTitleMarks)
                {
                    if (line.StartsWith(t.Value))
                    {
                        string pureContent = line[t.Value.Length..].Trim();
                        if (pureContent.Contains(Consts.titleLevelMark))
                            break;
                        else
                        {
                            this.Level = t.Key;
                            this.PureContent = pureContent;
                            return;
                        }
                    }
                }
                PureContent = line;
            }
        }
        public static class TitleMark
        {
            private static List<KeyValuePair<int, string>>? titleMarks;
            public static List<KeyValuePair<int, string>> OrderedTitleMarks
            {
                get
                {
                    if (titleMarks is not null)
                        return titleMarks;
                    titleMarks = new();
                    StringBuilder titleTemp = new();
                    for (int i = 1; i < 7; i++)
                    {
                        titleTemp.Append(Consts.titleLevelMark);
                        titleMarks.Add(new(i, titleTemp.ToString()));
                    }
                    titleMarks.Reverse();
                    return titleMarks;
                }
            }
        }
    }

    public interface IRuledBlockParser
    {
        public ElementCollection Run(List<string> inputLines);
    }
    public class RuledBlockParser:IRuledBlockParser
    {
        private readonly HtmlGenOptions _options;
        private readonly InlineParser _inlineParser;
        public RuledBlockParser(HtmlGenOptions options)
        {
            _options = options;
            _inlineParser = new(options);
        }
        public ElementCollection Run(List<string> inputLines)
        {
            if (inputLines.Count == 0)
                return new();
            List<LineWithRule> lines = inputLines.ConvertAll(x => new LineWithRule(x,_options.BlockParsingOptions.BlockRules));
            return Run(lines);
        }
        private ElementCollection Run(List<LineWithRule> lines)
        {
            ElementCollection res = new();
            if (lines.All(x => x.Rule is null))
            {
                //每一行都没有块规则调用
                foreach (var line in lines)
                {
                    res.Add(_inlineParser.RunForLine(line.PureContent));
                }
                return res;
            }
            lines.ForEach(x =>
            {
                if (x.Rule is not null)
                    _options.ReportUsage(x.Rule);
            });

            var emptyRule = new HtmlEmptyBlockRule();
            IHtmlBlockRule tracking = emptyRule;
            List<LineWithRule> generating = new();
            foreach(var l in lines)
            {
                var ruleOfthisLine = l.Rule ?? emptyRule;
                if (ruleOfthisLine != tracking)
                {
                    if (generating.Count > 0)
                    {
                        var pureLines = generating.Select(x => x.PureContent).ToList();
                        RuledBlockElement element = tracking.MakeBlockFromLines(pureLines,_inlineParser,this);
                        res.Add(element);
                        generating.Clear();
                    }
                    tracking = ruleOfthisLine;
                }
                generating.Add(l);
            }
            if (generating.Count > 0)
            {
                var pureLines = generating.Select(x => x.PureContent).ToList();
                RuledBlockElement element = tracking.MakeBlockFromLines(pureLines, _inlineParser, this);
                res.Add(element);
            }
            return res;
        }
        public class LineWithRule
        {
            public IHtmlBlockRule? Rule { get; }
            public string PureContent { get; }
            public LineWithRule(string line,List<IHtmlBlockRule> allRules)
            {
                this.Rule = allRules.FirstOrDefault(t => t.LineMatched(line));
                if (Rule is null)
                    PureContent = line;
                else
                    PureContent = Rule.GetPureContentOf(line);
            }
        }
    }
}
