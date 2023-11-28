using FCloud3.HtmlGen.Options;
using FCloud3.HtmlGen.Rules;
using FCloud3.HtmlGen.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.HtmlGen.Models
{
    public class TemplateElement : Element
    {
        private readonly HtmlTemplate _template;
        private readonly Dictionary<string, IHtmlable> _values;
        private readonly TemplateSlotInfoCache _slotInfoCache;

        public TemplateElement(HtmlTemplate template, Dictionary<string,IHtmlable> values, TemplateSlotInfoCache slotInfoCache)
        {
            _template = template;
            _values = values;
            _slotInfoCache = slotInfoCache;
        }
        public override string ToHtml()
        {
            string? code = _template.Source;
            if (code is null) 
                return ErrMsg.Inline($"模板[{_template.Name}]异常：缺少源代码");
            List<string> slots = _slotInfoCache.Get(_template);
            if (slots is null)
                return code;
            if (_values.Count == 1 && _values.ContainsKey(string.Empty) && slots.Count >= 1)
            {
                //省略参数名的简写形式
                _ = _values.TryGetValue(string.Empty, out var value);
                value ??= new EmptyElement();
                code = HtmlTemplate.Fill(code, slots[0], value.ToHtml());
            }
            else
            {
                foreach (var slotName in slots)
                {
                    _ = _values.TryGetValue(slotName, out var value);
                    value ??= new EmptyElement();
                    code = HtmlTemplate.Fill(code, slotName, value.ToHtml());
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

    public class TemplateSlotInfoCache:Dictionary<string, List<string>>
    {
        public List<string> Get(HtmlTemplate template)
        {
            if (string.IsNullOrEmpty(template.Name))
                return new();
            if (this.TryGetValue(template.Name, out var list))
                return list;
            var slots = template.GetSlots();
            this.Add(template.Name, slots);
            return slots;
        }
    }
}
