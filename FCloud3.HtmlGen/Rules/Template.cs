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

    public static class InternalTemplates
    {
        //带js的模板仅允许超级管理员创建（否则会遭到xss攻击）
        //js的全局作用域只能声明var变量（预览刷新时会重复运行）
        public static List<Template> GetInstances()
        {
            var test = new Template(
                name: "模板测试",
                source: "<div class=\"tt\"><div>模板测试</div>参数一：<b class=\"ttb\">[[__参数一__]]</b><br/><u onclick=\"alert('点击u标签')\">参数二：[[[__参数二__]]]</u></div>",
                styles: ".tt{background-color:#eee} .tt b{color:red;cursor:pointer} .tt u{color:blue;cursor:pointer}",
                preScripts:"",
                postScripts:"var ts = document.getElementsByClassName(\"ttb\");\r\n    for(const t of ts){\r\n        t.addEventListener(\"click\",()=>{alert(\"点击b标签\")})\r\n    }");
            return new List<Template> { test };
        }
    }
}
