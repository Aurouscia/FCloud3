using FCloud3.DbContexts;
using FCloud3.Entities.Table;
using FCloud3.Entities.TextSection;
using FCloud3.Entities.Wiki;
using FCloud3.Services.Test.TestSupport;
using FCloud3.Services.Wiki;

namespace FCloud3.Services.Test.Wiki
{
    [TestClass]
    public class WikiItemServiceTest
    {
        private readonly FCloudContext _ctx;
        private readonly WikiItemService _service;
        private readonly WikiItem _w1 = new() { Title = "侏儒兔", UrlPathName = "dwarf-rabbit" };
        private readonly WikiItem _w2 = new() { Title = "干草", UrlPathName = "hay" };
        private readonly WikiItem _w3 = new() { Title = "冬枣", UrlPathName = "winter-dates" };
        private readonly DateTime _initTime = new(2024, 6, 1);
        public WikiItemServiceTest() 
        {
            var provider = new TestingServiceProvider();
            _service = provider.Get<WikiItemService>();
            _ctx = provider.Get<FCloudContext>();
            _w1.Updated = _initTime;
            _w2.Updated = _initTime;
            _w3.Updated = _initTime;
            _ctx.WikiItems.AddRange(_w1, _w2, _w3);
            _ctx.SaveChanges();
            var text1 = new TextSection()
            {
                Title = "食物",
                Content = "我家这只侏儒兔喜欢吃[某种枣子](winter-dates)",
            };
            var table1 = new FreeTable()
            {
                Name = "营养价值",
                Data = "干草是粗纤维食物，糖分和水分不如冬枣那么多，更适合作为兔子的主食",
            };
            var para1 = new WikiPara()
            {
                Type = WikiParaType.Text,
                ObjectId = 1,  //食物 段落
                WikiItemId = 1 //侏儒兔 词条
            };
            var para2 = new WikiPara()
            {
                Type = WikiParaType.Table,
                ObjectId = 1,  //营养价值 段落
                WikiItemId = 2 //干草 词条
            };
            var contain = new WikiTitleContain()
            {
                Type = WikiTitleContainType.FreeTable,
                ObjectId = 1,  //营养价值 段落
                WikiId = 3,    //干草词条中的"营养价值"段落包含干草的隐式引用(TitleContain)
                Updated = _initTime
            };
            var @ref = new WikiRef()
            {
                WikiId = 1,    //侏儒兔 词条
                Str = "winter-dates" //侏儒兔词条中的"食物"段落包含"冬枣"的显式引用
            };
            _ctx.AddRange(text1, table1, para1, para2, contain, @ref);
            _ctx.SaveChanges();
            _ctx.ChangeTracker.Clear();
        }

        [TestMethod]
        public void Create_GetInfo()
        {
            string title = "荷兰猪";
            string urlPathName = "guinea-pig";
            var res = _service.Create(title, urlPathName, out string? errmsg);
            Assert.IsTrue(res);
            Assert.IsNull(errmsg);

            _ctx.ChangeTracker.Clear();
            var info = _service.GetInfo(urlPathName, out errmsg);
            Assert.IsNull(errmsg);
            Assert.AreEqual(title, info?.Title);
            Assert.AreEqual(urlPathName, info?.UrlPathName);
        }

        [TestMethod]
        public void UpdateRefProp()
        {
            string title = "荷兰猪";
            string urlPathName = "guinea-pig";
            _service.Create(title, urlPathName, out _);

            var w1 = _service.GetInfoById(1)!;
            var w2 = _service.GetInfoById(2)!;
            var w3 = _service.GetInfoById(3)!;
            Assert.AreEqual(_initTime, w1.Updated);
            Assert.AreEqual(_initTime, w2.Updated);
            Assert.AreEqual(_initTime, w3.Updated);
            _ctx.ChangeTracker.Clear();
 
            DateTime now = DateTime.Now;
            _service.EditInfo(3, "冬枣", "winter-jujube", out _);
            //由于词条1和词条2都引用了冬枣（显式或者隐式）
            //所以冬枣词条的标题/链接名变化时必须让他们重新解析
            //无关词条不受影响

            var w1_updated = _service.GetInfoById(1)!;
            var w2_updated = _service.GetInfoById(2)!;
            Assert.IsTrue(w1_updated.Updated > now);
            Assert.IsTrue(w2_updated.Updated > now);
            Assert.AreEqual(_initTime, w3.Updated);
        }
    }
}
