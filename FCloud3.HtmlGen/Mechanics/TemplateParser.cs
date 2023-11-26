using FCloud3.HtmlGen.Models;
using FCloud3.HtmlGen.Options;
using FCloud3.HtmlGen.Rules;
using FCloud3.HtmlGen.Util;
using System.Text;

namespace FCloud3.HtmlGen.Mechanics
{
    public class TemplateParser
    {
        private readonly HtmlGenOptions _options;
        private readonly Lazy<BlockParser> _blockParser;
        private readonly Lazy<InlineParser> _inlineParser;
        private readonly TemplateSlotInfoCache _slotInfoCache;

        public TemplateParser(HtmlGenOptions options)
        {
            //可能造成stackOverflow，解决办法：都改成Lazy的
            _options = options;
            _blockParser = new(()=>new(options));
            _inlineParser = new(()=>new(options));
            _slotInfoCache = new();
        }


        public ElementCollection Run(string input)
        {
            SplittedByCalls splitted = SplitByCalls(input);
            var frags = splitted.Frags;
            ElementCollection res = new();
            foreach(var f in frags)
            {
                if (f.Type == SplittedByCalls.FragTypes.Plain)
                    res.AddRange(_inlineParser.Value.Run(f.Content, mayContainTemplateCall: false));
                else if (f.Type == SplittedByCalls.FragTypes.Template)
                {
                    res.Add(ParseSingleCall(f.PureContent,out HtmlTemplate? detected));
                    if(detected is not null)
                        _options.ReportUsage(detected);
                }
                else
                {
                    string? implantRes = _options.Implants(f.PureContent);
                    if (implantRes is null)
                        res.Add(new ErrorElement($"不存在的内插：{f.Content}"));
                    else
                        res.Add(new TextElement(implantRes));
                }
            }
            return res;
        }

        private static readonly string[] valuesSep = new string[] { "&&" };
        private static readonly string[] keyValueSep = new string[] { "::", "：：" };
        public Element ParseSingleCall(string templateCallSource,out HtmlTemplate? detected)
        {
            try
            {
                ExtractCallName(templateCallSource, out string templateName, out string valueStr);
                if (string.IsNullOrEmpty(templateName))
                    throw new Exception($"{Consts.callFormatMsg}，未填写模板名");
                var template = _options.Templates.Find(x => x.Name == templateName);
                detected = template;
                if (template is null)
                    throw new Exception($"找不到指定模板:({templateName})");

                List<string> values = valueStr.Split(valuesSep, StringSplitOptions.TrimEntries).ToList();
                values.RemoveAll(string.IsNullOrEmpty);
                Dictionary<string, ElementCollection> valuesDic = new();

                if (values.Count == 1 && !keyValueSep.Any(x => values[0].Contains(x)))
                {
                    //若只填写一个参数，则不需要参数名
                    string key = string.Empty;
                    string value = values[0].Trim();
                    valuesDic.Add(key, _blockParser.Value.Run(value, enforceBlock:false));
                }
                else
                {
                    foreach (var str in values)
                    {
                        string[] parts = str.Split(keyValueSep, StringSplitOptions.TrimEntries);
                        if (parts.Length != 2)
                            throw new Exception($"{Consts.valueDicFormatMsg}，'&&'间应只有一个'::'");
                        string key = parts[0].Trim();
                        string value = parts[1].Trim();
                        valuesDic.Add(key, _blockParser.Value.Run(value, enforceBlock: false));
                    }
                }
                return new TemplateElement(template, valuesDic, _slotInfoCache);
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
                    name = call.Substring(1, pointer - 1);
                    if (pointer == call.Length - 1)
                        content = string.Empty;
                    else
                        content = call.Substring(pointer + 1);
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
                StringBuilder sb = new();
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
                            this.Frags.Add(new(sb.ToString(),false));
                            sb.Clear();
                            sb.Append(c);
                        }
                        else
                        {
                            sb.Append(c);
                        }
                    }
                    else
                    {
                        if (layer == 0)
                        {
                            isInPlain = true;
                            sb.Append(c);
                            this.Frags.Add(new(sb.ToString(), true));
                            sb.Clear();
                        }
                        else
                        {
                            sb.Append(c);
                        }
                    }
                }
                if (layer != 0)
                    throw new Exception("本段内有未闭合'{'与'}'");
                this.Frags.Add(new(sb.ToString(), false));
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
