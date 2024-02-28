using FCloud3.App.Services;
using FCloud3.App.Services.Utils;
using FCloud3.DbContexts;
using FCloud3.Entities.Files;
using FCloud3.Entities.Identities;
using FCloud3.Entities.Wiki;
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
            bool anyGroups = _context.UserGroups.Any();
            if (!anyUsers)
            {
                User u1 = new User()
                {
                    Name = "user1",
                    PwdEncrypted = new UserPwdEncryption().Run("user1")
                };
                User u2 = new User()
                {
                    Name = "user2",
                    PwdEncrypted = new UserPwdEncryption().Run("user2")
                };
                User u3 = new User()
                {
                    Name = "user3",
                    PwdEncrypted = new UserPwdEncryption().Run("user3")
                };
                _context.Users.AddRange(u1,u2,u3);
                _context.SaveChanges();
                UserGroup g1 = new UserGroup()
                {
                    Name = "群组1",
                    OwnerUserId = u1.Id,
                };
                UserGroup g2 = new UserGroup()
                {
                    Name = "群组2",
                    OwnerUserId = u2.Id,
                };
                _context.UserGroups.AddRange(g1, g2);
                _context.SaveChanges();
                UserToGroup r1 = new UserToGroup()
                {
                    GroupId = g1.Id,
                    UserId = u1.Id
                };
                UserToGroup r2 = new UserToGroup()
                {
                    GroupId = g2.Id,
                    UserId = u2.Id
                };
                _context.UserToGroups.AddRange(r1, r2);
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
