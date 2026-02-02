using FCloud3.WikiPreprocessor.Mechanics;
using FCloud3.WikiPreprocessor.Options;
using FCloud3.WikiPreprocessor.Test.Support;

namespace FCloud3.WikiPreprocessor.Test
{
    internal class ScopedConvertingProviderWithAutoReplace : ConvertingProviderBase
    {
        public override string? Replace(string replaceTarget)
        {
            return MakeUrlForWiki(replaceTarget);
        }
        public readonly Dictionary<string, int> wikis_1 = new()
        {
            {  "3C教育体系大纲",6 },
            {  "咪么", 28 },
            {  "咪么么么" ,14 },
            {  "拍拍拍拿放", 73 }
        };
        public readonly Dictionary<string, int> wikis_2 = new()
        {
            {  "夜莺",5 },
            {  "帝企鹅", 388 }
        };
        public string MakeUrlForWiki(string title)
        {
            if (wikis_1.TryGetValue(title, out int id1))
                return $"<a href=\"/w/{id1}\">{title}</a>";
            if (wikis_2.TryGetValue(title, out int id2))
                return $"<a href=\"/w/{id2}\">{title}</a>";
            return title;
        }
    }
    [TestClass]
    public class AutoReplaceTest
    {
        private readonly Parser _parser;
        private readonly Parser _parserWithCache;
        private readonly Parser _parserKeepUsage;
        private readonly ScopedConvertingProviderWithAutoReplace _convertingProvider;
        public AutoReplaceTest()
        {
            _convertingProvider = new ScopedConvertingProviderWithAutoReplace();

            var optionsBuilder = new ParserBuilder()
                .AutoReplace.AddReplacingTargets(
                    _convertingProvider.wikis_1.Select(x => x.Key).ToList(),
                    true
                );
            //“单次使用”与“缓存”不可能同时实现，这里不启用缓存机制
            _parser = optionsBuilder.BuildParser();
            _parser.SetConvertingProvider(_convertingProvider);
            
            var optionsBuilder2 = new ParserBuilder()
                .AutoReplace.AddReplacingTargets(
                    _convertingProvider.wikis_1.Select(x => x.Key).ToList(),
                    false
                );
            optionsBuilder2.Cache.EnableCache();
            _parserWithCache = optionsBuilder2.BuildParser();
            _parserWithCache.SetConvertingProvider(_convertingProvider);

            var optionsBuilder3 = new ParserBuilder()
                .AutoReplace.AddReplacingTargets(
                    _convertingProvider.wikis_1.Select(x => x.Key).ToList(),
                    true
                ).KeepRuleUsageBeforeCalling();
            _parserKeepUsage = optionsBuilder3.BuildParser();
            _parserKeepUsage.SetConvertingProvider(_convertingProvider);
        }

        public static IEnumerable<object[]> TestTestData()
        {
            yield return new object[] {
                "更多有趣内容见3C教育体系大纲等词条",
                "<p>更多有趣内容见<a href=\"/w/6\">3C教育体系大纲</a>等词条</p>"
            };
            yield return new object[] {
                "更多有趣内容*见3C教育体系大纲等词条*吧",
                "<p>更多有趣内容<i>见<a href=\"/w/6\">3C教育体系大纲</a>等词条</i>吧</p>"
            };
            yield return new object[] {
                "更多有趣内容见3C教育体系大纲和3C教育体系大纲第二版等词条",
                "<p>更多有趣内容见<a href=\"/w/6\">3C教育体系大纲</a>和3C教育体系大纲第二版等词条</p>"
            };
            yield return new object[] {
                "更多有趣内容*见3C教育体系大纲*和3C教育体系大纲第二版等词条",
                "<p>更多有趣内容<i>见<a href=\"/w/6\">3C教育体系大纲</a></i>和3C教育体系大纲第二版等词条</p>"
            };
            yield return new object[] {
                "Au一边喊着\"咪咪么么么么\"，一边把小兔子拍拍拍拿放",
                "<p>Au一边喊着\"咪<a href=\"/w/14\">咪么么么</a>么\"，一边把小兔子<a href=\"/w/73\">拍拍拍拿放</a></p>"
            };
            yield return new object[] {
                "咪么",
                "<p><a href=\"/w/28\">咪么</a></p>"
            };
        }

