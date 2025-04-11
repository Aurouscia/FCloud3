using FCloud3.DbContexts;
using FCloud3.Entities.Wiki;
using FCloud3.Repos.Wiki;
using FCloud3.Services.Test.TestSupport;
using FCloud3.Services.Wiki;

namespace FCloud3.Services.Test.Wiki
{
    [TestClass]
    public class WikiTitleContainServiceTest
    {
        private readonly FCloudContext _context;
        private readonly WikiTitleContainRepo _wikiTitleContainRepo;
        private readonly WikiTitleContainService _wikiTitleContainService;
        public WikiTitleContainServiceTest() 
        {
            TestingServiceProvider serviceProvider = new();
            _wikiTitleContainService = serviceProvider.Get<WikiTitleContainService>();
            _context = serviceProvider.Get<FCloudContext>();
            _wikiTitleContainRepo = serviceProvider.Get<WikiTitleContainRepo>();
            _wikiTitleContainRepo.ClearCache();
            serviceProvider.Get<WikiItemRepo>().ClearCache();
            var contains = new List<WikiTitleContain>
            {
                new(){WikiId = 1, Type = WikiTitleContainType.TextSection, ObjectId = 1, BlackListed = false},
                new(){WikiId = 2, Type = WikiTitleContainType.TextSection, ObjectId = 1, BlackListed = false},
                new(){WikiId = 3, Type = WikiTitleContainType.TextSection, ObjectId = 1, BlackListed = true},
                new(){WikiId = 4, Type = WikiTitleContainType.TextSection, ObjectId = 1, BlackListed = true},

                new(){WikiId = 1, Type = WikiTitleContainType.FreeTable, ObjectId = 1, BlackListed = false},
                new(){WikiId = 1, Type = WikiTitleContainType.TextSection, ObjectId = 2, BlackListed = false},
            };
            contains.ForEach(x => x.Updated = new DateTime(2024, 3, 12));
            var wikis = new List<WikiItem>
            {
                new(){Title = "侏儒兔"},  //1
                new(){Title = "月亮"},    //2
                new(){Title = "恍惚"},    //3
                new(){Title = "琼浆"},    //4
                new(){Title = "离谱"}     //5
            };
            wikis.ForEach(x => x.Updated = new DateTime(2024, 3, 12));
            _context.AddRange(contains);
            _context.AddRange(wikis);
            _context.SaveChanges();
        }
        [TestMethod]
        [DataRow("3,4", "1,2,3,4")]
        [DataRow("1,3,4", "1,2,3,4")]
        [DataRow("1,2", "1,2,3,4")]
        [DataRow("1,3", "1,2,3,4")]
        [DataRow("3,4,5", "1,2,3,4,5")]
        [DataRow("2,3,4,5,6", "1,2,3,4,5,6")]
        public void SetContains(string setParam, string expectedAll)
        {
            var setParamList = TestStrParse.IntList(setParam);
            var expectedAllList = TestStrParse.IntList(expectedAll);
            _wikiTitleContainService.SetContains(WikiTitleContainType.TextSection, 1, setParamList);
            _context.ChangeTracker.Clear();//模拟scope结束
            var nb = _wikiTitleContainRepo.NotBlackListed.WithTypeAndId(WikiTitleContainType.TextSection, 1)
                .Select(x=>x.WikiId).ToList();
            var all = _wikiTitleContainRepo.Existing.WithTypeAndId(WikiTitleContainType.TextSection, 1)
                .Select(x=>x.WikiId).ToList();
            var nb_c = _wikiTitleContainRepo.CachedContains(WikiTitleContainType.TextSection, 1)
                .Select(x => x.WikiId).ToList();
            var all_c = _wikiTitleContainRepo.CachedContains(WikiTitleContainType.TextSection, 1, false)
                .Select(x => x.WikiId).ToList();
            CollectionAssert.AreEquivalent(setParamList, nb);
            CollectionAssert.AreEquivalent(expectedAllList, all);
            CollectionAssert.AreEquivalent(setParamList, nb_c);
            CollectionAssert.AreEquivalent(expectedAllList, all_c);
        }

        [TestMethod]
        public void AutoAppendForGroups()
        {
            List<(WikiTitleContainType containType, int objId, List<int> excludeWIds, string content)> groups = [
                (WikiTitleContainType.TextSection, 1, [], "琼浆玉液酒，180月亮一杯，价格离谱"),
                (WikiTitleContainType.TextSection, 2, [], "这只侏儒兔像月亮一样圆溜"),
                (WikiTitleContainType.FreeTable, 1, [1, 2], "这只侏儒兔像月亮一样圆溜得离谱")
                ];
            _wikiTitleContainService.AutoAppendForGroups(groups);
            var nb_t1 = _wikiTitleContainRepo.NotBlackListed.WithTypeAndId(WikiTitleContainType.TextSection, 1)
                .Select(x => x.WikiId).ToList();
            var nb_t1_exp = new List<int> { 1, 2, 5 };
            CollectionAssert.AreEquivalent(nb_t1_exp, nb_t1);
            var all_t1 = _wikiTitleContainRepo.Existing.WithTypeAndId(WikiTitleContainType.TextSection, 1)
                .Select(x => x.WikiId).ToList();
            var all_t1_exp = new List<int> { 1, 2, 3, 4, 5 };
            CollectionAssert.AreEquivalent(all_t1_exp, all_t1);
            var nb_t2 = _wikiTitleContainRepo.NotBlackListed.WithTypeAndId(WikiTitleContainType.TextSection, 2)
                .Select(x => x.WikiId).ToList();
            var nb_t2_exp = new List<int> { 1, 2 };
            CollectionAssert.AreEquivalent(nb_t2_exp, nb_t2);
            var all_t2 = _wikiTitleContainRepo.Existing.WithTypeAndId(WikiTitleContainType.TextSection, 2)
                .Select(x => x.WikiId).ToList();
            var all_t2_exp = new List<int> { 1, 2 };
            CollectionAssert.AreEquivalent(all_t2_exp, all_t2);
            var nb_f1 = _wikiTitleContainRepo.NotBlackListed.WithTypeAndId(WikiTitleContainType.FreeTable, 1)
                .Select(x => x.WikiId).ToList();
            var nb_f1_exp = new List<int> { 1, 5 };
            CollectionAssert.AreEquivalent(nb_f1_exp, nb_f1);
            var all_f1= _wikiTitleContainRepo.Existing.WithTypeAndId(WikiTitleContainType.FreeTable, 1)
                .Select(x => x.WikiId).ToList();
            var all_f1_exp = new List<int> { 1, 5 };
            CollectionAssert.AreEquivalent(all_f1_exp, all_f1);
        }
    }
}
