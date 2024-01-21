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
        public WikiToDirRepo(FCloudContext context) : base(context)
        {
        }
        public List<int> GetDirWikiIds(int dirId)
        {
            return Existing.Where(x=>x.DirId==dirId).Select(x=>x.WikiId).ToList();
        }
        public bool AddWikisToDir(List<int> wikiIds,int dirId,out string? errmsg)
        {
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
