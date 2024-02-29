using FCloud3.HtmlGen.Models;
using FCloud3.HtmlGen.Options;
using FCloud3.HtmlGen.Rules;
using FCloud3.HtmlGen.Util;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;

namespace FCloud3.HtmlGen.Mechanics
{
    public interface IBlockParser
    {
        public IHtmlable Run(string input, bool enforceBlock = true);
    }
    public class BlockParser:IBlockParser
    {
        private readonly Lazy<TitledBlockParser> _titledBlockParser;
        private readonly Lazy<InlineParser> _inlineParser;
        private readonly ParserContext _ctx;
        private readonly bool _useCache;

        public BlockParser(ParserContext ctx)
        {
            _titledBlockParser = new(()=>new(ctx));
            _inlineParser = new(()=>new(ctx));
            _useCache = ctx.Options.CacheOptions.UseCache;
            _ctx = ctx;
        }

        public IHtmlable Run(string input, bool enforceBlock = true)
        {
            if (_useCache)
            {
                var res = _ctx.Caches.ReadParseResult(input);
                if (res is not null)
                {
                    _ctx.RuleUsage.ReportUsage(res.UsedRules);
                    return new CachedElement(res.Content,res.UsedRules);
                }
            }
            
            var lines = LineSplitter.Split(input,_ctx.Options.LocatorHash);
            //在此处已经对每行进行了HtmlEncode，Hash值为encode前的Hash值

            if (lines.Count == 0)
                return new EmptyElement();

            IHtmlable resElement;
            if (lines.Count == 1)
            {
                if (enforceBlock)
                    resElement = _inlineParser.Value.RunForLine(lines[0]);
                else
                    resElement = _inlineParser.Value.Run(lines[0].Text);
            }
            else
                resElement = _titledBlockParser.Value.Run(lines);

            if (_useCache)
            {
                string content = resElement.ToHtml();
                List<IRule> usedRules = resElement.ContainRules() ?? new();
                _ctx.Caches.SaveParseResult(input, content, usedRules);
                return new CachedElement(content,usedRules);
            }
            return resElement;
        }
    }

    public class TitledBlockParser
    {
        private readonly ParserContext _ctx;
        private readonly Lazy<RuledBlockParser> _ruledBlockParser;
        private readonly Lazy<InlineParser> _inlineParser;

