﻿using FCloud3.WikiPreprocessor.Context;
using FCloud3.WikiPreprocessor.Models;
using FCloud3.WikiPreprocessor.Rules;
using FCloud3.WikiPreprocessor.Util;

namespace FCloud3.WikiPreprocessor.Mechanics
{
    public class TemplateParser
    {
        private readonly ParserContext _ctx;
        private readonly Lazy<BlockParser> _blockParser;
        private readonly Lazy<InlineParser> _inlineParser;
        private readonly TemplateSlotInfo _slotInfo;

        public TemplateParser(ParserContext ctx)
        {
            //可能造成stackOverflow，解决办法：都改成Lazy的
            _ctx = ctx;
            _blockParser = new(()=>new(ctx));
            _inlineParser = new(()=>new(ctx));
            _slotInfo = ctx.TemplateSlotInfo;
        }


        public IHtmlable Run(string input)
        {
            if (_ctx.FrameCountCheck() is IHtmlable err)
                return err;
            SplittedByCalls splitted = SplitByCalls(input);
            var frags = splitted.Frags;

            ElementCollection res = new();
            foreach(var f in frags)
            {
                if (f.Type == SplittedByCalls.FragTypes.Plain)
                    res.AddFlat(_inlineParser.Value.Run(f.Content, mayContainTemplateCall: false));
                else if (f.Type == SplittedByCalls.FragTypes.Template)
                {
                    res.Add(ParseSingleCall(f.PureContent, out string? templateName, out Template? detected));
                    _ctx.Ref.ReportRef(templateName);
                    if(detected is not null)
                        _ctx.RuleUsage.ReportUsage(detected);
                }
                else if(f.Type == SplittedByCalls.FragTypes.Implant)
                {
                    _ctx.Ref.ReportRef(f.PureContent);
                    string? implantRes = _ctx.DataSource?.Implant(f.PureContent);
                    if (implantRes is null || implantRes == f.PureContent)
                        res.Add(new TextElement(f.Content));//对于未能匹配的Implant，应该仍保留俩括号"{}"，因为它们可能是style标签里的
                    else
                        res.Add(new TextElement(implantRes));
                }
            }
            return res;
        }

        private static readonly string[] valuesSep = new string[] { "&amp;&amp;" };
        private static readonly string[] keyValueSep = new string[] { "::", "：：" };
        public Element ParseSingleCall(string templateCallSource, out string? tName, out Template? detected)
        {
            tName = null;
            try
            {
                ExtractCallName(templateCallSource, out string templateName, out string valueStr);
                if (string.IsNullOrEmpty(templateName))
                    throw new Exception($"{Consts.callFormatMsg}，未填写模板名");
                tName = templateName;
                var template = _ctx.Options.TemplateParsingOptions.Templates.Find(x => x.Name == templateName);
                detected = template;
                if (template is null)
                    throw new Exception($"找不到指定模板:({templateName})");

                var slots = _slotInfo.Get(template);

                List<string> values = valueStr.Split(valuesSep, StringSplitOptions.TrimEntries).ToList();
                values.RemoveAll(string.IsNullOrEmpty);
                Dictionary<TemplateSlot, IHtmlable> valuesDic = new();

                if (values.Count == 1 && !keyValueSep.Any(x => values[0].Contains(x)))
                {
                    //若只填写一个参数，则不需要参数名
                    string key = string.Empty;
                    string value = values[0].Trim();
                    TemplateSlot? slot = slots.FirstOrDefault();
                    if(slot is not null)
                        valuesDic.Add(slot, slot.DealWithContent(value, _blockParser.Value, _inlineParser.Value));
                }
                else
                {
                    foreach (var str in values)
                    {
                        string[] parts = str.Split(keyValueSep, StringSplitOptions.TrimEntries);
                        if (parts.Length < 2)
                            throw new Exception($"{Consts.valueDicFormatMsg}，'&&'间应有一个'::'表示参数名和参数值分隔");
                        string key = parts[0].Trim();
                        string value = parts[1].Trim();
                        if (key.Length == 0)
                            throw new Exception($"{Consts.valueDicFormatMsg}，::前应有参数名");
                        TemplateSlot? slot = slots.FirstOrDefault(x => x.PureValue == key);
                        if(slot is not null)
                            valuesDic.Add(slot, slot.DealWithContent(value, _blockParser.Value, _inlineParser.Value));
                    }
                }
                return new TemplateElement(template, valuesDic, _slotInfo, _ctx.UniqueSlotIncre++);
            }
            catch (Exception e)
            {
                detected = null;
                return new ErrorElement(e.Message);
            }
        }


