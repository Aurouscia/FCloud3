﻿using FCloud3.WikiPreprocessor.Options;
using FCloud3.WikiPreprocessor.Options.SubOptions;
using FCloud3.WikiPreprocessor.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.WikiPreprocessor.Test
{
    [TestClass]
    public class OptionsBuildingTest
    {
        public OptionsBuildingTest() { }

        [TestMethod]
        public void BuildTemplate()
        {
            var template1 = new Template("A", "667788");
            var template2 = new Template("B", "aabbcc");
            var template3 = new Template("A", "哼哼唧唧");
            var template4 = new Template("C", "#$%");
            var original = new List<Template>() { template1,template2 };
            var extra = new List<Template>() { template3,template4 };
            var answer = new List<Template> { template3, template2, template4 };

            ParserBuilder builder = new();
            builder.Template.ClearTemplates();
            builder.Template.AddTemplates(original);
            builder.Template.AddTemplates(extra);
            var options = builder.GetCurrentOptions();

            CollectionAssert.AreEquivalent(
                options.TemplateParsingOptions.Templates.Select(x=>x.Source).ToList(),
                answer.Select(x=>x.Source).ToList());
        }

        [TestMethod]
        public void BuildInline()
        {
            var inline1 = new CustomInlineRule("**", "**", "<strong>", "</strong>");
            var inline2 = new CustomInlineRule("**", "**", "<em>", "<em>");
            var inline3 = new CustomInlineRule("$$", "$$", "money", "money");
            var inline4 = new CustomInlineRule("^^", "^^", "<i>", "</i>");
            var original = new List<IInlineRule>(){ inline1, inline3 };
            var extra = new List<IInlineRule>() { inline2, inline4 };
            var answer = InternalInlineRules.GetInstances();
            answer.RemoveAll(x => x.MarkLeft == "**");
            answer.AddRange(new List<IInlineRule>() { inline2, inline3, inline4 });

            ParserBuilder builder = new();
            builder.Inline.AddMoreRules(original);
            builder.Inline.AddMoreRules(extra);
            var options = builder.GetCurrentOptions();

            CollectionAssert.AreEquivalent(
                options.InlineParsingOptions.InlineRules.Select(x => x.PutLeft).ToList(),
                answer.Select(x => x.PutLeft).ToList()
                );
        }
        [TestMethod]
        public void BuildBlock()
        {
            int titleOffset = 2;
            var block1 = new PrefixBlockRule("-", "<ol>", "</ol>","");
            var block2 = new PrefixBlockRule("%%", "<textarea>", "</textarea>","");
            var block3 = new PrefixBlockRule("%%", "<div>", "</div>", "");
            var block4 = new PrefixBlockRule(">", "<marquee>", "</marquee>", "");
            var original = new List<IBlockRule>() { block1, block2 };
            var extra = new List<IBlockRule>() { block3, block4 };
            var answer = InternalBlockRules.GetInstances();
            answer.RemoveAll(x => x is PrefixBlockRule p && (p.Mark == "-" || p.Mark==">"));
            answer.AddRange(new List<IBlockRule>() { block1,block3,block4 });

            var builder = new ParserBuilder()
                .Block.AddMoreRules(original)
                .Block.AddMoreRules(extra)
                .Block.SetTitleLevelOffset(titleOffset);
            var options = builder.GetCurrentOptions();

            CollectionAssert.AreEquivalent(
                options.BlockParsingOptions.BlockRules.Where(x => x is PrefixBlockRule).Select(x=> (x as PrefixBlockRule)?.PutLeft).ToList(),
                answer.Where(x => x is PrefixBlockRule).Select(x => (x as PrefixBlockRule)?.PutLeft).ToList()
                );
        }
    }
}