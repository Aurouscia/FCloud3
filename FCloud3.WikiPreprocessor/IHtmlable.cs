﻿using FCloud3.WikiPreprocessor.Context.SubContext;
using FCloud3.WikiPreprocessor.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.WikiPreprocessor
{
    public interface IHtmlable
    {
        public string ToHtml();
        public void WriteHtml(StringBuilder sb);
        public void WriteBody(StringBuilder sb, int maxLength);
        public List<IRule>? ContainRules();
        public List<IHtmlable>? ContainFootNotes();
        public List<ParserTitleTreeNode>? ContainTitleNodes();
    }
}
