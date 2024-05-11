using FCloud3.App.Services;
using FCloud3.App.Services.Utils;
using FCloud3.DbContexts;
using FCloud3.Entities.Files;
using FCloud3.Entities.Identities;
using FCloud3.Entities.Wiki;
using FCloud3.Services.Etc.TempData.Context;
using FCloud3.Services.Wiki;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FCloud3.App.Controllers.Sys
{
    [Route("Init/{code}/[action]")]
    public class InitController:Controller
    {
        private readonly FCloudContext _context;
        private readonly TempDataContext _tempDataContext;
        private readonly WikiTitleContainService _wikiTitleContainService;
        private readonly static object migrateLockObj = new();
        public InitController(
            FCloudContext context,
            TempDataContext tempDataContext,
            WikiTitleContainService wikiTitleContainService,
            IConfiguration config,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _tempDataContext = tempDataContext;
            _wikiTitleContainService = wikiTitleContainService;

            var code = httpContextAccessor.HttpContext?.Request.RouteValues["code"]?.ToString();
            if (string.IsNullOrWhiteSpace(code))
                throw new Exception("请输入配置文件中的MasterAdminCode");
            var trueCode = config["MasterAdminCode"] ?? throw new Exception("配置文件中MasterAdminCode未填");
            if (trueCode != code)
                throw new Exception("MasterAdminCode错误");
        }
        public IActionResult InitDb()
        {
            lock (migrateLockObj)
            {
                _context.Database.Migrate();
                _tempDataContext.Database.Migrate();
                return this.ApiResp("已Migrate成功", true);
            }
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
            if (!anyUsers && !anyGroups)
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
                FileDir d2 = new FileDir()
                {
                    Name = "二号文件夹",
                };
                _context.FileDirs.AddRange(d1, d2);
                _context.SaveChanges();
            }
            return this.ApiResp();
        }

        /// <summary>
        /// 【⚠警告】该操作对内存和数据库均有极大负担，请谨慎操作
        /// </summary>
        /// <returns></returns>
        public IActionResult WikiTitleContainAutoFill()
        {
            var existingContainsForText = _context.WikiTitleContains
                .Where(x => x.Type == WikiTitleContainType.TextSection)
                .Select(x => x.ObjectId);
            var existingContainsForTable = _context.WikiTitleContains
                .Where(x => x.Type == WikiTitleContainType.FreeTable)
                .Select(x => x.ObjectId);

            var allTexts = _context.TextSections
                .Where(x => !x.Deleted)
                .Where(x => !existingContainsForText.Contains(x.Id))
                .Select(x => new { x.Id, x.Content }).ToList();
            var allTables = _context.FreeTables
                .Where(x => !x.Deleted)
                .Where(x => !existingContainsForTable.Contains(x.Id))
                .Select(x => new { x.Id, x.Data }).ToList();

            List<WikiTitleContain> newContains = [];
            allTexts.ForEach(x =>
            {
                var res = _wikiTitleContainService.AutoFill(x.Id, WikiTitleContainType.TextSection, x.Content ?? "");
                res.Items.ForEach(r => {
                    newContains.Add(new()
                    {
                        ObjectId = x.Id,
                        Type = WikiTitleContainType.TextSection,
                        WikiId = r.Id
                    });
                });
            });
            allTables.ForEach(x =>
            {
                var res = _wikiTitleContainService.AutoFill(x.Id, WikiTitleContainType.FreeTable, x.Data ?? "");
                res.Items.ForEach(r => {
                    newContains.Add(new()
                    {
                        ObjectId = x.Id,
                        Type = WikiTitleContainType.FreeTable,
                        WikiId = r.Id
                    });
                });
            });
            _context.WikiTitleContains.AddRange(newContains);
            _context.SaveChanges();
            return this.ApiResp("已完成", true);
        }
    }
}
