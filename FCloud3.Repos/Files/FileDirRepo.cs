using FCloud3.DbContexts;
using FCloud3.Entities.Files;
using FCloud3.Repos.Etc;
using System.Text.RegularExpressions;

namespace FCloud3.Repos.Files
{
    public class FileDirRepo : RepoBaseCache<FileDir, FileDirCacheModel>
    {
        private const string validUrlPathNamePattern = @"^[A-Za-z0-9\-]{1,}$";
        private const string zeroIdxUrlPathName = "homeless-items";
        public FileDirRepo(
            FCloudContext context,
            ICommitingUserIdProvider userIdProvider
            ) : base(context, userIdProvider)
        {
        }

        public IQueryable<FileDir> QuickSearch(string str)
            => Existing
            .Where(x => x.Name != null && x.Name.Contains(str))
            .OrderByDescending(x => x.Updated);

        public List<string>? GetPathById(int id)
        {
            var chain = GetChainIdsById(id);
            if (chain is null)
                return null;
            return GetRangeByIdsOrdered<string>(chain, dirs =>
            {
                var list = dirs.Select(x => new { x.Id, x.UrlPathName }).ToList();
                return list.ToDictionary(x => x.Id, x => x.UrlPathName ?? "???");
            });
        }
        public List<string>? GetFriendlyPathById(int id)
        {
            var chain = GetChainIdsById(id);
            if (chain is null)
                return null;
            return GetRangeByIdsOrdered<string>(chain, dirs =>
            {
                var list = dirs.Select(x => new { x.Id, x.Name }).ToList();
                return list.ToDictionary(x => x.Id, x => x.Name ?? "???");
            });
        }
        public List<(int id, List<string> nameChain)> GetNameChainsByIds(List<int> ids)
        {
            List<(int id, List<string>)> res = [];
            if (ids.Count == 0)
                return res;
            HashSet<int> relatedIds = [];
            List<(int id, List<int> chain)> chains = [];
            foreach (var id in ids)
            {
                var chain = GetChainIdsById(id);
                if(chain is null) 
                    continue;
                chain.ForEach(i => relatedIds.Add(i));
                chains.Add((id,chain));
            }
            var names = GetRangeByIds(relatedIds.ToList())
                .Select(x => new {x.Id, x.Name}).ToList();
            foreach(var item in chains)
            {
                List<string> nameChain = [];
                foreach(var dirId in item.chain)
                {
                    var name = names.Find(x => x.Id == dirId)?.Name ?? "??";
                    nameChain.Add(name);
                }
                res.Add((item.id, nameChain));
            }
            return res;
        }

        public List<int>? GetChainIdsById(int id)
        {
            if (id == 0)
                return [];
            List<int> res = new();
            var targetId = id;
            while (true)
            {
                var target = base.CachedItemById(targetId);
                if (target is null)
                    return null;
                res.Insert(0, target.Id);
                targetId = target.ParentDir;
                if (targetId == 0)
                    break;
            }
            return res;
        }
        public List<int>? GetChainIdsByPath(string[] path)
            => Existing.GetChainIdsByPath(path);
        public List<FileDir>? GetChainByPath(string[] path)
            => Existing.GetChainByPath(path);
        
        /// <summary>
        /// 将指定目录及其父目录，爷目录直到顶级目录全部更新时间设为现在
        /// </summary>
        /// <param name="id">目录id</param>
        /// <param name="errmsg"></param>
        /// <returns></returns>
        public bool SetUpdateTimeAncestrally(int id, out string? errmsg)
        {
            List<int>? chain = GetChainIdsById(id);
            if (chain is null)
            {
                errmsg = "更新文件夹时间出错：树状结构溯源失败";
                return false;
            }
            UpdateTime(chain);
            errmsg = null;
            return true;
        }
        /// <summary>
        /// 将指定目录及其父目录，爷目录直到顶级目录全部更新时间设为现在
        /// </summary>
        /// <param name="ids">目录id</param>
        /// <param name="errmsg"></param>
        /// <returns></returns>
        public bool SetUpdateTimeRangeAncestrally(List<int> ids, out string? errmsg)
        {
            var needUpdateIds = new List<int>();
            foreach (var d in ids)
            {
                List<int>? chain = GetChainIdsById(d);
                if (chain is null)
                {
                    errmsg = "更新文件夹时间出错：树状结构溯源失败";
                    return false;
                }
                needUpdateIds.AddRange(chain);
            }
            UpdateTime(needUpdateIds);
            errmsg = null;
            return true;
        }


        public IQueryable<FileDir>? GetChildrenById(int id)
            => Existing.Where(x => x.ParentDir == id);

