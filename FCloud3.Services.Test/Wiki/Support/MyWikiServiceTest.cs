using FCloud3.DbContexts;
using FCloud3.DbContexts.DbSpecific;
using FCloud3.Entities.Files;
using FCloud3.Entities.Wiki;
using FCloud3.Repos.Etc.Caching;
using FCloud3.Repos.Etc.Caching.Abstraction;
using FCloud3.Repos.Files;
using FCloud3.Repos.Wiki;
using FCloud3.Services.Test.TestSupport;
using FCloud3.Services.Wiki.Support;

namespace FCloud3.Services.Test.Wiki.Support
{
    [TestClass]
    public class MyWikiServiceTest
    {
        private readonly FCloudContext _ctx;
        private readonly MyWikisService _myWikisService;

        public MyWikiServiceTest()
        {
            int uid = 0;
            var userIdProvider = new StubUserIdProvider(uid);
            _ctx = FCloudMemoryContext.Create();
            WikiItemCaching wikiItemCaching = new(_ctx, new FakeLogger<CachingBase<WikiItemCachingModel, WikiItem>>());
            WikiItemRepo wikiItemRepo = new(_ctx, userIdProvider, wikiItemCaching);
            FileDirCaching fileDirCaching = new(_ctx, new FakeLogger<FileDirCaching>());
            WikiToDirRepo wikiToDirRepo = new(_ctx, userIdProvider);
            FileDirRepo fileDirRepo = new(_ctx, userIdProvider, fileDirCaching);
            wikiItemCaching.Clear();
            fileDirCaching.Clear();
            _myWikisService = new MyWikisService(wikiItemRepo, fileDirCaching, wikiToDirRepo, fileDirRepo);
            
            // 1
            // 2-3-5
            //  \
            //   4
            //
            // 1: 1
            // 2: 2
            // 3: 3,4
            // 5: 1
            _ctx.FileDirs.AddRange(new List<FileDir>(){
                new() //1
                {
                    Name = "1号文件夹",
                    ParentDir = 0,
                },
                new() //2
                {
                    Name = "2号文件夹",
                    ParentDir = 0,
                },
                new() //3
                {
                    Name = "3号文件夹",
                    ParentDir = 2,
                },                
                new() //4
                {
                    Name = "4号文件夹",
                    ParentDir = 2,
                },
                new() //5
                {
                    Name = "5号文件夹",
                    ParentDir = 3,
                },
            });
            _ctx.WikiItems.AddRange(new List<WikiItem>(){
                new() //1
                {
                    Title = "1号词条",
                    UrlPathName = "wiki-1"
                },
                new() //2
                {
                    Title = "2号词条",
                    UrlPathName = "wiki-2"
                },
                new() //3
                {
                    Title = "3号词条",
                    UrlPathName = "wiki-3"
                },
                new() //4
                {
                    Title = "4号词条",
                    UrlPathName = "wiki-4"
                },
                new() //5
                {
                    Title = "5号词条",
                    UrlPathName = "wiki-5"
                },
                new() //6
                {
                    Title = "6号词条",
                    UrlPathName = "wiki-6"
                },
            });
            _ctx.WikiToDirs.AddRange(new List<WikiToDir>(){
                new WikiToDir()
                {
                    DirId = 1,
                    WikiId = 1
                },
                new WikiToDir()
                {
                    DirId = 5,
                    WikiId = 1
                },
                new WikiToDir()
                {
                    DirId = 2,
                    WikiId = 2
                },
                new WikiToDir()
                {
                    DirId = 3,
                    WikiId = 3
                },
                new WikiToDir()
                {
                    DirId = 3,
                    WikiId = 4
                },
            });
            _ctx.SaveChanges();
        }

        [TestMethod]
        public void Test()
        {
            var res = _myWikisService.MyWikiDetail(0);
            Assert.IsNotNull(res.HomelessWikis);
            WikiListSame([["5号词条","wiki-5"],["6号词条","wiki-6"]], res.HomelessWikis);
            Assert.IsNotNull(res.TreeView);
            var expected = new MyWikisService.MyWikisInDir()
            {
                Id = 0,
                Name = "所有词条",
                Count = 5,
                Dirs = new()
                {
                    new()
                    {
                        Id = 1,
                        Name = "1号文件夹",
                        Wikis = [["1号词条", "wiki-1"]],
                        Count = 1,
                        Dirs = []
                    },
                    new()
                    {
                        Id = 2,
                        Name = "2号文件夹",
                        Wikis = [["2号词条", "wiki-2"]],
                        Count = 4,
                        Dirs =
                        [
                            new()
                            {
                                Id = 3,
                                Name = "3号文件夹",
                                Wikis = [["3号词条", "wiki-3"], ["4号词条", "wiki-4"]],
                                Count = 3,
                                Dirs =
                                [
                                    new()
                                    {
                                        Id = 5,
                                        Name = "5号文件夹",
                                        Wikis = [["1号词条", "wiki-1"]],
                                        Count = 1,
                                        Dirs = []
                                    }
                                ]
                            }
                        ]
                    }
                }
            };
            DirSame(expected, res.TreeView);
        }

        private void DirSame(MyWikisService.MyWikisInDir a, MyWikisService.MyWikisInDir b)
        {
            if (a.Id != b.Id || a.Name != b.Name)
                throw new Exception("id或名称不对");
            if (a.Count != b.Count)
                throw new Exception("内部总词条数量不对");
            if (a.Wikis.Count != b.Wikis.Count)
                throw new Exception("直接归属词条数量不对");
            WikiListSame(a.Wikis, b.Wikis);
            if (a.Dirs is null)
                Assert.IsNull(b.Dirs);
            else
            {
                Assert.IsNotNull(b.Dirs);
                a.Dirs.Sort((x, y) => x.Id - y.Id);
                b.Dirs.Sort((x, y) => x.Id - y.Id);
                for (int i = 0; i < a.Dirs.Count; i++)
                {
                    var aitem = a.Dirs[i];
                    var bitem = b.Dirs[i];
                    DirSame(aitem, bitem);
                }
            }
        }

        private void WikiListSame(List<string?[]> a, List<string?[]> b)
        {
            var comp = new WikiSameCompare();
            var diff = a.Except(b, comp);
            if (diff.Count() > 0)
                throw new Exception("a有b中没有的元素");
            diff = b.Except(a, comp);
            if (diff.Count() > 0)
                throw new Exception("b有a中没有的元素");
        }

        private class WikiSameCompare : IEqualityComparer<string?[]>
        {
            public bool Equals(string?[]? x, string?[]? y)
            {
                return x![0] == y![0] && x![1] == y![1];
            }

            public int GetHashCode(string?[] obj)
            {
                return (obj[0] ?? "").GetHashCode() + (obj[1] ?? "").GetHashCode();
            }
        }
    }
}