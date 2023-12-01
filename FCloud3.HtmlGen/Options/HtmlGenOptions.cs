using FCloud3.HtmlGen.Models;
using FCloud3.HtmlGen.Options.SubOptions;
using FCloud3.HtmlGen.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.HtmlGen.Options
{
    /// <summary>
    /// Html生成器的启动选项类
    /// </summary>
    public class HtmlGenOptions:IHtmlGenOptions
    {
        public TemplateParsingOptions TemplateParsingOptions { get; }
        public ImplantsHandleOptions ImplantsHandleOptions { get; }
        public AutoReplaceOptions AutoReplaceOptions { get; }
        public InlineParsingOptions InlineParsingOptions { get; }
        public BlockParsingOptions BlockParsingOptions { get;}

        public HtmlGenOptions(
            TemplateParsingOptions? templateOptions = null,
            ImplantsHandleOptions? implantsHandleOptions = null,
            AutoReplaceOptions? autoReplaceOptions = null,
            InlineParsingOptions? inlineParsingOptions = null,
            BlockParsingOptions? blockParsingOptions = null)
        {
            TemplateParsingOptions = templateOptions ?? new();
            ImplantsHandleOptions = implantsHandleOptions ?? new();
            AutoReplaceOptions = autoReplaceOptions ?? new();
            InlineParsingOptions = inlineParsingOptions ?? new();
            BlockParsingOptions = blockParsingOptions ?? new();
        }

        public void OverrideWith(IHtmlGenOptions another)
        {
            if (another is HtmlGenOptions h)
            {
                this.TemplateParsingOptions.OverrideWith(h.TemplateParsingOptions);
                this.ImplantsHandleOptions.OverrideWith(h.ImplantsHandleOptions);
                this.AutoReplaceOptions.OverrideWith(h.AutoReplaceOptions);
                this.InlineParsingOptions.OverrideWith(h.InlineParsingOptions);
                this.BlockParsingOptions.OverrideWith(h.BlockParsingOptions);
            }
            else if (another is TemplateParsingOptions to)
                TemplateParsingOptions.OverrideWith(to);
            else if (another is ImplantsHandleOptions io)
                ImplantsHandleOptions.OverrideWith(io);
            else if (another is AutoReplaceOptions ao)
                AutoReplaceOptions.OverrideWith(ao);
            else if (another is InlineParsingOptions ilo)
                InlineParsingOptions.OverrideWith(ilo);
            else if (another is BlockParsingOptions bo)
                BlockParsingOptions.OverrideWith(bo);
        }
    }

    public interface IHtmlGenOptionsProvider
    {
        public HtmlGenOptions GetOptions();
    }
    public class HtmlGenOptionsBuilder : IHtmlGenOptionsProvider
    {
        //优先级：xxOptions属性最高，ExtraXXX第二，系统自带的第三
        public TemplateParsingOptions TemplateParsingOptions { get; }
        public InlineParsingOptions InlineParsingOptions { get; }
        public BlockParsingOptions BlockParsingOptions { get; }
        public AutoReplaceOptions AutoReplaceOptions { get; }
        public ImplantsHandleOptions ImplantsHandleOptions { get; }

        public List<IHtmlBlockRule> ExtraBlockRules { get; set; }
        public List<IHtmlInlineRule> ExtraInlineRules { get; set; }
        public List<HtmlTemplate> ExtraTemplates { get; set; }
        public HtmlGenOptionsBuilder(
            List<HtmlTemplate> templates,
            List<HtmlCustomInlineRule> extraInlineRules,
            List<HtmlPrefixBlockRule> extraBlockRules,
            AutoReplaceOptions? autoReplaceOptions = null,
            ImplantsHandleOptions? implantsHandleOptions = null)
        {
            ExtraTemplates = templates;
            ExtraInlineRules = extraInlineRules.ConvertAll(x=>x as IHtmlInlineRule);
            ExtraBlockRules = extraBlockRules.ConvertAll(x => x as IHtmlBlockRule);
            InlineParsingOptions = new();
            BlockParsingOptions = new();
            TemplateParsingOptions = new();
            AutoReplaceOptions = autoReplaceOptions ?? new();
            ImplantsHandleOptions = implantsHandleOptions ?? new();
        }
        public HtmlGenOptionsBuilder(
            TemplateParsingOptions templateOptions,
            InlineParsingOptions inlineOptions,
            BlockParsingOptions blockOptions,
            AutoReplaceOptions? autoReplaceOptions = null,
            ImplantsHandleOptions? implantsHandleOptions = null)
        {
            ExtraTemplates = new();
            ExtraInlineRules = new();
            ExtraBlockRules = new();
            InlineParsingOptions = inlineOptions;
            BlockParsingOptions = blockOptions;
            TemplateParsingOptions = templateOptions;
            AutoReplaceOptions = autoReplaceOptions ?? new();
            ImplantsHandleOptions = implantsHandleOptions ?? new();
        }
        public HtmlGenOptionsBuilder()
        {
            ExtraTemplates = new();
            ExtraInlineRules = new();
            ExtraBlockRules = new();
            InlineParsingOptions = new();
            BlockParsingOptions = new();
            TemplateParsingOptions = new();
            AutoReplaceOptions = new();
            ImplantsHandleOptions = new();
        }
        public HtmlGenOptions GetOptions()
        {
            //谨慎考虑顺序问题
            BlockParsingOptions block = new(InternalBlockRules.GetInstances());//优先级最低
            block.OverrideWith(new BlockParsingOptions(ExtraBlockRules));//优先级第二
            block.OverrideWith(BlockParsingOptions);//优先级最高（有小标题等级偏移设置）

            InlineParsingOptions inline = new(InlineRulesFromAutoReplace(AutoReplaceOptions));
            inline.OverrideWith(new InlineParsingOptions(InternalInlineRules.GetInstances()));
            inline.OverrideWith(new InlineParsingOptions(ExtraInlineRules));
            inline.OverrideWith(InlineParsingOptions);

            TemplateParsingOptions template = new(ExtraTemplates);
            template.OverrideWith(TemplateParsingOptions);

            return new(template, ImplantsHandleOptions, AutoReplaceOptions, inline, block);
        }

        private static List<IHtmlInlineRule> InlineRulesFromAutoReplace(AutoReplaceOptions autoReplaceOptions)
        {
            List<IHtmlInlineRule> inlineRules = new();
            if (autoReplaceOptions is not null && autoReplaceOptions.Detects.Count > 0)
            {
                var detects = autoReplaceOptions.Detects;
                detects.RemoveAll(x => x.Length < 2);
                detects.Sort((x, y) => y.Length - x.Length);
                var rules = detects.ConvertAll(x =>
                    new HtmlLiteralInlineRule(x, () => autoReplaceOptions.Replace(x)));
                inlineRules.InsertRange
                (
                    index: 0,
                    collection: rules
                );
            }
            return inlineRules;
        }
    }
}
