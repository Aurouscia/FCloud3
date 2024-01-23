using FCloud3.DbContexts;
using FCloud3.Entities.Wiki;
using System.Text.RegularExpressions;

namespace FCloud3.Repos.Wiki
{
    public class WikiItemRepo : RepoBase<WikiItem>
    {
        private const string validUrlPathNamePattern = @"^[A-Za-z0-9\-]{1,}$";
        public WikiItemRepo(FCloudContext context) : base(context)
        {
        }

        public IQueryable<WikiItem> QuickSearch(string str)
        {
            return Existing
                .Where(x => x.Title != null && x.Title.Contains(str))
                .OrderBy(x => x.Updated);//可能要按什么别的办法排序
        }

        public override bool TryAddCheck(WikiItem item, out string? errmsg)
        {
            return InfoCheck(item,false,out errmsg);
        }
        public override bool TryEditCheck(WikiItem item, out string? errmsg)
        {
            return InfoCheck(item,true, out errmsg);
        }
        public bool InfoCheck(WikiItem item,bool existing , out string? errmsg)
        {
            errmsg = null;
            if (string.IsNullOrWhiteSpace(item.Title))
            {
                errmsg = "词条名称不能为空";
                return false;
            }
            if (string.IsNullOrWhiteSpace(item.UrlPathName))
            {
                errmsg = "词条路径名不能为空";
                return false;
            }
            if (!Regex.IsMatch(item.UrlPathName,validUrlPathNamePattern))
            {
                errmsg = "路径名只能有英文字母，数字和\"-\"";
                return false;
            }
            string? conflict = null;
            if (existing)
            {
                conflict = Existing.Where(x => x.UrlPathName == item.UrlPathName)
                    .Where(x=>x.Id!=item.Id)
                    .Select(x => x.Title).FirstOrDefault();
            }
            else
            {
                conflict = Existing.Where(x => x.UrlPathName == item.UrlPathName)
                    .Select(x => x.Title).FirstOrDefault();
            }
            if (conflict is not null)
            {
                errmsg = $"冲突：已有同样路径名的其他词条【{conflict}】";
                return false;
            }
            return true;
        }
    }
}
