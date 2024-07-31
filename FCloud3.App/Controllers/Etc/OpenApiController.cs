using FCloud3.App.Services.Filters;
using FCloud3.App.Services.Utils;
using FCloud3.Entities.Files;
using FCloud3.Entities.Identities;
using FCloud3.Entities.Wiki;
using FCloud3.Repos.Files;
using FCloud3.Repos.Identities;
using FCloud3.Repos.Wiki;
using Microsoft.AspNetCore.Mvc;

namespace FCloud3.App.Controllers.Etc
{
    public class OpenApiController(
        WikiItemRepo wikiItemRepo,
        WikiToDirRepo wikiToDirRepo,
        UserRepo userRepo,
        FileDirRepo dirRepo)
        :Controller
    {
        [RateLimited]
        public IActionResult GetWikiInfo(int skip, int take, bool asc)
        {
            skip = Math.Clamp(skip, 0, int.MaxValue);
            take = Math.Clamp(take, 1, 1000);
            IQueryable<WikiItem> q = wikiItemRepo.Existing;
            if (asc)
                q = q.OrderBy(x => x.Id);
            else
                q = q.OrderByDescending(x => x.Id);
            var wikis = 
                q.Select(x => new{
                        x.Id,x.Title,x.OwnerUserId,x.Sealed,x.Updated,x.Created,
                        Dirs = new List<int>()})
                    .Skip(skip)
                    .Take(take)
                    .ToList();
            var wikiIds = wikis.ConvertAll(x => x.Id);
            var wikiToDirs = wikiToDirRepo.Existing
                .Where(x => wikiIds.Contains(x.WikiId))
                .Select(x => new { x.DirId, x.WikiId })
                .ToList();
            foreach (var w in wikis)
            {
                var itsDirs = wikiToDirs
                    .Where(x => x.WikiId == w.Id)
                    .Select(x => x.DirId);
                w.Dirs.AddRange(itsDirs);
            }
            
            var wikisReformed = ListReformer.Run(wikis);
            return this.ApiResp(wikisReformed);
        }
        
        [RateLimited]
        public IActionResult GetUserInfo(int skip, int take, bool asc)
        {
            skip = Math.Clamp(skip, 0, int.MaxValue);
            take = Math.Clamp(take, 1, 1000);
            IQueryable<User> q = userRepo.Existing;
            if (asc)
                q = q.OrderBy(x => x.Id);
            else
                q = q.OrderByDescending(x => x.Id);
            var users = 
                q.Select(x=>new{x.Id,x.Name,x.Updated,x.Created})
                    .Skip(skip)
                    .Take(take)
                    .ToList();
            var reformed = ListReformer.Run(users);
            return this.ApiResp(reformed);
        }
        
        [RateLimited]
        public IActionResult GetDirInfo(int skip, int take, bool asc)
        {
            skip = Math.Clamp(skip, 0, int.MaxValue);
            take = Math.Clamp(take, 1, 1000);
            IQueryable<FileDir> q = dirRepo.Existing;
            if (asc)
                q = q.OrderBy(x => x.Id);
            else
                q = q.OrderByDescending(x => x.Id);
            var dirs = 
                q.Select(x=>new{x.Id,x.Name,x.ParentDir,x.Updated,x.Created})
                    .Skip(skip)
                    .Take(take)
                    .ToList();
            var reformed = ListReformer.Run(dirs);
            return this.ApiResp(reformed);
        }
    }
}