using FCloud3.HtmlGen.Options;
using FCloud3.HtmlGen.Options.SubOptions;
using FCloud3.HtmlGen.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.HtmlGenTest
{
    [TestClass]
    public class OptionsBuildingTest
    {
        public OptionsBuildingTest() { }

        [TestMethod]
        public void BuildTemplate()
        {
            var template1 = new HtmlTemplate("A", "667788");
            var template2 = new HtmlTemplate("B", "aabbcc");
            var template3 = new HtmlTemplate("A", "哼哼唧唧");
            var template4 = new HtmlTemplate("C", "#$%");
            var templateOptions = new TemplateParsingOptions(new() { template1,template2 });
            var extra = new List<HtmlTemplate>() { template3,template4 };
            var answer = new List<HtmlTemplate> { template1, template2, template4 };

            HtmlGenOptionsBuilder builder = new(templateOptions, new(), new());
            builder.ExtraTemplates.AddRange(extra);
            var options = builder.GetOptions();

            CollectionAssert.AreEquivalent(
                options.TemplateParsingOptions.Templates.Select(x=>x.Source).ToList(),
                answer.Select(x=>x.Source).ToList());
        }

        [TestMethod]
        public void BuildInline()
        {
            var inline1 = new HtmlCustomInlineRule("**", "**", "<strong>", "</strong>");
            var inline2 = new HtmlCustomInlineRule("**", "**", "<em>", "<em>");
            var inline3 = new HtmlCustomInlineRule("$$", "$$", "money", "money");
            var inline4 = new HtmlCustomInlineRule("^^", "^^", "<i>", "</i>");
            var inlineOptions = new InlineParsingOptions(new List<IHtmlInlineRule>(){ inline1, inline3 });
            var extra = new List<IHtmlInlineRule>() { inline2, inline4 };
            var answer = InternalInlineRules.GetInstances();
            answer.RemoveAll(x => x.MarkLeft == "**");
            answer.AddRange(new List<IHtmlInlineRule>() { inline1, inline3, inline4 });

            HtmlGenOptionsBuilder builder = new(new(),inlineOptions,new());
            builder.ExtraInlineRules.AddRange(extra);
            var options = builder.GetOptions();

            CollectionAssert.AreEquivalent(
                options.InlineParsingOptions.InlineRules.Select(x => x.MarkLeft).ToList(),
                answer.Select(x => x.MarkLeft).ToList()
                );
        }
        [TestMethod]
        public void BuildBlock()
        {
            int titleOffset = 2;
            var block1 = new HtmlPrefixBlockRule("-", "<ol>", "</ol>","");
            var block2 = new HtmlPrefixBlockRule("%%", "<textarea>", "</textarea>","");
            var block3 = new HtmlPrefixBlockRule("%%", "<div>", "</div>", "");
            var block4 = new HtmlPrefixBlockRule(">", "<marquee>", "</marquee>", "");
            var blockOptions = new BlockParsingOptions(new List<IHtmlBlockRule>() { block1, block2 },titleOffset);
            var extra = new List<IHtmlBlockRule>() { block3, block4 };
            var answer = InternalBlockRules.GetInstances();
            answer.RemoveAll(x => x is HtmlPrefixBlockRule p && (p.Mark == "-" || p.Mark==">"));
            answer.AddRange(new List<IHtmlBlockRule>() { block1,block2,block4 });

            HtmlGenOptionsBuilder builder = new(new(), new(), blockOptions);
            builder.ExtraBlockRules.AddRange(extra);
            var options = builder.GetOptions();

            CollectionAssert.AreEquivalent(
                options.BlockParsingOptions.BlockRules.Where(x => x is HtmlPrefixBlockRule).Select(x=> (x as HtmlPrefixBlockRule)?.PutLeft).ToList(),
                answer.Where(x => x is HtmlPrefixBlockRule).Select(x => (x as HtmlPrefixBlockRule)?.PutLeft).ToList()
                );
        }

        [TestMethod]
        public void BuildImplant()
        {
            Func<string, string?> implant1 = x =>
            {
                if (x == "abc" || x == "iop")
                    return x.ToUpper()+"1";
                return null;
            };
            Func<string, string?> implant2 = x =>
            {
                if (x == "abc" || x == "qwe")
                    return x.ToUpper()+"2";
                return null;
            };
            var options = new ImplantsHandleOptions(implant1);
            var anotherOption = new ImplantsHandleOptions(implant2);
            options.OverrideWith(anotherOption);
            Assert.AreEqual(options.HandleImplant("abc"),"ABC2");
            Assert.AreEqual(options.HandleImplant("qwe"), "QWE2");
            Assert.AreEqual(options.HandleImplant("iop"), "IOP1");
        }

        [TestMethod]
        public void BuildAutoReplace()
        {
            List<string> detect1 = new(){ "abc", "iop" };
            Func<string, string> implant1 = x =>
            {
                if (x == "abc" || x == "iop")
                    return x.ToUpper() + "1";
                return x;
            };
            List<string> detect2 = new() { "abc", "qwe" };
            Func<string, string> implant2 = x =>
            {
                if (x == "abc" || x == "qwe")
                    return x.ToUpper() + "2";
                return x;
            };
            var options = new AutoReplaceOptions(detect1, implant1);
            var anotherOption = new AutoReplaceOptions(detect2, implant2);
            options.OverrideWith(anotherOption);
            CollectionAssert.AreEquivalent(options.Detects,new List<string>() { "abc","iop","qwe" });
            Assert.AreEqual(options.Replace("abc"), "ABC2");
            Assert.AreEqual(options.Replace("qwe"), "QWE2");
            Assert.AreEqual(options.Replace("iop"), "IOP1");
        }
    }
}