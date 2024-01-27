using FCloud3.DbContexts;
using FCloud3.Entities.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.Repos.Files
{
    public class FileItemRepo : RepoBase<FileItem>
    {
        public FileItemRepo(FCloudContext context) : base(context)
        {
        }
        public IQueryable<FileItem> QuickSearch(string str)
        {
            return Existing.Where(x => x.DisplayName != null && x.DisplayName.Contains(str));
        }
        public string? GetStorePathName(int id)
        {
            return Existing.Where(x => x.Id == id).Select(x => x.StorePathName).FirstOrDefault();
        }
        public IQueryable<FileItem> GetByDirId(int dirId)
        {
            return Existing.Where(x => x.InDir == dirId);
        }
        public override bool TryAddCheck(FileItem item, out string? errmsg)
        {
            if (string.IsNullOrEmpty(item.DisplayName))
            {
                errmsg = "显示名不能为空";
                return false;
            }
            if(item.DisplayName.Length > FileItem.displayNameMaxLength)
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