        [TestMethod]
        [DynamicData(nameof(TestTestData))]
        public void Test(string input, string answer)
        {
            string res = _parser.RunToPlain(input);
            Assert.AreEqual(answer, res);
        }

        public static IEnumerable<object[]> ChangeTargetsTestData()
        {
            yield return new object[] {
                "更多有趣内容见3C教育体系大纲等词条，更多有趣内容见3C教育体系大纲等词条",
                "<p>更多有趣内容见3C教育体系大纲等词条，更多有趣内容见3C教育体系大纲等词条</p>",
                true
            };
            yield return new object[] {
                "更多有趣内容见3C教育体系大纲等词条，更多有趣内容见3C教育体系大纲等词条",
                "<p>更多有趣内容见<a href=\"/w/6\">3C教育体系大纲</a>等词条，更多有趣内容见3C教育体系大纲等词条</p>",
                false
            };
            yield return new object[] {
                "夜莺是人类伴生种，夜莺会吃城市里的其他小鸟",
                "<p><a href=\"/w/5\">夜莺</a>是人类伴生种，夜莺会吃城市里的其他小鸟</p>",
                true
            };
        }

        [TestMethod]
        [DynamicData(nameof(ChangeTargetsTestData))]
        public void ChangeTargets(string input, string answer, bool clear)
        {
            //更换目标，可选择是否去除旧目标
            List<string?> targets = _convertingProvider.wikis_2.Keys
                .ToList().ConvertAll(x => (string?)x);
            _parser.Context.AutoReplace.Register(targets, true, clear);
            string res = _parser.RunToPlain(input);
            Assert.AreEqual(answer, res);
            _parser.SetConvertingProvider(_convertingProvider);
            string res2 = _parser.RunToPlain(input);
            Assert.AreEqual(answer, res);
        }
        
        public static IEnumerable<object[]> NoSingleUseTestData()
        {
            yield return new object[] {
                "夜莺是人类伴生种，夜莺会吃城市里的其他小鸟",
                "<p><a href=\"/w/5\">夜莺</a>是人类伴生种，<a href=\"/w/5\">夜莺</a>会吃城市里的其他小鸟</p>"
            };
        }

        [TestMethod]
        [DynamicData(nameof(NoSingleUseTestData))]
        public void NoSingleUse(string input, string answer)
        {
            List<string?> targets = _convertingProvider.wikis_2.Keys
                .ToList().ConvertAll(x => (string?)x);
            _parserWithCache.Context.AutoReplace.Register(targets, false, true);
            string res = _parserWithCache.RunToPlain(input);
            Assert.AreEqual(answer, res);
            _parserWithCache.SetConvertingProvider(_convertingProvider);
            string res2 = _parserWithCache.RunToPlain(input);
            Assert.AreEqual(answer, res2);
            _parserWithCache.SetConvertingProvider(_convertingProvider);
            string res3 = _parserWithCache.RunToPlain(input);
            Assert.AreEqual(answer, res3);
        }

        public static IEnumerable<object[]> SingleUseAcrossCallingTestData()
        {
            yield return new object[] {
                "夜莺是人类伴生种，夜莺会吃城市里的其他小鸟",
                "<p><a href=\"/w/5\">夜莺</a>是人类伴生种，夜莺会吃城市里的其他小鸟</p>",
                "<p>夜莺是人类伴生种，夜莺会吃城市里的其他小鸟</p>"
            };
        }

        [TestMethod]
        [DynamicData(nameof(SingleUseAcrossCallingTestData))]
        public void SingleUseAcrossCalling(string input, string answer1, string answer2)
        {
            List<string?> targets = _convertingProvider.wikis_2.Keys
                .ToList().ConvertAll(x => (string?)x);
            _parserKeepUsage.Context.AutoReplace.Register(targets, true, true);
            string res = _parserKeepUsage.RunToPlain(input);
            Assert.AreEqual(answer1, res);
            _parserKeepUsage.SetConvertingProvider(_convertingProvider);
            string res2 = _parserKeepUsage.RunToPlain(input);
            Assert.AreEqual(answer2, res2);
            _parserKeepUsage.SetConvertingProvider(_convertingProvider);
            string res3 = _parserKeepUsage.RunToPlain(input);
            Assert.AreEqual(answer2, res3);
        }
    }
}
