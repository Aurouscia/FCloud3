using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FCloud3.HtmlGen.Options
{
    public class HtmlTemplate
    {
        public string? Name { get; set; }
        public string? Source { get; set; }

        public const string SlotGettingRegex = @"(?<=\{\{)[\u4E00-\u9FA5A-Za-z0-9_]+?(?=\}\})";
        public List<string> GetSlots()
        {
            List<string> slots = new();
            if(Source is null)
                return slots;
            var matches = Regex.Matches(this.Source, SlotGettingRegex);
            foreach(var match in matches.AsEnumerable<Match>()) 
            {
                string slot = match.Value;
                if (!slots.Contains(slot))
                {
                    slots.Add(slot);
                }
            }
            return slots;
        }
        public static string Fill(string code,string slot, string value)
        {
            string match = "{{" + slot + "}}";
            return code.Replace(match,value);
        }
    }
}
