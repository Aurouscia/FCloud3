﻿using FCloud3.HtmlGen.Models;
using FCloud3.HtmlGen.Options;
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
        private readonly TypedBlockParser _typedParser;
        public TitledBlockParser(HtmlGenOptions options)
        {
            _options = options;
            _typedParser = new TypedBlockParser(options);
        }
        public ElementCollection Run(List<string> inputLines)
        {
            List<LineWithTitleLevel> lines = inputLines.ConvertAll(x => new LineWithTitleLevel(x));
            return Run(lines);
        }
        private ElementCollection Run(List<LineWithTitleLevel> lines)
        {
            if (lines.All(x => x.Level == 0))
            {
                ElementCollection typed = _typedParser.Run(lines.Select(x => x.PureContent).ToList());
                return typed;
            }
            int targetLevel = lines.Where(x => x.Level != 0).Select(x => x.Level).Min();

            ElementCollection res = new();

            List<LineWithTitleLevel> generating = new();
            string? title = null;
            foreach (var l in lines)
            {
                if (targetLevel == l.Level)
                {
                    if (title is not null)
                    {
                        ElementCollection generated = Run(generating);
                        TitledBlockElement titledBlock = new(title, targetLevel, generated);
                        res.Add(titledBlock);
                    }
                    else
                    {
                        ElementCollection typed = _typedParser.Run(generating.Select(x => x.PureContent).ToList());
                        res.AddRange(typed);
                    }
                    generating.Clear();
                    title = l.PureContent;
                }
                else
                {
                    generating.Add(l);
                }
            }
            if (title is not null)
            {
                ElementCollection generated = Run(generating);
                TitledBlockElement titledBlock = new(title, targetLevel, generated);
                res.Add(titledBlock);
            }
            else
            {
                ElementCollection typed = _typedParser.Run(generating.Select(x => x.PureContent).ToList());
                res.AddRange(typed);
            }
            return res;
        }

        public class LineWithTitleLevel
        {
            public int Level { get; }
            public string PureContent { get; }
            public LineWithTitleLevel(string line)
            {
                Level = 0;
                PureContent = string.Empty;
                foreach(var t in TitleMark.OrderedTitleMarks)
                {
                    if (line.StartsWith(t.Value))
                    {
                        this.Level = t.Key;
                        this.PureContent = line.Replace(t.Value, string.Empty).Trim();
                        break;
                    }
                }
                if(Level==0)
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

    public interface ITypedBlockParser
    {
        public ElementCollection Run(List<string> inputLines);
    }
    public class TypedBlockParser:ITypedBlockParser
    {
        private readonly HtmlGenOptions _options;
        private readonly InlineParser _inlineParser;
        public TypedBlockParser(HtmlGenOptions options)
        {
            _options = options;
            _inlineParser = new(options);
        }
        public ElementCollection Run(List<string> inputLines)
        {
            if (inputLines.Count == 0)
                return new();
            List<LineWithType> lines = inputLines.ConvertAll(x => new LineWithType(x,_options.TypedBlockRules));
            return Run(lines);
        }
        private ElementCollection Run(List<LineWithType> lines)
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

            var emptyRule = new HtmlEmptyBlockRule();
            IHtmlBlockRule tracking = emptyRule;
            List<LineWithType> generating = new();
            foreach(var l in lines)
            {
                var ruleOfthisLine = l.Rule ?? emptyRule;
                if (ruleOfthisLine != tracking)
                {
                    if (generating.Count > 0)
                    {
                        var pureLines = generating.Select(x => x.PureContent).ToList();
                        TypedBlockElement element = tracking.MakeBlockFromLines(pureLines,_inlineParser,this);
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
                TypedBlockElement element = tracking.MakeBlockFromLines(pureLines, _inlineParser, this);
                res.Add(element);
            }
            return res;
        }
        public class LineWithType
        {
            public IHtmlBlockRule? Rule { get; }
            public string PureContent { get; }
            public LineWithType(string line,List<IHtmlBlockRule> allTypes)
            {
                this.Rule = allTypes.FirstOrDefault(t => t.LineMatched(line));
                if (Rule is null)
                    PureContent = line;
                else
                    PureContent = Rule.GetPureContentOf(line);
            }
        }
    }
}