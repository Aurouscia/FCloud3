using FCloud3.Repos.Models.Corr;
using FCloud3.Repos.Models.Wiki;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCloud3.Services.Wiki
{
    public class WikiService
    {
        private readonly WikiItemRepo _wikiRepo;
        private readonly CorrRepo _corrRepo;
        public const int maxWikiTitleLength = 30;
        public WikiService(WikiItemRepo wikiRepo, CorrRepo corrRepo)
        {
            _wikiRepo = wikiRepo;
            _corrRepo = corrRepo;
        }
        public bool TryAdd(int creator,string? title, out string? errmsg)
        {
            if(string.IsNullOrEmpty(title))
            {
                errmsg = "标题不能为空";
                return false;
            }
            if (title.Length > maxWikiTitleLength)
            {
                errmsg = $"标题不能超过{maxWikiTitleLength}字";
                return false;
            }
            WikiItem w = new()
            {
                CreatorUserId = creator,
                OwnerUserId = creator,
                Title = title
            };
            if (!_wikiRepo.TryAdd(w,out errmsg))
            {
                return false;
            }
            return true;
        }
        public bool TryEdit()
        {
            throw new NotImplementedException();
        }
    }
}
