using FCloud3.Entities.Wiki;
using FCloud3.Repos.Files;
using FCloud3.Repos.Identities;
using FCloud3.Repos.Wiki;

namespace FCloud3.Services.Etc.Split
{
    public class SplitService(
        WikiItemRepo wikiItemRepo,
        WikiToDirRepo wikiToDirRepo,
        FileDirRepo fileDirRepo,
        UserRepo userRepo)
    {
        public List<AllWikiInDirReportItem> AllWikiInDirReport(int dirId)
        {
            List<WikiToDir> wikiToDirs = wikiToDirRepo.Existing.ToList();
            var allWInfo = AllWikiInDir(wikiToDirs, dirId);
            var wIdsList = allWInfo.wIds.ToList();
            var us = userRepo.Existing.Select(x => new { x.Id, x.Name }).ToList();
            var ws = wikiItemRepo
                .GetRangeByIds(wIdsList)
                .Select(x => new { x.Id, x.Title, x.OwnerUserId }).ToList();
            var res = ws.ConvertAll(w =>
            {
                var title = w.Title ?? "??";
                var owner = us.Find(u => u.Id == w.OwnerUserId)?.Name ?? "??";
                var existsInOtherDir = wikiToDirs
                    .Where(x => x.WikiId == w.Id)
                    .Select(x => x.DirId)
                    .Except(allWInfo.dirIds).Any();
                return new AllWikiInDirReportItem(title, owner, existsInOtherDir);
            });
            return res;
        }
        private (HashSet<int> wIds, HashSet<int> dirIds) AllWikiInDir(List<WikiToDir> wikiToDirs, int dirId)
        {
            List<SimpleDir> dirs = fileDirRepo.Existing
                .Select(x => new SimpleDir(x.Id, x.ParentDir)).ToList();
            HashSet<int> wikiIds = [];
            HashSet<int> dirIds = [];
            AllWikiInDir(wikiIds, dirIds, wikiToDirs, dirs, dirId);
            return (wikiIds, dirIds);
        }
        private void AllWikiInDir(HashSet<int> wIds, HashSet<int> dIds,
            List<WikiToDir> wikiToDirs, List<SimpleDir> dirs, int dirId)
        {
            var wHere = wikiToDirs
                .Where(x => x.DirId == dirId)
                .Select(x => x.WikiId);
            foreach (var w in wHere)
                wIds.Add(w);
            dIds.Add(dirId);
            
            var subdirIds = dirs
                .Where(x => x.ParentId == dirId)
                .Select(x => x.Id);
            foreach (var subdirId in subdirIds)
            {
                AllWikiInDir(wIds, dIds, wikiToDirs, dirs, subdirId);
            }
        }
        private readonly struct SimpleDir(int id, int parentId)
        {
            public int Id { get; } = id;
            public int ParentId { get; } = parentId;
        }
    }

    public class AllWikiInDirReportItem(string title, string owner, bool existsInOtherDir)
    {
        public string Title { get; set; } = title;
        public string Owner { get; set; } = owner;
        public bool ExistsInOtherDir { get; set; } = existsInOtherDir;
    }
}