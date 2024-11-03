using FCloud3.DbContexts;
using FCloud3.Entities.Files;
using FCloud3.Repos.Etc;
using Microsoft.EntityFrameworkCore;

namespace FCloud3.Repos.Files
{
    public class FileItemRepo : RepoBase<FileItem>
    {
        public FileItemRepo(FCloudContext context,ICommitingUserIdProvider userIdProvider) : base(context,userIdProvider)
        {
        }
        public IQueryable<FileItem> QuickSearch(string str)
        {
            return Existing
                .Where(x => x.DisplayName != null && x.DisplayName.Contains(str))
                .OrderBy(x => x.DisplayName!.Length)
                .ThenByDescending(x => x.Updated);
        }
        public string? GetStorePathName(int id)
        {
            return Existing.Where(x => x.Id == id).Select(x => x.StorePathName).FirstOrDefault();
        }
        public IQueryable<FileItem> GetByDirId(int dirId)
        {
            return Existing.Where(x => x.InDir == dirId);
        }
        public bool TryAdd(FileItem item, out string? errmsg)
        {
            if(!InfoCheck(item, out errmsg)) 
                return false;
            var sameName = Existing.Where(x => x.Hash == item.Hash).Select(x => x.DisplayName).FirstOrDefault();
            if (sameName is not null)
            {
                errmsg = $"已存在内容完全相同的文件({sameName})";
                return false;
            }

            base.Add(item);
            errmsg = null;
            return true;
        }
        public int TryAddAndGetId(FileItem item, out string? errmsg)
        {
            if (TryAdd(item, out errmsg))
                return item.Id;
            return 0;
        }
        public bool TryUpdate(FileItem item, out string? errmsg)
        {
            if (!InfoCheck(item, out errmsg))
                return false;
            base.Update(item);
            errmsg = null;
            return true;
        }
        public void Remove(FileItem item) => base.Remove(item);
        public bool SetInDirForRange(int distDirId, List<int> ids, out string? errmsg)
        {
            int affected = base.GetRangeByIds(ids)
                .ExecuteUpdate(x => x
                    .SetProperty(f => f.InDir, distDirId)
                    .SetProperty(f => f.Updated, DateTime.Now));
            AfterDataChange();
            errmsg = null;
            return true;
        }

        public bool InfoCheck(FileItem item, out string? errmsg)
        {
            if (string.IsNullOrWhiteSpace(item.DisplayName))
            {
                errmsg = "显示名不能为空";
                return false;
            }
            if (item.DisplayName.Length > FileItem.displayNameMaxLength)
            {
                errmsg = $"显示名过长，请缩短";
                return false;
            }
            if (string.IsNullOrEmpty(item.StorePathName))
            {
                errmsg = "存储路径不能为空";
                return false;
            }
            if (item.StorePathName.Length > FileItem.storePathNameMaxLength)
            {
                errmsg = $"存储名过长，请缩短";
                return false;
            }
            if (item.StorePathName.StartsWith("/"))
            {
                errmsg = $"存储名不能以/开头";
                return false;
            }
            errmsg = null;
            return true;
        }
    }
}
