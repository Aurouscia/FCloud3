using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.WikiPreprocessor.Util
{
    public static class Consts
    {
        public const char lineSep='\n';
        public const char tplt_L = '{';
        public const char tplt_R = '}';
        public static readonly List<char> htmlAvoidChars = ['\n', '\r'];
        public const string callFormatMsg = "模板调用格式：{{模板名}参数列表}";
        public const string valueDicFormatMsg = "参数列表格式：参数名A::参数值A && 参数名B::参数值B";

        public const char titleLevelMark = '#';

        public const string locatorAttrName = "loc";
        public const string titleIdAttrName = "id";
        public const string redLinkClassName = "redLink";
        public const string redLinkItemUrlAttr = "pathName";
    }
}
