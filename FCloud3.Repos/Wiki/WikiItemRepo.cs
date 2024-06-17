using FCloud3.DbContexts;
using FCloud3.Entities.Wiki;
using FCloud3.Repos.Etc;
using System.Text.RegularExpressions;
using FCloud3.Repos.Etc.Caching;

namespace FCloud3.Repos.Wiki
{
    public class WikiItemRepo : RepoBaseWithCaching<WikiItem, WikiItemCachingModel>
    {
        private const string validUrlPathNamePattern = @"^[A-Za-z0-9\-]{1,}$";
        public WikiItemRepo(
            FCloudContext context,
            ICommitingUserIdProvider userIdProvider,
            WikiItemCaching wikiItemCaching) 
            : base(context, userIdProvider, wikiItemCaching)
        {
        }

        public IQueryable<WikiItem> ExistingAndNotSealed => Existing.Where(x => !x.Sealed);
        public IQueryable<WikiItem> QuickSearch(string str)
        {
            return Existing
                .Where(x => x.Title != null && x.Title.Contains(str))
                .OrderBy(x => x.Title!.Length)
                .ThenByDescending(x => x.Updated);
        }
        public IQueryable<WikiItem> GetByUrlPathName(string urlPathName)
        {
            return Existing.Where(x => x.UrlPathName == urlPathName);
        }

        public override int GetOwnerIdById(int id)
        {
            return Existing.Where(x => x.Id == id).Select(x => x.OwnerUserId).FirstOrDefault();
        }

        public override bool TryAddCheck(WikiItem item, out string? errmsg)
        {
            return InfoCheck(item,false,out errmsg);
        }
        public override bool TryAdd(WikiItem item, out string? errmsg)
        {
            item.OwnerUserId = _userIdProvider.Get();
            return base.TryAdd(item, out errmsg);
        }
        public override int TryAddAndGetId(WikiItem item, out string? errmsg)
        {
            item.OwnerUserId = _userIdProvider.Get();
            return base.TryAddAndGetId(item, out errmsg);
        }
        public override bool TryEditCheck(WikiItem item, out string? errmsg)
        {
            return InfoCheck(item,true, out errmsg);
        }
        public override bool TryRemoveCheck(WikiItem item, out string? errmsg)
        {
            var uid = _userIdProvider.Get();
            if(item.OwnerUserId != uid)
            {
                errmsg = "只有所有者能删除词条";
                return false;
            }
            errmsg = null;
            return true;
        }
        public bool InfoCheck(WikiItem item,bool existing , out string? errmsg)
        {
            errmsg = null;
            if (string.IsNullOrWhiteSpace(item.Title))
            {
                errmsg = "词条标题不能为空";
                return false;
            }
            if (string.IsNullOrWhiteSpace(item.UrlPathName))
            {
                errmsg = "词条路径名不能为空";
                return false;
            }
            if (item.Title.Length > WikiItem.titleMaxLength)
            {
                errmsg = $"词条标题不能超过{WikiItem.titleMaxLength}个字符";
                return false;
            }
            if (item.UrlPathName.Length > WikiItem.urlPathNameMaxLength)
            {
                errmsg = $"词条路径名不能超过{WikiItem.urlPathNameMaxLength}个字符";
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

        public override IQueryable<WikiItem> OwnedByUser(int uid = -1)
        {
            if (uid == -1)
                return Existing;
            return Existing.Where(x => x.OwnerUserId == uid);
        }

        public override void UpdateTime(int id)
        {
            base.UpdateTime(id);
            _caching.Update(id, x=>x.Update=DateTime.Now);
        }
        public override void UpdateTime(List<int> ids)
        {
            base.UpdateTime(ids);
            _caching.UpdateRange(ids, x=>x.Update=DateTime.Now);
        }
        public override int UpdateTime(IQueryable<int> ids)
        {
            var idList = ids.ToList();
            var count = base.UpdateTime(ids);
            _caching.UpdateRange(idList, x=>x.Update=DateTime.Now);
            return count;
        }
    }
}
