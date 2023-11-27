using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FCloud3.HtmlGen.Rules
{
    public class HtmlTemplate : IHtmlRule, IEquatable<HtmlTemplate>
    {
        public string Name { get;}
        public string Source { get; }
        public string PreCommons { get; }
        public string PostCommons { get; }
        public string Styles { get; }
        public bool IsSingleUse => false;

        public HtmlTemplate(string name, string source,string styles="", string preScripts = "", string postScripts = "")
        {
            Name = name;
            Source = source;
            Styles = styles;
            PreCommons = preScripts;
            PostCommons = postScripts;
        }

        public const string SlotGettingRegex = @"(?<=\[\[)[\u4E00-\u9FA5A-Za-z0-9_]+?(?=\]\])";
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
            string match = $"[[{slot}]]";
            return code.Replace(match,value);
        }

        public string GetPreScripts() => PreCommons;
        public string GetPostScripts() => PostCommons;
        public string GetStyles() => Styles;

        public bool Equals(HtmlTemplate? other)
        {
            if(other == null) 
                return false;
            return other.Name == this.Name;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as HtmlTemplate);
        }

        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }
    }
}
