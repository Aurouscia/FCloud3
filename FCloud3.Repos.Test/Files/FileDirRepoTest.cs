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
        // dir1--dir2--dir4
        //     \     \
        //      dir3  aaa5
        //          \
        //           aaa6
        #region 测试初始化
        private readonly FileDirRepo _repo;
        private readonly FileDirCaching _caching;
        private readonly FCloudContext _context;
        public FileDirRepoTest() 
        {
            _context = FCloudMemoryContext.Create() as FCloudContext;

            _context.Users.AddRange(new List<User>()
            {
                new() { Name = "user1", Type = UserType.SuperAdmin },
                new() { Name = "user2", Type = UserType.Member }
            });
            _context.SaveChanges();
            
            var dir1 = new FileDir() { Name = "DIR_1", UrlPathName = "dir-1", Depth = 0 };
            var dir2 = new FileDir() { Name = "DIR_2", UrlPathName = "dir-2", Depth = 1, ParentDir = 1 };
            var dir3 = new FileDir() { Name = "DIR_3", UrlPathName = "dir-3", Depth = 1, ParentDir = 1 };
            var dir4 = new FileDir() { Name = "DIR_4", UrlPathName = "dir-4", Depth = 2, ParentDir = 2 };
            var aaa5 = new FileDir() { Name = "AAA", UrlPathName = "aaa", Depth = 2, ParentDir = 2 };
            var aaa6 = new FileDir() { Name = "AAA", UrlPathName = "aaa", Depth = 2, ParentDir = 3 };
            _context.FileDirs.AddRange(dir1, dir2, dir3, dir4, aaa5, aaa6);
            _context.SaveChanges();

            _caching = new FileDirCaching(_context, new FakeLogger<FileDirCaching>());
            _caching.Clear();//每次数据初始化时应该将缓存（是静态内存）清空
            _repo = new(_context, new StubUserIdProvider(2), _caching);
        }
        #endregion

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
        [DataRow("1", "1")]
        [DataRow("2", "1,2")]
        [DataRow("4", "1,2,4")]
        [DataRow("5,6", "1,2,3,5,6")]
        public void SetUpdateTimeAncestrally(string targetIds, string updatedIds)
        {
            var items = _repo.All.ToList();
            var initialTime = new DateTime(1970, 1, 1);
            var shouldGreatThanAfterUpdate = DateTime.Now;
            items.ForEach(i => i.Updated = initialTime);
            _context.SaveChanges();

            var targets = targetIds.Split(',').ToList().ConvertAll(int.Parse);
            var expectUpdate = updatedIds.Split(',').ToList().ConvertAll(int.Parse);
            _repo.SetUpdateTimeRangeAncestrally(targets, out string? errmsg);
            Assert.IsNull(errmsg);

            items = _repo.All.ToList();
            var actualUpdated = items.FindAll(x => x.Updated > shouldGreatThanAfterUpdate).ConvertAll(x => x.Id);
            CollectionAssert.AreEquivalent(expectUpdate, actualUpdated);
        }
        #endregion

        #region 结构变化

        [TestMethod]
        [DataRow("6->5","1|2,3|4,5|6")]
        [DataRow("2->3","1|3|2,6|4,5")]
        [DataRow("6->5  2->3","1|2,3|4,5|6  1|3|2|4,5|6")]
        public void MoveDir(string howMoveList, string expectedDepthsList)
        {
            string[] howMoveArr = howMoveList.Split("  ");
            string[] expectedDepthArr = expectedDepthsList.Split("  ");
            for (int i = 0; i < howMoveArr.Length; i++)
            {
                string howMove = howMoveArr[i];
                string expectedDepths = expectedDepthArr[i];
                string[] howMoveParts = howMove.Split("->");
                int beMoved = int.Parse(howMoveParts[0]);
                int to = int.Parse(howMoveParts[1]);

                var moving = _repo.GetById(beMoved)!;
                var destination = _repo.GetById(to)!;
                moving.ParentDir = to;
                moving.Depth = destination.Depth + 1;
                _repo.TryEdit(moving, out _);

                _repo.UpdateDescendantsInfoFor([beMoved], out var errmsg);
                Assert.IsNull(errmsg);

                List<string> expectedDepthsHere = expectedDepths.Split('|').ToList();
                for (int depth = 0; depth < expectedDepthsHere.Count; depth++)
                {
                    List<int> expectedIdsThisDepth =
                        expectedDepthsHere[depth].Split(',').ToList().ConvertAll(int.Parse);
                    List<int> actualIdsThisDepth =
                        _repo.Existing.Where(x => x.Depth == depth).Select(x => x.Id).ToList();
                    CollectionAssert.AreEquivalent(expectedIdsThisDepth, actualIdsThisDepth);
                }
            }
        }
        #endregion
    }
}
