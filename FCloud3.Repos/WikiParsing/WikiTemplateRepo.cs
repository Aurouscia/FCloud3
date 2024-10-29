using FCloud3.DbContexts;
using FCloud3.Entities.WikiParsing;
using FCloud3.Repos.Etc;
using NPOI.SS.Formula.PTG;

namespace FCloud3.Repos.WikiParsing
{
    public class WikiTemplateRepo : RepoBase<WikiTemplate>
    {
        public WikiTemplateRepo(FCloudContext context, ICommitingUserIdProvider userIdProvider) 
            : base(context, userIdProvider)
        {
        }
        public IQueryable<WikiTemplate> QuickSearch(string s)
        {
            return Existing.Where(x => x.Name != null && x.Name.Contains(s))
                .OrderBy(x => x.Updated);
        }
        public new void Remove(int id) => base.Remove(id);
        public bool TryAdd(WikiTemplate item, out string? errmsg)
        {
            if (!NameCheck(item.Name, out errmsg))
                return false;
            if (Existing.Any(x => x.Name == item.Name)) 
            {
                errmsg = "已存在同名模板";
                return false;
            }
            base.Add(item);
            return true;
        }
        public bool TryUpdate(WikiTemplate item, out string? errmsg)
        {
            if (!NameCheck(item.Name, out errmsg))
                return false;
            if (Existing.Any(x => x.Id!=item.Id && x.Name == item.Name))
            {
                errmsg = "已存在其他同名模板";
                return false;
            }
            base.Update(item);
            return true;
        }
        private static bool NameCheck(string? name, out string? errmsg)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                errmsg = "名称不能为空";
                return false;
            }
            if (name.Length > WikiTemplate.nameMaxLength)
            {
                errmsg = $"名称最长{WikiTemplate.nameMaxLength}个字符";
                return false;
            }
            errmsg = null;
            return true;
        }
    }
}
