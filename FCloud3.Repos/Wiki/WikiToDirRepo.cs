using FCloud3.DbContexts;
using FCloud3.Entities.Wiki;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.Repos.Wiki
{
    public class WikiToDirRepo : RepoBase<WikiToDir>
    {
        public WikiToDirRepo(FCloudContext context, ICommitingUserIdProvider userIdProvider) : base(context, userIdProvider)
        {
        }
        public List<int> GetWikiIdsByDir(int dirId) => 
            Existing.Where(x=>x.DirId == dirId).Select(x=>x.WikiId).Distinct().ToList();
        public List<int> GetWikiIdsByDirs(List<int> dirIds) =>
            Existing.Where(x => dirIds.Contains(x.DirId)).Select(x => x.WikiId).Distinct().ToList();
        public IQueryable<int> GetDirIdsByWikiId(int wikiId) =>
            Existing.Where(x => x.WikiId == wikiId).Select(x => x.DirId);
        public IQueryable<int> GetDirIdsByWikiIds(List<int> wikiIds) =>
            Existing.Where(x => wikiIds.Contains(x.WikiId)).Select(x => x.DirId);
        public bool AddWikisToDir(List<int> wikiIds,int dirId,out string? errmsg)
        {
            if (dirId <= 0)
            {
                errmsg = "不能在此放下词条";
                return false;
            }
            List<WikiToDir> existingRelation = Existing
                .Where(x => x.DirId == dirId)
                .Where(x => wikiIds.Contains(x.WikiId))
                .ToList();
            var needAddForWikiIds = wikiIds.Except(existingRelation.ConvertAll(x => x.WikiId));
            List<WikiToDir> addingRelations = new();
            foreach (var wikiId in needAddForWikiIds)
            {
                WikiToDir relation = new()
                {
                    WikiId = wikiId,
                    DirId = dirId,
                };
                addingRelations.Add(relation);
            }
            return TryAddRange(addingRelations, out errmsg);
        }
        public bool RemoveWikisFromDir(List<int> wikiIds,int dirId,out string? errmsg)
        {
            List<WikiToDir> existingRelation = Existing
                .Where(x => x.DirId == dirId)
                .Where(x => wikiIds.Contains(x.WikiId))
                .ToList();
            return TryRemoveRange(existingRelation, out errmsg);
        }
    }
}
