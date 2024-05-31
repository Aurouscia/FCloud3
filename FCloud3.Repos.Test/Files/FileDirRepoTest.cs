using FCloud3.DbContexts;
using FCloud3.DbContexts.DbSpecific;
using FCloud3.Entities.Files;
using FCloud3.Entities.Identities;
using FCloud3.Repos.Etc.Caching;
using FCloud3.Repos.Files;
using FCloud3.Repos.Test.TestSupport;

namespace FCloud3.Repos.Test.Files
{
    [TestClass]
    public class FileDirRepoTest
    {
        private readonly FileDirRepo _repo;
        public FileDirRepoTest() 
        {
            var context = FCloudMemoryContext.Create() as FCloudContext;

            context.Users.AddRange(new List<User>()
            {
                new() { Name = "user1", Type = UserType.SuperAdmin },
                new() { Name = "user2", Type = UserType.Member }
            });
            context.SaveChanges();

            // dir1--dir2--dir4
            //     \     \
            //      dir3  aaa
            //          \
            //           aaa
            var dir1 = new FileDir() { Name = "DIR_1", UrlPathName = "dir-1", Depth = 0 };
            var dir2 = new FileDir() { Name = "DIR_2", UrlPathName = "dir-2", Depth = 1, ParentDir = 1 };
            var dir3 = new FileDir() { Name = "DIR_3", UrlPathName = "dir-3", Depth = 1, ParentDir = 1 };
            var dir4 = new FileDir() { Name = "DIR_4", UrlPathName = "dir-4", Depth = 2, ParentDir = 2 };
            var aaa5 = new FileDir() { Name = "AAA", UrlPathName = "aaa", Depth = 2, ParentDir = 2 };
            var aaa6 = new FileDir() { Name = "AAA", UrlPathName = "aaa", Depth = 2, ParentDir = 3 };
            context.FileDirs.AddRange(dir1, dir2, dir3, dir4, aaa5, aaa6);
            context.SaveChanges();

            var caching = new FileDirCaching(context, new FakeLogger<FileDirCaching>());
            _repo = new(context, new StubUserIdProvider(2), caching);
        }


        #region 关于路径的查询
        [TestMethod]
        [DataRow("", "")]
        [DataRow("dir-1/dir-2", "1,2")]
        [DataRow("dir-1/dir-3", "1,3")]
        [DataRow("dir-1/dir-2/aaa", "1,2,5")]
        [DataRow("dir-1/dir-3/aaa", "1,3,6")]
        [DataRow("dir-x", null)]
        [DataRow("dir-1/dir-x", null)]
        [DataRow("dir-1/dir-x/aaa", null)]
        public void GetChainByPath(string path, string? ids)
        {
            var pathParts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var res1 = _repo.GetChainIdsByPath(pathParts);
            var res2 = _repo.GetChainByPath(pathParts);

            if (ids is not null)
            {
                Assert.IsNotNull(res2);
                var idsActual = ids.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList().ConvertAll(int.Parse);
                CollectionAssert.AreEqual(idsActual, res1);
                var resPathNames = res2.ConvertAll(x => x.UrlPathName);
                var resIds = res2.ConvertAll(x => x.Id);
                CollectionAssert.AreEqual(pathParts, resPathNames);
                CollectionAssert.AreEqual(idsActual, resIds);
            }
            else
            {
                Assert.IsNull(res1);
            }
        }

        [TestMethod]
        [DataRow(1, "1", "dir-1")]
        [DataRow(2, "1,2", "dir-1/dir-2")]
        [DataRow(3, "1,3", "dir-1/dir-3")]
        [DataRow(5, "1,2,5", "dir-1/dir-2/aaa")]
        [DataRow(6, "1,3,6", "dir-1/dir-3/aaa")]
        [DataRow(1000, null, null)]
        public void GetChainById(int id, string? ids, string? path)
        {
            var res = _repo.GetChainIdsById(id);
            var pathRes = _repo.GetPathById(id);
            if (ids is not null && path is not null)
            {
                var idActual = ids.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList().ConvertAll(int.Parse);
                var pathActual = path.Split('/');
                CollectionAssert.AreEqual(idActual, res);
                CollectionAssert.AreEqual(pathActual, pathRes);
            }
            else
            {
                Assert.IsNull(res);
                Assert.IsNull(pathRes);
            }
        }
        #endregion

        #region 级联设置更新时间
        [TestMethod]
        [DataRow(1, "1")]
        [DataRow(2, "1,2")]
        [DataRow(3, "1,2,3")]
        public void SetUpdateTimeAncestrally(int id, string updatedIds)
        {
            
        }
        #endregion
    }
}