        public TitledBlockParser(ParserContext ctx)
        {
            _ctx = ctx;
            _ruledBlockParser = new(()=> new RuledBlockParser(ctx));
            _inlineParser = new(()=>new InlineParser(ctx));
        }
        public IHtmlable Run(List<LineAndHash> inputLines)
        {
            List<LineWithTitleLevel> lines = inputLines.ConvertAll(x => new LineWithTitleLevel(x));
            return Run(lines);
        }
        private IHtmlable Run(List<LineWithTitleLevel> lines)
        {
            //为每一行标记其标题（如果有）的等级
            if (lines.All(x => x.Level == 0))
            {
                IHtmlable ruled = _ruledBlockParser.Value.Run(lines.Select(x => x.PureContent).ToList());
                return ruled;
            }
            //从标题等级最高（数字最小）开始找
            int targetLevel = lines.Where(x => x.Level != 0).Select(x => x.Level).Min();
            int titleLevelOffset = _ctx.Options.BlockParsingOptions.TitleLevelOffset;

            ElementCollection res = new();

            List<LineWithTitleLevel> generating = new();
            LineAndHash? title = null;
            foreach (var l in lines)
            {
                //遇到目标等级标题的行
                if (targetLevel == l.Level)
                {
                    //将其之前的部分纳入上一个同级标题麾下
                    if (title is not null && !string.IsNullOrEmpty(title.Text))
                    {
                        IHtmlable generated = Run(generating);
                        IHtmlable titleParsed = _inlineParser.Value.Run(title.Text);
                        TitledBlockElement titledBlock = new(titleParsed, title.RawLineHash, targetLevel+titleLevelOffset, generated);
                        res.Add(titledBlock);
                    }
                    else
                    {
                        IHtmlable ruled = _ruledBlockParser.Value.Run(generating.Select(x => x.PureContent).ToList());
                        res.AddFlat(ruled);
                    }
                    generating.Clear();
                    title = l.PureContent;
                }
                else
                {
                    generating.Add(l);
                }
            }
            if (title is not null && !string.IsNullOrEmpty(title.Text))
            {
                IHtmlable generated = Run(generating);
                IHtmlable titleParsed = _inlineParser.Value.Run(title.Text);
                TitledBlockElement titledBlock = new(titleParsed,title.RawLineHash, targetLevel+titleLevelOffset, generated);
                res.Add(titledBlock);
            }
            else
            {
                IHtmlable ruled = _ruledBlockParser.Value.Run(generating.Select(x => x.PureContent).ToList());
                res.AddFlat(ruled);
            }
            return res.Simplify();
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
            public LineAndHash PureContent { get; }
            public LineWithTitleLevel(LineAndHash line)
            {
                PureContent = line;
                string lineStr = line.Text;
                Level = 0;
                if (!lineStr.StartsWith(Consts.titleLevelMark))
                    return;
                foreach(var t in TitleMark.OrderedTitleMarks)
                {
                    if (lineStr.StartsWith(t.Value))
                    {
                        string pureContent = lineStr[t.Value.Length..].Trim();
                        if (pureContent.Contains(Consts.titleLevelMark))
                            break;
                        else
                        {
                            this.Level = t.Key;
                            this.PureContent.Text = pureContent;
                            return;
                        }
                    }
                }
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
                    titleMarks = new(6);
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
        public IHtmlable Run(List<LineAndHash> inputLines);
    }
    public class RuledBlockParser:IRuledBlockParser
    {
        private readonly ParserContext _ctx;
        private readonly Lazy<InlineParser> _inlineParser;
        public RuledBlockParser(ParserContext ctx)
        {
            _ctx = ctx;
            _inlineParser = new(()=>new(ctx));
        }
        public IHtmlable Run(List<LineAndHash> inputLines)
        {
            if (inputLines.Count == 0)
                return new EmptyElement();
            List<LineWithRule> lines = inputLines.ConvertAll(x => new LineWithRule(x,_ctx.Options.BlockParsingOptions.BlockRules));
            return Run(lines);
        }
        private IHtmlable Run(List<LineWithRule> lines)
        {
            ElementCollection res = new();
            if (lines.All(x => x.Rule is null))
            {
                //每一行都没有块规则调用
                foreach (var line in lines)
                {
                    res.Add(_inlineParser.Value.RunForLine(line.PureContent));
                }
                return res.Simplify();
            }
            lines.ForEach(x =>
            {
                if (x.Rule is not null)
                    _ctx.RuleUsage.ReportUsage(x.Rule);
            });

            var emptyRule = new EmptyBlockRule();
            IBlockRule tracking = emptyRule;
            List<LineWithRule> generating = new();
            foreach(var l in lines)
            {
                var ruleOfthisLine = l.Rule ?? emptyRule;
                if (ruleOfthisLine != tracking)
                {
                    if (generating.Count > 0)
                    {
                        var pureLines = generating.Select(x => x.PureContent).ToList();
                        IHtmlable htmlable = tracking.MakeBlockFromLines(pureLines,_inlineParser.Value,this);
                        res.AddFlat(htmlable);
                        generating.Clear();
                    }
                    tracking = ruleOfthisLine;
                }
                generating.Add(l);
            }
            if (generating.Count > 0)
            {
                var pureLines = generating.Select(x => x.PureContent).ToList();
                IHtmlable htmlable = tracking.MakeBlockFromLines(pureLines, _inlineParser.Value, this);
                res.AddFlat(htmlable);
            }
            return res.Simplify();
        }
        public class LineWithRule
        {
            public IBlockRule? Rule { get; }
            public LineAndHash PureContent { get; }
            public LineWithRule(LineAndHash line,List<IBlockRule> allRules)
            {
                PureContent = line;
                this.Rule = allRules.FirstOrDefault(t => t.LineMatched(line.Text));
                if (Rule is not null)
                    PureContent.Text = Rule.GetPureContentOf(line.Text);
            }
        }
    }
}
