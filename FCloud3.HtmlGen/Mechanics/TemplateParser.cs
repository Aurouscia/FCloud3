using FCloud3.HtmlGen.Models;
using FCloud3.HtmlGen.Options;
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
            var frags = splitted.OrderedFrags;
            ElementCollection res = new();
            foreach(var f in frags)
            {
                if (f.IsPlain)
                    res.AddRange(_inlineParser.Value.Run(f.Content,mayContainTemplateCall:false));
                else
                    res.Add(ParseSingleCall(f.Content));
            }
            return res;
        }

        private static readonly string[] valuesSep = new string[] { "&&" };
        private static readonly string[] keyValueSep = new string[] { "::", "：：" };
        public Element ParseSingleCall(string templateCallSource)
        {
            try
            {
                ExtractCallName(templateCallSource, out string templateName, out string valueStr);
                if (string.IsNullOrEmpty(templateName))
                    throw new Exception($"{Consts.callFormatMsg}，未填写模板名");
                var template = _options.Templates.Find(x => x.Name == templateName);
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
            private List<string> Plains { get; }
            private List<string> Calls { get; }
            public List<SplittedFrag> OrderedFrags { get; }
            public SplittedByCalls(string input)
            {
                OrderedFrags = new();
                Plains = new();
                Calls = new();
                input = input.Trim();
                if (string.IsNullOrEmpty(input))
                    return;
                StringBuilder sb = new();
                int layer = 0;
                bool isInPlain = true;

                foreach (var c in input)
                {
                    if (c == Consts.tplt_L)
                        layer++;
                    else if (c == Consts.tplt_R)
                        layer--;
                    if (isInPlain)
                    {
                        if (layer >= 1)
                        {
                            isInPlain = false;
                            this.Plains.Add(sb.ToString());
                            sb.Clear();
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
                            this.Calls.Add(sb.ToString());
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
                this.Plains.Add(sb.ToString());

                while (Plains.Count > 0 || Calls.Count > 0)
                {
                    if (Plains.Count > 0)
                    {
                        this.OrderedFrags.Add(new(Plains[0], true));
                        Plains.RemoveAt(0);
                    }
                    if (Calls.Count > 0)
                    {
                        this.OrderedFrags.Add(new(Calls[0], false));
                        Calls.RemoveAt(0);
                    }
                }
            }
            public class SplittedFrag
            {
                public string Content { get; }
                public bool IsPlain { get; }
                public SplittedFrag(string content, bool isPlain)
                {
                    Content = content;
                    IsPlain = isPlain;
                }
            }
        }
    }
}
