using FCloud3.HtmlGen.Options;
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
        private readonly Dictionary<string, ElementCollection> _values;
        private readonly TemplateSlotInfoCache _slotInfoCache;

        public TemplateElement(HtmlTemplate template, Dictionary<string,ElementCollection> values, TemplateSlotInfoCache slotInfoCache)
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
            foreach(var slotName in slots)
            {
                _ = _values.TryGetValue(slotName, out ElementCollection? value);
                value ??= new();
                code = HtmlTemplate.Fill(code, slotName, value.ToHtml());
            }
            return code;
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
