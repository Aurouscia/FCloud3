using FCloud3.DbContexts;
using FCloud3.DbContexts.DbSpecific;
using FCloud3.Entities.Files;
using FCloud3.Entities.Identities;
using FCloud3.Repos.Files;
using FCloud3.Repos.Sys;
using FCloud3.Repos.Test.TestSupport;
using Microsoft.EntityFrameworkCore;
using NPOI.OpenXmlFormats.Spreadsheet;

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

            var someTime = new DateTime(2024, 1, 6);
            var dir1 = new FileDir() { Name = "DIR_1", UrlPathName = "dir-1", Depth = 0, RootDir = 1, Updated = someTime };
            var dir2 = new FileDir() { Name = "DIR_2", UrlPathName = "dir-2", Depth = 1, ParentDir = 1, RootDir = 1, Updated = someTime };
            var dir3 = new FileDir() { Name = "DIR_3", UrlPathName = "dir-3", Depth = 1, ParentDir = 1, RootDir = 1, Updated = someTime };
            var dir4 = new FileDir() { Name = "DIR_4", UrlPathName = "dir-4", Depth = 2, ParentDir = 2, RootDir = 1, Updated = someTime };
            var aaa5 = new FileDir() { Name = "AAA", UrlPathName = "aaa", Depth = 2, ParentDir = 2, RootDir = 1, Updated = someTime };
            var aaa6 = new FileDir() { Name = "AAA", UrlPathName = "aaa", Depth = 2, ParentDir = 3, RootDir = 1, Updated = someTime };
            _context.FileDirs.AddRange(dir1, dir2, dir3, dir4, aaa5, aaa6);
            _context.SaveChanges();

            _repo = new(_context, new StubUserIdProvider(2));
            _repo.ClearCache();
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
            var res1 = _repo.GetChainItemsByPath(pathParts)?.ConvertAll(x=>x.Id);

            if (ids is not null)
            {
                var idsActual = ids.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList().ConvertAll(int.Parse);
                CollectionAssert.AreEqual(idsActual, res1);
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
            var shouldGreatThanAfterUpdate = new DateTime(1980,1,1);
            items.ForEach(i => i.Updated = initialTime);
            _context.SaveChanges();

            var targets = targetIds.Split(',').ToList().ConvertAll(int.Parse);
            var expectUpdate = updatedIds.Split(',').ToList().ConvertAll(int.Parse);
            _repo.SetUpdateTimeRangeAncestrally(targets, out string? errmsg);
            Assert.IsNull(errmsg);

            _repo.ChangeTracker.Clear();
            //由于使用了ExecuteUpdate批量操作（本地实体不会被修改）
            //要检查数据库中的状态必须先清空更改跟踪器，否则只会返回本地实体
            items = _repo.All.ToList();
            var actualUpdated = items.FindAll(x => x.Updated >= shouldGreatThanAfterUpdate).ConvertAll(x => x.Id);
            CollectionAssert.AreEquivalent(expectUpdate, actualUpdated);
        }
        #endregion

        #region 结构变化

        [TestMethod]
        [DataRow("6->5", "1|2,3|4,5|6", "1,2,3,4,5,6")]
        //表示6号目录移到5下后，1的depth应为0，23的depth应为1，45的depth应为2
        [DataRow("2->3", "1|3|2,6|4,5", "1,2,3,4,5,6")]
        [DataRow("6->5  2->3", "1|2,3|4,5|6  1|3|2|4,5|6", "1,2,3,4,5,6  1,2,3,4,5,6")]
        [DataRow("2->0", "1,2|3,4,5|6", "1,3,6|2,4,5")]
        [DataRow("2->0  6->4", "1,2|3,4,5|6  1,2|3,4,5|6", "1,3,6|2,4,5  1,3|2,4,5,6")]
        [DataRow("4->3  3->0", "1|2,3|4,5,6  1,3|2,4,6|5", "1,2,3,4,5,6  1,2,5||3,4,6")]
        [DataRow("3->0  2->0", "1,3|2,6|4,5  1,2,3|4,5,6", "1,2,4,5||3,6  1|2,4,5|3,6")]
        //表示2号目录移到0下后，12的depth应为0，345的depth应为1，6的depth应为2，136的root应该为1，245的root应该为2
        public void MoveDir(string howMoveList, string expectedDepthsList, string expectedRootList)
        {
            string[] howMoveArr = howMoveList.Split("  ");
            string[] expectedDepthArr = expectedDepthsList.Split("  ");
            string[] expectedRootArr = expectedRootList.Split("  ");
            for (int i = 0; i < howMoveArr.Length; i++)
            {
                string howMove = howMoveArr[i];
                string expectedDepthsNow = expectedDepthArr[i];
                string expectedRootNow = expectedRootArr[i];
                string[] howMoveParts = howMove.Split("->");
                int beMoved = int.Parse(howMoveParts[0]);
                int to = int.Parse(howMoveParts[1]);

                //此处模拟移动目录的操作
                //首先改动要移动的目录本身
                var moving = _repo.GetById(beMoved)!;
                moving.ParentDir = to;//设置父级目录id，0表示根目录
                if (to == 0)//如果移动到根目录
                {
                    moving.Depth = 0;//深度设为0
                    moving.RootDir = moving.Id;//root设为自己的id
                }
                else//如果移动到其他目录下
                {
                    var destination = _repo.GetById(to)!;
                    moving.Depth = destination.Depth + 1;//深度设为目标深度+1
                    moving.RootDir = destination.RootDir;//root设为父级的root
                }
                moving.Updated = DateTime.Now;
                _context.Update(moving);
                _context.SaveChanges();

                //测试的方法：被移动的目录需要能将其子代作出相应的正确改动
                _repo.UpdateDescendantsInfoFor([beMoved], out var errmsg);
                _context.ChangeTracker.Clear();//模拟scope结束
                Assert.IsNull(errmsg);

                //检查所有目录的深度是否正确
                List<string> expectedDepthsNows = expectedDepthsNow.Split('|').ToList();
                for (int depth = 0; depth < expectedDepthsNows.Count; depth++)
                {
                    List<int> expectedIdsThisDepth = TestStrParse.IntList(expectedDepthsNows[depth]);
                    List<int> actualIdsThisDepth =
                        _repo.Existing.Where(x => x.Depth == depth).Select(x => x.Id).ToList();
                    CollectionAssert.AreEquivalent(expectedIdsThisDepth, actualIdsThisDepth);
                }
                //检查所有目录的root是否正确
                List<string> expectedRootNows = expectedRootNow.Split('|').ToList();
                for (int rootId = 1; rootId <= expectedRootNows.Count; rootId++)
                {
                    List<int> expectedIdsThisRoot = TestStrParse.IntList(expectedRootNows[rootId-1]);
                    List<int> actualIdsThisRoot =
                        _repo.Existing.Where(x => x.RootDir == rootId).Select(x => x.Id).ToList();
                    CollectionAssert.AreEquivalent(expectedIdsThisRoot, actualIdsThisRoot);
                }
            }
        }

        [TestMethod]
        [DataRow(2, 0, "1,2|3,4,5|6", "1,3,6|2,4,5")]
        [DataRow(5, 0, "1,5|2,3|4,6", "1,2,3,4,6||||5")]
        public void ManualFix(int moveDir, int to, string expectedDepthsList, string expectedRootList)
        {
            //在depths和rootDir全错的情况下，只要ParentDir还在，就可以全部重新算对
            _context.FileDirs.ExecuteUpdate(c 
                => c.SetProperty(x => x.RootDir, 100));
            _context.FileDirs.ExecuteUpdate(c 
                => c.SetProperty(x => x.Depth, 100));
            _context.FileDirs.ExecuteUpdate(c
                => c.SetProperty(x => x.Updated, DateTime.Now));
            _context.FileDirs.Where(x => x.Id == moveDir).ExecuteUpdate(c
                => c.SetProperty(x => x.ParentDir, to));
            _context.ChangeTracker.Clear();
            _repo.ManualFixInfoForAll(out _);

            var depthsList = expectedDepthsList.Split('|');
            for (int depth = 0; depth < depthsList.Length; depth++)
            {
                var expectedToBeThisDepth = TestStrParse.IntList(depthsList[depth]);
                var actualThisDepths = 
                    _context.FileDirs.Where(x => x.Depth == depth).Select(x => x.Id).ToList();
                CollectionAssert.AreEquivalent(expectedToBeThisDepth, actualThisDepths);
            }
            List<string> expectedRootNows = expectedRootList.Split('|').ToList();
            for (int rootId = 1; rootId <= expectedRootNows.Count; rootId++)
            {
                var expectedIdsThisRoot = TestStrParse.IntList(expectedRootNows[rootId-1]);
                var actualIdsThisRoot =
                    _repo.Existing.Where(x => x.RootDir == rootId).Select(x => x.Id).ToList();
                CollectionAssert.AreEquivalent(expectedIdsThisRoot, actualIdsThisRoot);
            }
        }
        #endregion
    }
}
