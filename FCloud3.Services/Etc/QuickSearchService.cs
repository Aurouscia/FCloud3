﻿using FCloud3.Entities.Wiki;
using FCloud3.Repos.Files;
using FCloud3.Repos.Identities;
using FCloud3.Repos.Table;
using FCloud3.Repos.TextSec;
using FCloud3.Repos.Wiki;
using FCloud3.Services.Files.Storage.Abstractions;

namespace FCloud3.Services.Etc
{
    public class QuickSearchService(
        WikiItemRepo wikiItemRepo,
        WikiParaRepo wikiParaRepo,
        UserRepo userRepo,
        UserGroupRepo userGroupRepo,
        FileItemRepo fileItemRepo,
        MaterialRepo materialRepo,
        FileDirRepo fileDirRepo,
        WikiToDirRepo wikiToDirRepo,
        FreeTableRepo freeTableRepo,
        TextSectionRepo textSectionRepo,
        IOperatingUserIdProvider userIdProvider,
        IStorage storage)
    {
        private const int maxCount = 8;

        public QuickSearchResult SearchWikiItem(string str, int excludeDir, bool isAdmin)
        {
            var uid = userIdProvider.Get();
            var q = wikiItemRepo.QuickSearch(str, isAdmin, uid);
            if(excludeDir > 0)
            {
                var excludeWikis = wikiToDirRepo.Existing
                    .Where(x => x.DirId == excludeDir)
                    .Select(x => x.WikiId);
                q = q.Where(w => !excludeWikis.Contains(w.Id));
            }
            var items = q.Select(x => new { x.Title, x.UrlPathName, x.Id }).Take(maxCount).ToList();
            QuickSearchResult res = new();
            items.ForEach(x =>
            {
                res.Items.Add(new(x.Title ?? "N/A", x.UrlPathName, x.Id));
            });
            return res;
        }
        public QuickSearchResult SearchUser(string str)
        {
            var q = userRepo.QuickSearch(str);
            var items = q.Select(x => new { x.Name, x.Id }).Take(maxCount).ToList();
            QuickSearchResult res = new();
            items.ForEach(x =>
            {
                res.Items.Add(new(x.Name ?? "N/A", null, x.Id));
            });
            return res;
        }
        public QuickSearchResult SearchUserGroup(string str)
        {
            var q = userGroupRepo.QuickSearch(str);
            var items = q.Select(x => new { x.Name, x.Id }).Take(maxCount).ToList();
            QuickSearchResult res = new();
            items.ForEach(x =>
            {
                res.Items.Add(new(x.Name ?? "N/A", null, x.Id));
            });
            return res;
        }
        public QuickSearchResult SearchFileItem(string str)
        {
            var q = fileItemRepo.QuickSearch(str);
            var items = q.Select(x => new { x.Id, x.DisplayName, x.StorePathName }).Take(maxCount).ToList();
            QuickSearchResult res = new();
            items.ForEach(x =>
            {
                res.Items.Add(new(x.DisplayName ?? "N/A", x.StorePathName, x.Id));
            });
            return res;
        }
        public QuickSearchResult SearchMaterial(string str)
        {
            var q = materialRepo.QuickSearch(str);
            var items = q.Select(x => new { x.Id, x.Name, x.StorePathName }).Take(maxCount).ToList();
            QuickSearchResult res = new(true);
            items.ForEach(x =>
            {
                res.Items.Add(new(x.Name ?? "N/A", storage.FullUrl(x.StorePathName ?? "??"), x.Id));
            });
            return res;
        }
        public QuickSearchResult SearchFileDir(string str)
        {
            var dirs = fileDirRepo.QuickSearch(str).Select(x => new { x.Id, x.Name }).Take(maxCount).ToList();
            var nameChains = fileDirRepo.GetNameChainsByIds(dirs.ConvertAll(x => x.Id));
            QuickSearchResult res = new();
            nameChains.ForEach(x =>
            {
                var last = x.nameChain.LastOrDefault();
                if (last is null)
                    return;
                var exceptLast = x.nameChain[..^1];
                var path = string.Join('/', exceptLast);
                res.Items.Add(new(last, path, x.id));
            });
            return res;
        }
        public QuickSearchResult SearchCopyableTextSection(string str)
        {
            var q = textSectionRepo.SearchCopyableName(str);
            var items = (
                from w in wikiItemRepo.Existing
                from p in wikiParaRepo.WithType(WikiParaType.Text)
                from t in q
                where p.WikiItemId == w.Id && p.ObjectId == t.Id
                select t)
                .OrderBy(x => x.Title)
                .Select(x => new {x.Id, x.Title})
                .Take(maxCount)
                .ToList();
            HashSet<int> addedIds = [];
            QuickSearchResult res = new();
            items.ForEach(x =>
            {
                if(addedIds.Contains(x.Id))
                    return;
                res.Items.Add(new(x.Title ?? "N/A", null, x.Id));
                addedIds.Add(x.Id);
            });
            return res;
        }
        public QuickSearchResult SearchCopyableFreeTable(string str)
        {
            var q = freeTableRepo.SearchCopyableName(str);
            var items = (
                from w in wikiItemRepo.Existing
                from p in wikiParaRepo.WithType(WikiParaType.Table)
                from t in q
                where p.WikiItemId == w.Id && p.ObjectId == t.Id
                select t)
                .OrderBy(x => x.Name)
                .Select(x => new { x.Id, x.Name })
                .Take(maxCount)
                .ToList();
            HashSet<int> addedIds = [];
            QuickSearchResult res = new();
            items.ForEach(x =>
            {
                if (addedIds.Contains(x.Id))
                    return;
                res.Items.Add(new(x.Name ?? "N/A", null, x.Id));
                addedIds.Add(x.Id);
            });
            return res;
        }
    }

    public class QuickSearchResult
    {
        public List<QuickSearchResultItem> Items { get; set; }
        public bool DescIsSrc { get; set; }
        public QuickSearchResult(bool descIsSrc = false)
        {
            Items = new();
            DescIsSrc = descIsSrc;
        }
        public class QuickSearchResultItem
        {
            public string Name { get; set; }
            public string? Desc { get; set; }
            public int Id { get; set; }
            public QuickSearchResultItem(string name, string? desc, int id)
            {
                Name = name;
                Desc = desc;
                Id = id;
            }
        }
    }
}
