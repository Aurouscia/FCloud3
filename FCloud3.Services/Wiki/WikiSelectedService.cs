using FCloud3.Entities.Wiki;
using FCloud3.Repos.Files;
using FCloud3.Repos.Identities;
using FCloud3.Repos.Wiki;
using FCloud3.Services.Files.Storage.Abstractions;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace FCloud3.Services.Wiki
{
    public class WikiSelectedService(
        WikiSelectedRepo wikiSelectedRepo,
        WikiItemRepo wikiItemRepo,
        WikiParaRepo wikiParaRepo,
        FileItemRepo fileItemRepo,
        UserRepo userRepo,
        IStorage storage,
        IConfiguration config)
    {
        private int CoverImgSizeLimit => CoverImgSizeLimitConfig.Value;
        private Lazy<int> CoverImgSizeLimitConfig = new(() =>
        {
            var configVal = config["WikiSelected:CoverImgSizeLimit"];
            if (int.TryParse(configVal, out int val))
                return val;
            return 1000*1000;
        });
        public List<WikiSelectedDto> GetList()
        {
            var items = (
                from w in wikiItemRepo.Existing
                from ws in wikiSelectedRepo.Existing
                from u in userRepo.Existing
                where ws.WikiItemId == w.Id
                where u.Id == ws.CreatorUserId
                orderby ws.Order
                select new WikiSelectedDto()
                {
                    Id = ws.Id,
                    WikiItemId = ws.WikiItemId,
                    Intro = ws.Intro,
                    DropAfterHr = ws.DropAfterHr,
                    CreatorUserId = ws.CreatorUserId,
                    Order = ws.Order,
                    CreatorName = u.Name,
                    WikiTitle = w.Title,
                    Created = ws.Created
                }).ToList();
            var wikiIds = items.ConvertAll(x => x.WikiItemId);
            var sizeLimit = CoverImgSizeLimit;
            var potentialCovers = (
                from f in fileItemRepo.Existing
                from p in wikiParaRepo.Existing
                where wikiIds.Contains(p.WikiItemId)
                where p.Type == WikiParaType.File && f.Id == p.ObjectId
                where f.ByteCount < sizeLimit
                orderby p.Order
                select new
                {
                    p.WikiItemId,
                    p.ObjectId,
                    p.Order,
                    f.StorePathName,
                }).ToList();
            var now = DateTime.Now;
            foreach (var item in items)
            {
                double leftHr = item.DropAfterHr - ((now - item.Created).TotalHours);
                item.LeftHr = (int)Math.Floor(leftHr);
                if (item.LeftHr < 0)
                    item.LeftHr = 0;
                
                var coverStoreName = potentialCovers
                    .Find(c => 
                        c.WikiItemId == item.WikiItemId 
                        && IsImage(c.StorePathName))
                    ?.StorePathName;
                if (coverStoreName is not null)
                    item.CoverUrl = storage.FullUrl(coverStoreName);
            }
            return items;
        }

        public bool Insert(int beforeOrder, WikiSelectedDto model, out string? errmsg)
        {
            return wikiSelectedRepo.TryInsert(
                beforeOrder, model.WikiItemId, model.Intro, model.DropAfterHr, out errmsg);
        }

        public bool Edit(WikiSelectedDto model, out string? errmsg)
        {
            return wikiSelectedRepo.TryEdit(
                model.Id, model.Intro, model.DropAfterHr, out errmsg);
        }

        public bool Remove(int id, out string? errmsg)
        {
            return wikiSelectedRepo.TryRemove(id, out errmsg);
        }

        private static string[] coverExts = [".png", ".jpg", ".jpeg", ".svg"];
        private static bool IsImage(string fileName)
        {
            return coverExts.Any(e => fileName.EndsWith(e, StringComparison.OrdinalIgnoreCase));
        }
    }
    
    public class WikiSelectedDto
    {
        public int Id { get; set; }
        public int WikiItemId { get; set; }
        public string? Intro { get; set; }
        public int DropAfterHr { get; set; }
        public int CreatorUserId { get; set; }
        public int Order { get; set; }
            
        public string? CreatorName { get; set; }
        public string? WikiTitle { get; set; }
        public string? CoverUrl { get; set; }
        public int LeftHr { get; set; }
        [JsonIgnore]
        public DateTime Created { get; set; }
    }
}