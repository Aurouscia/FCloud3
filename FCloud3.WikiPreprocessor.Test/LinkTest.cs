using FCloud3.WikiPreprocessor.Context;
using FCloud3.WikiPreprocessor.DataSource.Models;
using FCloud3.WikiPreprocessor.Mechanics;
using FCloud3.WikiPreprocessor.Options;
using FCloud3.WikiPreprocessor.Test.Support;

namespace FCloud3.WikiPreprocessor.Test
{
    internal class ScopedDataSourceWithLink : DataSourceBase
    {
        private readonly static List<LinkItem> LinkSource = [
            new LinkItem("武汉市", "wuhan"),
            new LinkItem("热干面","hot-dry-noodles")
        ];
        public override LinkItem? Link(string linkSpan)
        {
            return LinkSource.FirstOrDefault(x => 
                x.Text == linkSpan || x.Url == linkSpan);
        }
    }

    [TestClass]
    public class LinkTest
    {
        private readonly ParserContext _ctxDefault;
        private readonly ParserContext _ctxWithConvertFn;

        public LinkTest()
        {
            var dataSource = new ScopedDataSourceWithLink();

            var options1 = new ParserBuilder()
                .GetCurrentOptions();
            _ctxDefault = new(options1);
            _ctxDefault.SetDataSource(dataSource);

            var options2 = new ParserBuilder()
                .Link.ReplaceConvertFn((l, mustUseName) =>
                {
                    if(mustUseName is not null)
                        return $"<a pathName=\"{l.Url}\">{mustUseName}</a>";
                    return $"<a pathName=\"{l.Url}\">{l.Text}</a>";
                })
                .GetCurrentOptions();
            _ctxWithConvertFn = new(options2);
            _ctxWithConvertFn.SetDataSource(dataSource);
        }

        [TestMethod]
        [DataRow("[武汉市]是一座历史悠久的城市", "<a href=\"wuhan\">武汉市</a>是一座历史悠久的城市")]
        [DataRow("[wuhan]是一座历史悠久的城市", "<a href=\"wuhan\">武汉市</a>是一座历史悠久的城市")]
        [DataRow("[changsha]是一座历史悠久的城市", "<a class=\"redLink\" pathName=\"changsha\">changsha</a>是一座历史悠久的城市")]
        [DataRow("[武汉](wuhan)是一座历史悠久的城市", "<a href=\"wuhan\">武汉</a>是一座历史悠久的城市")]
        [DataRow("[武汉](wuhan)人早餐吃[热干面]", "<a href=\"wuhan\">武汉</a>人早餐吃<a href=\"hot-dry-noodles\">热干面</a>")]
        [DataRow("[武汉](/wiki/wuhan)人早餐吃[热干面]", "<a href=\"/wiki/wuhan\">武汉</a>人早餐吃<a href=\"hot-dry-noodles\">热干面</a>")]
        [DataRow("[武汉](wuhan)人早餐吃[/wiki/rgm]", "<a href=\"wuhan\">武汉</a>人早餐吃<a href=\"/wiki/rgm\">/wiki/rgm</a>")]
        [DataRow("[长沙](changsha)是一座历史悠久的城市", "<a class=\"redLink\" pathName=\"changsha\">长沙</a>是一座历史悠久的城市")]
        public void DefaultConvert(string input, string expected)
        {
            _ctxDefault.SetInitialFrameCount();
            var p = new InlineParser(_ctxDefault);
            var @out = p.Run(input, false);
            var html = @out.ToHtml();
            Assert.AreEqual(expected, html);
        }
        
        [TestMethod]
        [DataRow("[武汉市]是一座历史悠久的城市", "<a pathName=\"wuhan\">武汉市</a>是一座历史悠久的城市")]
        [DataRow("[wuhan]是一座历史悠久的城市", "<a pathName=\"wuhan\">武汉市</a>是一座历史悠久的城市")]
        [DataRow("[changsha]是一座历史悠久的城市", "<a class=\"redLink\" pathName=\"changsha\">changsha</a>是一座历史悠久的城市")]
        [DataRow("[武汉](wuhan)是一座历史悠久的城市", "<a pathName=\"wuhan\">武汉</a>是一座历史悠久的城市")]
        [DataRow("[武汉](wuhan)人早餐吃[热干面]", "<a pathName=\"wuhan\">武汉</a>人早餐吃<a pathName=\"hot-dry-noodles\">热干面</a>")]
        [DataRow("[长沙](changsha)是一座历史悠久的城市", "<a class=\"redLink\" pathName=\"changsha\">长沙</a>是一座历史悠久的城市")]
        public void CustomConvert(string input, string expected)
        {
            _ctxWithConvertFn.SetInitialFrameCount();
            var p = new InlineParser(_ctxWithConvertFn);
            var @out = p.Run(input, false);
            var html = @out.ToHtml();
            Assert.AreEqual(expected, html);
        }
    }
}