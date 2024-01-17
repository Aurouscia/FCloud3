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
    }
}
