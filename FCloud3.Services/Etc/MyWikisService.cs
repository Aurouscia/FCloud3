using FCloud3.Repos.Files;
using FCloud3.Repos.Wiki;
using Newtonsoft.Json;

namespace FCloud3.Services.Etc
{
    public class MyWikisService(
        WikiItemRepo wikiItemRepo,
        WikiToDirRepo wikiToDirRepo,
        FileDirRepo fileDirRepo)
    {
        public MyWikisOverallResp MyWikiOverall(int uid)
        {
            var myAllWs = wikiItemRepo
                .CachedItemsByPred(x => x.OwnerId == uid)
                .ToList();
            var allWIds = myAllWs.Select(x => x.Id).ToHashSet();
            var wikiToDirs = wikiToDirRepo.Existing
                .Select(x => new {x.WikiId, x.DirId})
                .ToList();
            wikiToDirs.RemoveAll(x => !allWIds.Contains(x.WikiId));
            List<MyWikisInDir> flatCollection = [];
            List<string?[]> homelessWikis = [];
            List<string?[]> sealedWikis = [];
            
            //找到所有该用户词条所在的目录，无归属的统一放起来
            foreach (var w in myAllWs)
            {
                if(w.Sealed)
                    sealedWikis.Add([w.Title, w.UrlPathName]);
                var relas = wikiToDirs.FindAll(x => x.WikiId == w.Id);
                if(relas.Count == 0)
                    homelessWikis.Add([w.Title, w.UrlPathName]);
                foreach (var r in relas)
                {
                    var dirExisting = flatCollection.Find(x => x.Id == r.DirId);
                    if (dirExisting is null)
                    {
                        var dir = fileDirRepo.CachedItemById(r.DirId);
                        if (dir is null)
                            continue;
                        var dirNew = new MyWikisInDir()
                        {
                            Id = dir.Id,
                            ParentId = dir.ParentDir,
                            Wikis = [[w.Title, w.UrlPathName]]
                        };
                        flatCollection.Add(dirNew);
                    }
                    else
                    {
                        dirExisting.Wikis.Add([w.Title, w.UrlPathName]);
                    }
                }
            }
            
            //树状找到所有祖宗目录
            Stack<MyWikisInDir> addingParent = new(flatCollection);
            int safety = fileDirRepo.CachedItemsCount();//死循环安全措施
            while (addingParent.Count > 0 && safety >= 0)
            {
                safety--;//死循环安全措施
                
                var target = addingParent.Pop();
                if(target.ParentId > 0 && !flatCollection.Any(mw => mw.Id == target.ParentId))
                {
                    var parent = fileDirRepo.CachedItemById(target.ParentId);
                    if (parent is { })
                    {
                        var adding = new MyWikisInDir()
                        {
                            Id = parent.Id,
                            ParentId = parent.ParentDir
                        };
                        flatCollection.Add(adding);
                        if(adding.ParentId > 0)
                            addingParent.Push(adding);
                    }
                };
            }
            
            //设置搜索和设置目录名
            var allRelatedDirIds = flatCollection.ConvertAll(x => x.Id);
            var dirNames = fileDirRepo
                .GetRangeByIds(allRelatedDirIds)
                .Select(x=>new{x.Id, x.Name})
                .ToList();
            flatCollection.ForEach(mw =>
            {
                var name = dirNames.Find(x => x.Id == mw.Id);
                mw.Name = name?.Name;
            });

            //设置子代目录
            var roots = new List<MyWikisInDir>();
            foreach (var mw in flatCollection)
            {
                var itsChildren = flatCollection.FindAll(x => x.ParentId == mw.Id);
                mw.Dirs = itsChildren;
                if(mw.ParentId == 0)
                    roots.Add(mw);
            }
            var treeView = new MyWikisInDir()
            {
                Id = 0,
                Name = "所有词条",
                Dirs = roots
            };
            treeView.CalculateCount();
            return new MyWikisOverallResp()
            {
                HomelessWikis = homelessWikis,
                SealedWikis = sealedWikis,
                TreeView = treeView
            };
        }

        public class MyWikisOverallResp
        {
            public MyWikisInDir? TreeView { get; set; }
            public List<string?[]>? HomelessWikis { get; set; }
            public List<string?[]>? SealedWikis { get; set; }
        }
        public class MyWikisInDir
        {
            public int Id { get; set; }
            [JsonIgnore]
            public int ParentId { get; set; }
            public string? Name { get; set; }
            public int Count { get; set; }
            public List<string?[]> Wikis { get; set; } = [];
            public List<MyWikisInDir>? Dirs { get; set; }

            public int CalculateCount()
            {
                int count = Wikis.Count;
                if (Dirs is null || Dirs.Count == 0)
                {
                    Count = count;
                    return count;
                }
                var childrenCount = Dirs.ConvertAll(x => x.CalculateCount()).Sum();
                count += childrenCount;
                Count = count;
                return count;
            }
        }
    }
}