        public static void ExtractCallName(string call, out string name, out string content)
        {
            call = call.Trim();
            if (call.Length == 0 || call[0] != Consts.tplt_L)
                throw new Exception($"{Consts.callFormatMsg}，未检测到第二个'{{'");
            int layer = 0;
            int pointer = 0;
            foreach(var c in call)
            {
                if (c == Consts.tplt_L)
                    layer++;
                else if (c == Consts.tplt_R)
                    layer--;
                if (layer == 0)
                {
                    name = call.Substring(1, pointer - 1).Trim();
                    if (pointer == call.Length - 1)
                        content = string.Empty;
                    else
                        content = call.Substring(pointer + 1).Trim();
                    return;
                }
                else
                    pointer++;
            }
            throw new Exception($"{Consts.callFormatMsg}，未检测到第一个'}}'");
        }
        public static SplittedByCalls SplitByCalls(string input)
        {
            return new SplittedByCalls(input);
        }
        public class SplittedByCalls
        {
            public List<SplittedFrag> Frags { get; }
            public SplittedByCalls(string input)
            {
                Frags = new();
                input = input.Trim();
                if (string.IsNullOrEmpty(input))
                    return;

                if (!input.Contains(Consts.tplt_L))
                {
                    this.Frags.Add(new(input, false));
                    return;
                }
                int pointerA = 0;
                int pointerB = 0;
                int layer = 0;
                bool isInPlain = true;

                for (int i = 0; i < input.Length; i++)
                {
                    char c = input[i];
                    if (c == Consts.tplt_L)
                        layer++;
                    else if (c == Consts.tplt_R)
                        layer--;
                    if (isInPlain)
                    {
                        if (layer >= 1)
                        {
                            isInPlain = false;
                            this.Frags.Add(new(input.Substring(pointerA,pointerB-pointerA),false));
                            pointerA = pointerB;
                            pointerB++;
                        }
                        else
                        {
                            pointerB++;
                        }
                    }
                    else
                    {
                        if (layer == 0)
                        {
                            isInPlain = true;
                            pointerB++;
                            this.Frags.Add(new(input.Substring(pointerA, pointerB - pointerA), true));
                            pointerA = pointerB;
                        }
                        else
                        {
                            pointerB++;
                        }
                    }
                }
                if (layer != 0)
                    throw new Exception("未闭合'{'与'}'");
                this.Frags.Add(new(input.Substring(pointerA, pointerB - pointerA), false));
            }
            public class SplittedFrag
            {
                public string Content { get; }
                public string PureContent { get
                    {
                        if (Type == FragTypes.Plain || Content.Length<2)
                            return Content;
                        else
                            return Content[1..^1];
                    } }
                public FragTypes Type { get; }
                public SplittedFrag(string content, bool hasBrace)
                {
                    Content = content;
                    if (hasBrace)
                    {
                        if (TemplateElement.IsValidTemplateCall(content))
                            Type = FragTypes.Template;
                        else
                            Type = FragTypes.Implant;
                    }
                    else
                        Type = FragTypes.Plain;
                }
            }
            public enum FragTypes
            {
                Plain = 0,
                Implant = 1,
                Template = 2
            }
        }
    }
}
