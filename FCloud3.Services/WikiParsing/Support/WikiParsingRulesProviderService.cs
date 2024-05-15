using FCloud3.WikiPreprocessor.Rules;
using FCloud3.Repos.WikiParsing;
using System.Text;

namespace FCloud3.Services.WikiParsing.Support
{
    public class WikiParsingRulesProviderService
    {
        private readonly WikiTemplateRepo _wikiTemplateRepo;
        public WikiParsingRulesProviderService(WikiTemplateRepo wikiTemplateRepo)
        {
            _wikiTemplateRepo = wikiTemplateRepo;
        }

        public WikiRulesCommonsResult GetCommonsOfRules(List<string> ruleNames)
        {
            List<IRule> rules = GetAllRulesWithCommons();
            rules.RemoveAll(x => !ruleNames.Contains(x.UniqueName));
            WikiRulesCommonsResult res = new();
            rules.ForEach(r => res.AddItem(r.UniqueName, r.GetStyles(), r.GetPreScripts(), r.GetPostScripts()));
            return res;
        }

        public List<IRule> GetAllRulesWithCommons()
        {
            List<IRule> rules = new();
            rules.AddRange(InternalBlockRules.GetInstances());
            rules.AddRange(InternalInlineRules.GetInstances());
            rules.AddRange(InternalTemplates.GetInstances());
            rules.AddRange(this.GetDbTemplates());
            rules.FilterAllWithCommons();
            return rules;
        }
        private List<IRule> GetDbTemplates()
        {
            return _wikiTemplateRepo.Existing.ToList().ConvertAll(t =>
            {
                return new Template(t.Name, t.Source, t.Styles, t.PreScripts, t.PostScripts) as IRule;
            });
        }

        public class WikiRulesCommonsResult
        {
            public List<WikiRulesCommonsResultItem> Items { get; set; }
            public WikiRulesCommonsResult() { Items = new(); }
            public void AddItem(string? ruleName, string? styles, string? preScripts, string? postScripts)
            {
                Items.Add(new()
                {
                    RuleName = ruleName,
                    Styles = styles,
                    PreScripts = preScripts,
                    PostScripts = postScripts
                });
            }
            public class WikiRulesCommonsResultItem
            {
                public string? RuleName { get; set; }
                public string? Styles { get; set; }
                public string? PreScripts { get; set; }
                public string? PostScripts { get; set; }
            }
        }
    }
}
