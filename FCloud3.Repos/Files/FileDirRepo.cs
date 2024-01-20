using FCloud3.DbContexts;
using FCloud3.Entities.Files;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.Repos.Files
{
    public class FileDirRepo : RepoBase<FileDir>
    {
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
        public List<int>? GetChainByPath(string[] path)
        {
            return Existing.GetChainByPath(path);
        }
        public IQueryable<FileDir>? GetChildrenByPath(string[] path,out int thisDirId, out string? errmsg)
        {
            errmsg = null;
            int dirId = GetIdByPath(path);
            if (dirId == -1)
            {
                errmsg = "未找到指定路径的文件夹";
                thisDirId = 0;
                return null;
            }
            thisDirId = dirId;
            return Existing.Where(x => x.ParentDir == dirId);
        }
        public bool MoveDirToDir(int movingDirId,int intoDirId, out string? errmsg)
        {
            errmsg = null;
            //if (!LoopCheck(movingDirId, intoDirId, out errmsg))
            //    return false;
            int parentDepth = 0;
            if (intoDirId != 0)
                parentDepth = Existing.Where(x => x.Id == intoDirId).Select(x => x.Depth).FirstOrDefault();

            int res = Existing.Where(x => x.Id == movingDirId)
                .ExecuteUpdate(x => x
                .SetProperty(y => y.ParentDir, intoDirId)
                .SetProperty(y => y.Depth, parentDepth + 1));
            if (res == 1)
                return true;
            errmsg = "更新失败";
            return false;
        }
        //public bool LoopCheck(int movingDirId, int intoDirId, out string? errmsg)
        //{
        //    int lookingAt = intoDirId;
        //    errmsg = null;
        //    while (true)
        //    {
        //        if (lookingAt == 0)
        //            return true;
        //        var parent = Existing.Where(x => x.Id == lookingAt).Select(x=>x.ParentDir).FirstOrDefault();
        //        if (parent == 0)
        //            return true;
        //        if (parent == movingDirId)
        //        {
        //            errmsg = "不允许循环嵌套";
        //            return false;
        //        }
        //        lookingAt = parent;
        //    }
        //}

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
            errmsg = null;
            if (string.IsNullOrEmpty(item.Name?.Trim()))
            {
                errmsg = "文件夹名称不能为空";
                return false;
            }
            return true;
        }
        public override bool TryEditCheck(FileDir item, out string? errmsg)
        {
            errmsg = null;
            if(string.IsNullOrEmpty(item.Name?.Trim()))
            {
                errmsg = "文件夹名称不能为空";
                return false;
            }
            return true;
        }
    }
}
