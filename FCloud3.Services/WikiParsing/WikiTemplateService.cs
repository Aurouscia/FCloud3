using FCloud3.Entities.WikiParsing;
using FCloud3.Repos.Identities;
using FCloud3.Repos.WikiParsing;

namespace FCloud3.Services.WikiParsing
{
    public class WikiTemplateService
    {
        private readonly WikiTemplateRepo _wikiTemplateRepo;
        private readonly UserRepo _userRepo;

        public WikiTemplateService(WikiTemplateRepo wikiTemplateRepo, UserRepo userRepo)
        {
            _wikiTemplateRepo = wikiTemplateRepo;
            _userRepo = userRepo;
        }
        public List<WikiTemplateListItem> GetList(string search)
        {
            if(search.Length > 20)
                search = search[..20];
            var data =
                from t in _wikiTemplateRepo.QuickSearch(search)
                from u in _userRepo.Existing
                where t.CreatorUserId == u.Id
                select new
                {
                    t.Id,
                    t.Name,
                    t.Updated,
                    t.CreatorUserId,
                    CreatorName = u.Name,
                };
            var res = data.ToList().ConvertAll(x => new WikiTemplateListItem
            {
                Id = x.Id,
                Name = x.Name,
                Updated = x.Updated.ToString("yy/MM/dd HH:mm"),
                CreatorUserId = x.CreatorUserId,
                CreatorName = x.CreatorName
            });
            return res;
        }

        public bool Add(string name, out string? errmsg)
        {
            WikiTemplate wt = new()
            {
                Name = name
            };
            return _wikiTemplateRepo.TryAdd(wt, out errmsg);
        }

        public WikiTemplate Edit(int id)
        {
            return _wikiTemplateRepo.GetById(id) ?? throw new Exception("找不到指定模板");
        }

        public bool EditExe(WikiTemplate data, out string? errmsg)
        {
            //仅超级管理员能加入脚本
            return _wikiTemplateRepo.TryEdit(data, out errmsg);
        }

        public bool Remove(int id, out string? errmsg)
        {
            return _wikiTemplateRepo.TryRemove(id, out errmsg);
        }

        public class WikiTemplateListItem
        {
            public int Id { get; set; }
            public string? Name { get; set; }
            public string? Updated { get; set; }
            public int CreatorUserId { get; set; }
            public string? CreatorName { get; set; }
        }
    }
}
