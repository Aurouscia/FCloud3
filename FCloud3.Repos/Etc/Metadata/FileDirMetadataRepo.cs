﻿using FCloud3.Entities.Files;
using FCloud3.Repos.Etc.Metadata.Abstraction;
using Microsoft.Extensions.Logging;

namespace FCloud3.Repos.Etc.Metadata
{
    public class FileDirMetadataRepo : MetadataRepoBase<FileDirMetadata, FileDir>
    {
        public FileDirMetadataRepo(RepoBase<FileDir> repo, ILogger<MetadataRepoBase<FileDirMetadata, FileDir>> logger) : base(repo, logger)
        {
        }

        protected override IQueryable<FileDirMetadata> GetFromDbModel(IQueryable<FileDir> dbModels)
        {
            return dbModels.Select(x => new FileDirMetadata { 
                Id = x.Id,
                ParentDir = x.ParentDir,
                Depth = x.Depth
            });
        }

        public List<int> GetChain(int id)
        {
            if (id == 0)
            {
                return [];
            }
            var all = GetAll();
            List<int> res = new();
            var targetId = id;
            while (true)
            {
                var target = all.Find(x => x.Id == targetId);
                if (target is null)
                    break;
                res.Insert(0, target.Id);
                targetId = target.ParentDir;
                if (targetId == 0)
                    break;
            }
            return res;
        }
        public List<FileDirMetadata> UpdateDescendantsInfoFor(List<int> masters, out string? errmsg)
        {
            errmsg = null;
            var all = GetAll();
            if (masters is null || masters.Count == 0)
                return [];
            var masterData = all.FindAll(x => masters.Contains(x.Id));
            //用changed记录哪些文件夹发生变化
            List<FileDirMetadata> changed = [];

            //从父代到子代计算depth
            void setChildrenDepth(FileDirMetadata dir, int safety)
            {
                if (safety == 50)
                    throw new Exception("未知错误：无穷递归");
                var children = all.FindAll(x => x.ParentDir == dir.Id);
                children.ForEach(x => {
                    x.Depth = dir.Depth + 1;
                    setChildrenDepth(x, safety + 1);
                    changed.Add(x);
                });
            }
            masterData.ForEach(m => setChildrenDepth(m, 0));

            //此时depth已经是正确值
            //可以再从子代到父代再算总字节数，内容数什么的
            return changed;
        }

        private static readonly object LockObj = new();
        protected override object Locker => LockObj;
    }

    public class FileDirMetadata: MetadataBase<FileDir>
    {
        public int ParentDir { get; set; }
        public int Depth { get; set; }
    }
}
