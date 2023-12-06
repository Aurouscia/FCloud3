using FCloud3.HtmlGen.Mechanics;
using FCloud3.HtmlGen.Options;
using FCloud3.HtmlGen.Rules;
using FCloud3.HtmlGen.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Reflection.Metadata.BlobBuilder;

namespace FCloud3.HtmlGen.Models
{
    public class TemplateElement : Element
    {
        private readonly Template _template;
        private readonly Dictionary<TemplateSlot, IHtmlable> _values;
        private readonly TemplateSlotInfo _slotInfo;
        private readonly int _uniqueSlotIncre;

        public TemplateElement(Template template, Dictionary<TemplateSlot,IHtmlable> values, TemplateSlotInfo slotInfo,int uniqueSlotIncre = 0)
        {
            _template = template;
            _values = values;
            _slotInfo = slotInfo;
            _uniqueSlotIncre = uniqueSlotIncre;
        }
        public override string ToHtml()
        {
            string? code = _template.Source;
            if (code is null) 
                return ErrMsg.Inline($"模板[{_template.Name}]异常：缺少源代码");
            List<TemplateSlot> slots = _slotInfo.Get(_template);

            foreach (var slot in slots)
            {
                if (slot is UniqueSlot u)
                {
                    var replace = u.BuildReplacement(_uniqueSlotIncre);
                    if (!_values.TryAdd(slot, replace))
                        _values[slot] = replace;
                }
            }

            if (slots is null || slots.Count==0)
                return code;
            {
                foreach (var slot in slots)
                {
                    _ = _values.TryGetValue(slot, out var value);
                    value ??= slot.DefaultContent();
                    code = code.Replace(slot.Value, value.ToHtml());
                }
            }
            return code;
        }
        public static bool IsValidTemplateCall(string thingLikeCall)
        {
            thingLikeCall = thingLikeCall.Trim();
            if (thingLikeCall.Length < 5)
                return false;
            if(!thingLikeCall.StartsWith(Consts.tplt_L) || !thingLikeCall.EndsWith(Consts.tplt_R))
                return false;
            string insideBrace = thingLikeCall.Substring(1, thingLikeCall.Length - 2);
            return insideBrace.TrimStart().StartsWith(Consts.tplt_L) && insideBrace.Contains(Consts.tplt_R);
        }
    }

    /// <summary>
    /// 存储每一个模板内部的插槽信息，key为模板名，value为插槽List
    /// </summary>
    public class TemplateSlotInfo : Dictionary<string, List<TemplateSlot>>
    {
        public TemplateSlotInfo() 
        {
        }
        /// <summary>
        /// 懒加载地获取一个模板中的所有插槽
        /// </summary>
        /// <param name="template">模板</param>
        /// <returns></returns>
        public List<TemplateSlot> Get(Template template)
        {
            if (string.IsNullOrEmpty(template.Name))
                return new();
            if (this.TryGetValue(template.Name, out var list))
                return list;
            var slots = GetSlots(template);
            this.Add(template.Name, slots);
            return slots;
        }
        private List<TemplateSlot> GetSlots(Template template)
        {
            List<TemplateSlot> slots = new();
            if (template.Source is null)
                return slots;
            MatchAndCollect(template.Source, PlainSlot.MatchRegex, slots, x => new PlainSlot(x));
            MatchAndCollect(template.Source, ParseSlot.MatchRegex, slots, x => new ParseSlot(x));
            MatchAndCollect(template.Source, UniqueSlot.MatchRegex, slots, x => new UniqueSlot(x));
            return slots;
        }
        private static void MatchAndCollect(string source, string regex, List<TemplateSlot> data, Func<string,TemplateSlot> constructor)
        {
            var matches = Regex.Matches(source, regex);
            foreach (var match in matches.AsEnumerable<Match>())
            {
                string val = match.Value;
                if (!data.Any(x => x.Value == val))
                    data.Add(constructor(val));
            }
        }
    }

    /// <summary>
    /// 模板插槽，表示模板作者希望用户填写的内容被放置到此处
    /// </summary>
    public abstract class TemplateSlot:IEquatable<TemplateSlot>
    {
        /// <summary>
        /// 包含着匹配符号的插槽名
        /// </summary>
        public string Value { get; }
        /// <summary>
        /// 去除了匹配符号的插槽名
        /// </summary>
        public string PureValue { get; }
        public TemplateSlot(string value,string pureValue)
        {
            Value = value;
            PureValue = pureValue;
        }
        public abstract IHtmlable DealWithContent(string content, IBlockParser parser);
        public virtual IHtmlable DefaultContent() => new EmptyElement();

        public bool Equals(TemplateSlot? other)
        {
            return other?.Value == this.Value;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as TemplateSlot);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
    /// <summary>
    /// 表示一个填写内容不会被解析的模板插槽
    /// </summary>
    public class PlainSlot : TemplateSlot
    {
        public const string MatchRegex = @"\[\[\[__[\u4E00-\u9FA5A-Za-z0-9_]{1,10}__\]\]\]";
        public PlainSlot(string value) : base(value, value[5..^5])
        {
        }

        public override IHtmlable DealWithContent(string content, IBlockParser parser)
        {
            return new TextElement(content);
        }
    }
    /// <summary>
    /// 表示一个填写内容会被解析的模板插槽
    /// </summary>
    public class ParseSlot : TemplateSlot
    {
        public const string MatchRegex = @"\[\[__[\u4E00-\u9FA5A-Za-z0-9_]{1,10}__\]\]";
        public ParseSlot(string value) : base(value,value[4..^4])
        {
        }
        public override IHtmlable DealWithContent(string content, IBlockParser parser)
        {
            return parser.Run(content,enforceBlock:false);
        }
    }
    /// <summary>
    /// 表示一个内容自动填入，无需用户指定的模板插槽（内容在同一次调用中相同，但在不同调用的中不同）
    /// </summary>
    public class UniqueSlot : TemplateSlot
    {
        public const string MatchRegex = @"\[\[__%[\u4E00-\u9FA5A-Za-z0-9]{1,10}%__\]\]";

        public UniqueSlot(string value) : base(value,value[5..^5])
        {
        }

        public override IHtmlable DealWithContent(string content, IBlockParser parser)
        {
            return new EmptyElement();
        }

        public IHtmlable BuildReplacement(int templateUsedTimes)
        {
            return new TextElement($"{PureValue}_{templateUsedTimes}");
        }
    }
}
