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
        private readonly Dictionary<string, string> _values;

        public TemplateElement(HtmlTemplate template, Dictionary<string,string> values)
        {
            _template = template;
            _values = values;
        }
        public override string ToHtml()
        {
            string? code = _template.Source;
            if (code is null) 
                return ErrMsg.Inline($"模板[{_template.Name}]异常：缺少源代码");
            List<string> slots = _template.GetSlots();
            if (slots is null)
                return code;
            foreach(var slotName in slots)
            {
                string? value = null;
                _ = _values.TryGetValue(slotName, out value);
                value ??= "暂缺";
                code = HtmlTemplate.Fill(code, slotName, value);
            }
            return code;
        }
    }
}
