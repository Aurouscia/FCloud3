using FCloud3.DbContexts;
using FCloud3.Entities.Files;
using FCloud3.Entities.Identities;
using FCloud3.Entities.Wiki;
using FCloud3.Utils.Utils.Cryptography;
using Microsoft.AspNetCore.Mvc;

namespace FCloud3.App.Controllers.Sys
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
        public IActionResult InitUsers()
        {
            bool anyUsers = _context.Users.Any();
            if (!anyUsers)
            {
                User u = new User()
                {
                    Name = "user1",
                    PwdMd5 = MD5Helper.GetMD5Of("123456")
                };
                _context.Users.Add(u);
                _context.SaveChanges();
            }
            return this.ApiResp();
        }
        public IActionResult InitFileDirs()
        {
            bool anyFileDirs = _context.FileDirs.Any();
            if (!anyFileDirs)
            {
                FileDir d1 = new FileDir()
                {
                    Name = "一号文件夹",
                };
                _context.FileDirs.Add(d1);
                _context.SaveChanges();
                FileDir d2 = new FileDir()
                {
                    Name = "二号文件夹",
                };
                _context.FileDirs.Add(d2);
                _context.SaveChanges();

                FileDir d3 = new FileDir()
                {
                    Name = "三号文件夹",
                    ParentDir = d1.Id,
                    Depth = 1
                };
                _context.FileDirs.Add(d3);
                _context.SaveChanges();
                FileDir d4 = new FileDir()
                {
                    Name = "四号文件夹",
                    ParentDir = d1.Id,
                    Depth = 1
                };
                _context.FileDirs.Add(d4);
                _context.SaveChanges();

                FileDir d5 = new FileDir()
                {
                    Name = "五号文件夹",
                    ParentDir = d3.Id,
                    Depth = 2
                };
                _context.FileDirs.Add(d5);
                _context.SaveChanges();
                FileDir d6 = new FileDir()
                {
                    Name = "六号文件夹",
                    ParentDir = d4.Id,
                    Depth = 2
                };
                _context.FileDirs.Add(d6);
                _context.SaveChanges();
            }
            return this.ApiResp();
        }
    }
}
