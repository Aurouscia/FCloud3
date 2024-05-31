﻿using FCloud3.DbContexts;
using FCloud3.Entities.Files;
using FCloud3.Repos.Etc;
using FCloud3.Repos.Etc.Caching;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace FCloud3.Repos.Files
{
    public class FileDirRepo : RepoBase<FileDir>
    {
        private const string validUrlPathNamePattern = @"^[A-Za-z0-9\-]{1,}$";
        private const string zeroIdxUrlPathName = "homeless-items";
        private readonly FileDirCaching _fileDirCaching;
        public FileDirRepo(
            FCloudContext context,
            ICommitingUserIdProvider userIdProvider,
            FileDirCaching fileDirCaching
            ) : base(context, userIdProvider)
        {
            _fileDirCaching = fileDirCaching;
        }
        public List<string>? GetPathById(int id)
        {
            var chain = _fileDirCaching.GetChain(id);
            if (chain is null)
                return null;
            return GetRangeByIdsOrdered<string>(chain, dirs =>
            {
                var list = dirs.Select(x => new { x.Id, x.UrlPathName }).ToList();
                return list.ToDictionary(x => x.Id, x => x.UrlPathName ?? "???");
            });
        }

        public List<int>? GetChainIdsById(int id)
            => _fileDirCaching.GetChain(id);
        public List<int>? GetChainIdsByPath(string[] path)
            => Existing.GetChainIdsByPath(path);
        public List<FileDir>? GetChainByPath(string[] path)
            => Existing.GetChainByPath(path);
        public bool SetUpdateTimeAncestrally(int id, out string? errmsg)
        {
            List<int>? chain = _fileDirCaching.GetChain(id);
            if (chain is null)
            {
                errmsg = "更新文件夹时间出错：树状结构溯源失败";
                return false;
            }
            Existing.Where(x => chain.Contains(x.Id)).ExecuteUpdate(x=>x.SetProperty(d=>d.Updated, DateTime.Now));
            errmsg = null;
            return true;
        }
        public bool SetUpdateTimeRangeAncestrally(IQueryable<int> ids, out string? errmsg)
        {
            foreach (var d in ids.AsEnumerable())
            {
                if(!SetUpdateTimeAncestrally(d, out errmsg))
                    return false;
            }
            errmsg = null;
            return true;
        }


        public IQueryable<FileDir>? GetChildrenById(int id)
            => Existing.Where(x => x.ParentDir == id);

        public bool UpdateDescendantsInfoFor(List<int> masters, out string? errmsg)
        {
            errmsg = null;
            //更新所有子代的计算涉及到大量查询，移动到caching服务里计算，
            //计算完成后的结果拿回来更新数据库
            var changedData = _fileDirCaching.UpdateDescendantsInfoFor(masters);
            var changedIds = changedData.ConvertAll(x => x.Id);
            var dbModelsNeedMutate = GetRangeByIds(changedIds);
            foreach(var m in dbModelsNeedMutate)
            {
                var data = changedData.Find(x => x.Id == m.Id);
                if (data is null)
                    continue;
                m.Depth = data.Depth;
            }
            _context.UpdateRange(dbModelsNeedMutate);
            _context.SaveChanges();
            errmsg = null; 
            return true;
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
            if (item.UrlPathName == zeroIdxUrlPathName)
            {
                errmsg = "请勿使用该路径名";
                return false;
            }
            if (!Regex.IsMatch(item.UrlPathName, validUrlPathNamePattern))
            {
                errmsg = "路径名只能有英文字母，数字和\"-\"";
                return false;
            }
            if (item.Depth > 10)
            {
                errmsg = "文件夹层数过深";
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

        public override int GetOwnerIdById(int id)
        {
            if (id == 0)
                return 0;
            return base.GetOwnerIdById(id);
        }
    }
}
