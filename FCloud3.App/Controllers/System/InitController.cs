using FCloud3.DbContexts;
using FCloud3.Entities.Wiki;
using Microsoft.AspNetCore.Mvc;

namespace FCloud3.App.Controllers.System
{
    public class InitController:Controller
    {
        private readonly FCloudContext _context;
        public InitController(FCloudContext context)
        {
            _context = context;
        }
        public IActionResult InitWikis()
        {
            bool anyWikis = _context.WikiItems.Any();
            bool anyWikiParas = _context.WikiParas.Any();
            if(!anyWikis && !anyWikiParas)
            {
                WikiItem item = new WikiItem()
                {
                    Title = "测试词条1"
                };
                _context.WikiItems.Add(item);
                _context.SaveChanges();
                WikiPara p1 = new WikiPara()
                {
                    WikiItemId = item.Id,
                    Type = WikiParaType.Text,
                    Order = 0,
                };
                WikiPara p2 = new WikiPara()
                {
                    WikiItemId = item.Id,
                    Type = WikiParaType.Text,
                    Order = 1,
                };
                _context.WikiParas.AddRange(p1, p2);
                _context.SaveChanges();
            }
            return this.ApiResp();
        }
    }
}
