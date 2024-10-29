using FCloud3.Entities.Wiki;
using FCloud3.Repos.Files;
using FCloud3.Repos.Wiki;
using FCloud3.Services.Files.Storage.Abstractions;

namespace FCloud3.Services.Wiki.Support
{
    public class WikiAlbumService(
        WikiParaRepo wikiParaRepo,
        FileItemRepo fileItemRepo,
        IStorage storage)
    {
        public WikiAlbumResult Get(int wikiId)
        {
            var fileParas = (
                from p in wikiParaRepo.Existing
                from f in fileItemRepo.Existing
                where p.WikiItemId == wikiId
                where p.Type == WikiParaType.File
                where p.ObjectId == f.Id
                select new
                {
                    FileItemId = f.Id,
                    StorePathName = f.StorePathName
                }).ToList();
            var res = new WikiAlbumResult();
            fileParas.ForEach(f =>
            {
                string src = storage.FullUrl(f.StorePathName);
                res.Items.Add(new(f.FileItemId, src));
            });
            return res;
        }
        public class WikiAlbumResult
        {
            public List<WikiAlbumItem> Items = []; 
            public class WikiAlbumItem(int fileItemId, string src, string? wikiPathName = null)
            {
                public string? WPathName { get; set; } = wikiPathName;
                public int FileItemId { get; set; } = fileItemId;
                public string Src { get; set; } = src;
            }
        }
    }
}