        /// <summary>
        /// 对于刚刚被移动过的目录，更新其所有子孙目录的信息(深度，所属顶级目录等)，以确保匹配目录结构
        /// </summary>
        /// <param name="masters">刚刚被移动过的目录id</param>
        /// <param name="errmsg"></param>
        /// <returns></returns>
        public bool UpdateDescendantsInfoFor(List<int> masters, out string? errmsg)
        {
            var changedData = UpdateDescendantsInfoFor(masters);//计算完成后的结果拿回来更新数据库
            var changedIds = changedData.ConvertAll(x => x.Id);
            var dbModelsNeedMutate = GetRangeByIds(changedIds).ToList();
            foreach(var m in dbModelsNeedMutate)
            {
                var data = changedData.Find(x => x.Id == m.Id);
                if (data is null)
                    continue;
                m.Depth = data.Depth;
                m.RootDir = data.RootDir;
            }
            base.UpdateRange(dbModelsNeedMutate);
            errmsg = null; 
            return true;
        }
        private List<FileDirCacheModel> UpdateDescendantsInfoFor(List<int> masters)
        {
            if (masters is null || masters.Count == 0)
                return [];
            List<FileDirCacheModel> masterData = CachedItemsByIds(masters).ToList();
            List<FileDirCacheModel> masterDataCorrect = new(masterData.Count);
            //用changing记录哪些文件夹发生变化
            List<FileDirCacheModel> changings = [];

            //检查其中顶级目录的属性是否正确，如果不正确则修正并加入“已变化”列表
            foreach(var x in masterData)
            {
                if (x.ParentDir == 0 && (x.RootDir != x.Id || x.Depth != 0))
                {
                    var rootDir = x.Id;//顶级目录的rootDir应该是自己
                    var depth = 0;//顶级目录的depth应该是0
                    var changing = new FileDirCacheModel(x.Id, x.Updated, x.ParentDir, rootDir, depth);
                    changings.Add(changing);
                    masterDataCorrect.Add(changing);
                }
                else
                    masterDataCorrect.Add(x);
            }

            //从父代到子代计算depth
            void setChildrenDepth(FileDirCacheModel dir, int safety)
            {
                if (safety == 50)
                    throw new Exception("未知错误：无穷递归");
                var children = CachedItemsByPred(x => x.ParentDir == dir.Id);
                foreach(var x in children)
                {
                    int shouldBeDepth = dir.Depth + 1;
                    bool depthChanged = x.Depth != shouldBeDepth;
                    int shouldBeRoot = dir.RootDir;
                    bool rootChanged = x.RootDir != shouldBeRoot;
                    if (depthChanged || rootChanged)
                    {
                        var correctX = new FileDirCacheModel(
                            x.Id, x.Updated, x.ParentDir, shouldBeRoot, shouldBeDepth);
                        changings.Add(correctX);
                        setChildrenDepth(correctX, safety + 1);
                    }
                    else
                        setChildrenDepth(x, safety + 1);
                };
            }
            foreach(var m in masterDataCorrect)
                setChildrenDepth(m, 0);

            //此时depth已经是正确值
            //可以再从子代到父代再算总字节数，内容数什么的
            return changings;
        }

        /// <summary>
        /// 针对所有顶级目录执行“更新其所有子孙目录的信息”，确保整个数据库中所有信息匹配目录结构
        /// </summary>
        /// <param name="errmsg"></param>
        /// <returns></returns>
        public bool ManualFixInfoForAll(out string? errmsg)
        {
            var roots = Existing.Where(x => x.ParentDir == 0).Select(x => x.Id).ToList();
            return UpdateDescendantsInfoFor(roots, out errmsg);
        }

        public bool TryRemove(FileDir item, out string? errmsg)
        {
            var userId = _userIdProvider.Get();
            if (userId != item.CreatorUserId)
            {
                errmsg = "只有所有者能删除";
                return false;
            }
            base.Remove(item);
            errmsg = null;
            return true;
        }

        public bool InfoCheck(FileDir item, out string? errmsg)
        {
            errmsg = null;
            if (string.IsNullOrWhiteSpace(item.Name))
            {
                errmsg = "目录名称不能为空";
                return false;
            }
            if (string.IsNullOrWhiteSpace(item.UrlPathName)){
                errmsg = "目录路径名不能为空";
                return false;
            }
            if (item.UrlPathName == zeroIdxUrlPathName)
            {
                errmsg = "请勿使用该路径名";
                return false;
            }
            if (item.Name.Length > FileDir.nameMaxLength)
            {
                errmsg = $"目录名不能超过{FileDir.nameMaxLength}个字符";
                return false;
            }
            if (item.UrlPathName.Length > FileDir.urlPathNameMaxLength)
            {
                errmsg = $"路径名不能超过{FileDir.urlPathNameMaxLength}个字符";
                return false;
            }
            if (!Regex.IsMatch(item.UrlPathName, validUrlPathNamePattern))
            {
                errmsg = "路径名只能有英文字母，数字和\"-\"";
                return false;
            }
            if (item.Depth > 10)
            {
                errmsg = "目录层数过深";
                return false;
            }
            var conflict = Existing.Where(x => x.Id!=item.Id && x.ParentDir == item.ParentDir && x.UrlPathName == item.UrlPathName)
                .Select(x => x.Name).FirstOrDefault();
            if (conflict is not null)
            {
                errmsg = $"冲突：此处已有同样路径名的其他目录【{conflict}】";
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


        protected override IQueryable<FileDirCacheModel> ConvertToCacheModel(IQueryable<FileDir> q)
        {
            return q.Select(x => 
                new FileDirCacheModel(x.Id, x.Updated, x.ParentDir, x.RootDir, x.Depth));
        }
    }

    public class FileDirCacheModel(
        int id, DateTime updated, int parentDir, int rootDir, int depth)
        : CacheModelBase<FileDir>(id, updated)
    {
        public int ParentDir { get; } = parentDir;
        public int RootDir { get; } = rootDir;
        public int Depth { get; } = depth;
    }
}
