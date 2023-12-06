using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FCloud3.HtmlGen.Rules
{
    public class Template : IRule, IEquatable<Template>
    {
        public string Name { get;}
        public string Source { get; }
        public string PreCommons { get; }
        public string PostCommons { get; }
        public string Styles { get; }
        public bool IsSingleUse => false;

        public Template(string name, string source,string styles="", string preScripts = "", string postScripts = "")
        {
            Name = name;
            Source = source;
            Styles = styles;
            PreCommons = preScripts;
            PostCommons = postScripts;
        }

        public string GetPreScripts() => PreCommons;
        public string GetPostScripts() => PostCommons;
        public string GetStyles() => Styles;

        public bool Equals(Template? other)
        {
            if(other == null) 
                return false;
            return other.Name == this.Name;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as Template);
        }

        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }


    }
}
