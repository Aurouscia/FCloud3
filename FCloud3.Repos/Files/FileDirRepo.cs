using FCloud3.DbContexts;
using FCloud3.Entities.Files;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FCloud3.Repos.Files
{
    public class FileDirRepo : RepoBase<FileDir>
    {
        private const string validUrlPathNamePattern = @"^[A-Za-z0-9\-]{1,}$";
        public FileDirRepo(FCloudContext context) : base(context)
        {
        }
        public int GetIdByPath(string[] path)
        {
            return Existing.GetIdByPath(path);
        }
        public FileDir? GetByPath(string[] path)
        {
            int id = Existing.GetIdByPath(path);
            if (id > 0)
                return GetById(id);
            return null;
        }
        public List<int>? GetChainIdsByPath(string[] path)
        {
            return Existing.GetChainIdsByPath(path);
        }
        public List<FileDir>? GetChainByPath(string[] path)
        {
            return Existing.GetChainByPath(path);
        }
        public IQueryable<FileDir>? GetChildrenById(int id)
        {
            return Existing.Where(x => x.ParentDir == id);
        }

        public List<FileDir>? GetDescendantsFor(List<int> dirIds, out string? errmsg)
        {
            errmsg = null;
            List<FileDir> res = new();
            List<int> searching = dirIds;
            do
            {
                var children = Existing.Where(x => searching.Contains(x.ParentDir)).ToList();
                res.AddRange(children);
                searching = children.Select(x => x.Id).ToList();
            } while (searching.Count > 0);
            res = res.DistinctBy(x => x.Id).ToList();
            //var suspicious = res.FindAll(x => dirIds.Contains(x.Id));
            //foreach(var s in suspicious)
            //{
            //    int lookingAt = s.ParentDir;
            //    int safety = 0;
            //    do
            //    {
            //        if (lookingAt == s.Id || safety>50) { errmsg = "检测到循环，请勿把文件夹放入其子代中"; return null; };
            //        var parent = res.Find(x => x.Id == lookingAt);
            //        lookingAt = (parent is not null) ? parent.Id : 0;
            //        safety++;
            //    } while (lookingAt != 0);
            //}
            return res;
        }
        public bool UpdateDescendantsInfoFor(List<FileDir> masters, out string? errmsg)
        {
            errmsg = null;
            List<FileDir>? targets = GetDescendantsFor(masters.ConvertAll(x=>x.Id),out errmsg);
            if (targets is null)
                return false;

            //从父代到子代计算depth
            void setChildrenDepth(FileDir dir, int safety)
            {
                if (safety == 50)
                    throw new Exception("未知错误：无穷递归");
                var children = targets.FindAll(x => x.ParentDir == dir.Id);
                children.ForEach(x => {
                    x.Depth = dir.Depth + 1;
                    setChildrenDepth(x, safety + 1);
                });
            }
            masters.ForEach(m=> setChildrenDepth(m, 0));

            //此时depth已经是正确值
            //可以再从子代到父代再算总字节数，内容数什么的
            return TryEditRange(targets, out errmsg);
        }

        public override bool TryAddCheck(FileDir item, out string? errmsg)
        {
            return InfoCheck(item, out errmsg);
        }
        public override bool TryEditCheck(FileDir item, out string? errmsg)
        {
            return InfoCheck(item, out errmsg);
        }
        public bool InfoCheck(FileDir item, out string? errmsg)
        {
            errmsg = null;
            if (string.IsNullOrWhiteSpace(item.Name))
            {
                errmsg = "文件夹名称不能为空";
                return false;
            }
            if (string.IsNullOrWhiteSpace(item.UrlPathName)){
                errmsg = "文件夹路径名不能为空";
                return false;
            }
            if (!Regex.IsMatch(item.UrlPathName, validUrlPathNamePattern))
            {
                errmsg = "路径名只能有英文字母，数字和\"-\"";
                return false;
            }
            var conflict = Existing.Where(x => x.Id!=item.Id && x.ParentDir == item.ParentDir && x.UrlPathName == item.UrlPathName)
                .Select(x => x.Name).FirstOrDefault();
            if (conflict is not null)
            {
                errmsg = $"冲突：此处已有同样路径名的其他文件夹【{conflict}】";
                return false;
            }
            return true;
        }
    }
}
