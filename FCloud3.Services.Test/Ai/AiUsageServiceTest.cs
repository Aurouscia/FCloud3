using FCloud3.DbContexts;
using FCloud3.DbContexts.DbSpecific;
using FCloud3.Entities.Ai;
using FCloud3.Entities.Identities;
using FCloud3.Repos.Ai;
using FCloud3.Repos.Identities;
using FCloud3.Services.Test.TestSupport;
using FCloud3.Services.Ai;

namespace FCloud3.Services.Test.Ai
{
    [TestClass]
    public class AiUsageServiceTest
    {
        private readonly FCloudContext _ctx;
        private readonly AiUsageService _svc;
        private readonly AiUsageRecordRepo _repo;
        private readonly AiInstanceConfigRepo _configRepo;
        private readonly DateTime _baseTime = new(2024, 1, 15, 10, 0, 0);

        public AiUsageServiceTest()
        {
            _ctx = FCloudMemoryContext.Create();
            _repo = new AiUsageRecordRepo(_ctx, new StubUserIdProvider(1));
            _configRepo = new AiInstanceConfigRepo(_ctx, new StubUserIdProvider(1));
            _svc = new AiUsageService(_repo, _configRepo);

            // 准备数据：2 个 AI 实例配置
            _ctx.AiInstanceConfigs.AddRange(new List<AiInstanceConfig>
            {
                new()
                {
                    Id = 1,
                    GroupId = 100,
                    DefaultModelName = "gpt-4o",
                    ApiBaseUrl = "https://api.openai.com/v1",
                    ApiKey = "key1",
                    CreatorUserId = 1,
                    Created = _baseTime,
                    Updated = _baseTime
                },
                new()
                {
                    Id = 2,
                    GroupId = 200,
                    DefaultModelName = "claude-3.5",
                    ApiBaseUrl = "https://api.anthropic.com",
                    ApiKey = "key2",
                    CreatorUserId = 1,
                    Created = _baseTime,
                    Updated = _baseTime
                }
            });

            // 准备用量记录
            _ctx.AiUsageRecords.AddRange(new List<AiUsageRecord>
            {
                // 配置 1，用户 1，今天
                new()
                {
                    UserId = 1,
                    AiInstanceConfigId = 1,
                    InputTokens = 100,
                    OutputTokens = 50,
                    TotalTokens = 150,
                    ModelName = "gpt-4o",
                    Success = true,
                    Created = _baseTime,
                    Updated = _baseTime
                },
                // 配置 1，用户 1，今天
                new()
                {
                    UserId = 1,
                    AiInstanceConfigId = 1,
                    InputTokens = 200,
                    OutputTokens = 100,
                    TotalTokens = 300,
                    ModelName = "gpt-4o",
                    Success = true,
                    Created = _baseTime.AddHours(1),
                    Updated = _baseTime.AddHours(1)
                },
                // 配置 1，用户 2，今天
                new()
                {
                    UserId = 2,
                    AiInstanceConfigId = 1,
                    InputTokens = 50,
                    OutputTokens = 50,
                    TotalTokens = 100,
                    ModelName = "gpt-4o",
                    Success = true,
                    Created = _baseTime.AddHours(2),
                    Updated = _baseTime.AddHours(2)
                },
                // 配置 1，用户 1，昨天（应被默认过滤）
                new()
                {
                    UserId = 1,
                    AiInstanceConfigId = 1,
                    InputTokens = 1000,
                    OutputTokens = 500,
                    TotalTokens = 1500,
                    ModelName = "gpt-4o",
                    Success = true,
                    Created = _baseTime.AddDays(-1),
                    Updated = _baseTime.AddDays(-1)
                },
                // 配置 2，用户 1，今天
                new()
                {
                    UserId = 1,
                    AiInstanceConfigId = 2,
                    InputTokens = 80,
                    OutputTokens = 20,
                    TotalTokens = 100,
                    ModelName = "claude-3.5",
                    Success = false,
                    Created = _baseTime,
                    Updated = _baseTime
                }
            });

            _ctx.SaveChanges();
        }

        [TestMethod]
        public void GetUserConfigUsage_ReturnsSumForUserAndConfig()
        {
            // 用户 1 在配置 1：150 + 300 = 450
            var total = _svc.GetUserConfigUsage(1, 1, _baseTime.Date);
            Assert.AreEqual(450, total);
        }

        [TestMethod]
        public void GetUserConfigUsage_FiltersByDate()
        {
            // 用户 1 在配置 1，从昨天开始算：150 + 300 + 1500 = 1950
            var total = _svc.GetUserConfigUsage(1, 1, _baseTime.AddDays(-1).Date);
            Assert.AreEqual(1950, total);
        }

        [TestMethod]
        public void GetUserConfigUsage_NoRecords_ReturnsZero()
        {
            var total = _svc.GetUserConfigUsage(99, 1, _baseTime.Date);
            Assert.AreEqual(0, total);
        }

        [TestMethod]
        public void GetConfigUsage_ReturnsSumForAllUsers()
        {
            // 配置 1 所有用户今天：150 + 300 + 100 = 550
            var total = _svc.GetConfigUsage(1, _baseTime.Date);
            Assert.AreEqual(550, total);
        }

        [TestMethod]
        public void GetConfigUsage_DifferentConfig_Isolated()
        {
            // 配置 2 所有用户今天：100
            var total = _svc.GetConfigUsage(2, _baseTime.Date);
            Assert.AreEqual(100, total);
        }

        [TestMethod]
        public void GetGroupUsage_ReturnsSumViaGroupIdLookup()
        {
            // Group 100 -> Config 1 -> 今天 550
            var total = _svc.GetGroupUsage(100, _baseTime.Date);
            Assert.AreEqual(550, total);
        }

        [TestMethod]
        public void GetGroupUsage_NoConfig_ReturnsZero()
        {
            // Group 999 没有配置
            var total = _svc.GetGroupUsage(999, _baseTime.Date);
            Assert.AreEqual(0, total);
        }

        [TestMethod]
        public void GetConfigUsageRanking_ReturnsOrderedRanking()
        {
            var ranking = _svc.GetConfigUsageRanking(1, _baseTime.Date);

            Assert.AreEqual(2, ranking.Count);
            // 用户 1：450 tokens，2 次调用
            Assert.AreEqual(1, ranking[0].UserId);
            Assert.AreEqual(450, ranking[0].TotalTokens);
            Assert.AreEqual(2, ranking[0].CallCount);
            // 用户 2：100 tokens，1 次调用
            Assert.AreEqual(2, ranking[1].UserId);
            Assert.AreEqual(100, ranking[1].TotalTokens);
            Assert.AreEqual(1, ranking[1].CallCount);
        }

        [TestMethod]
        public void GetConfigUsageRanking_Empty_ReturnsEmptyList()
        {
            var ranking = _svc.GetConfigUsageRanking(99, _baseTime.Date);
            Assert.AreEqual(0, ranking.Count);
        }

        [TestMethod]
        public void GetGroupUsageRanking_ReturnsViaGroupIdLookup()
        {
            var ranking = _svc.GetGroupUsageRanking(100, _baseTime.Date);

            Assert.AreEqual(2, ranking.Count);
            Assert.AreEqual(1, ranking[0].UserId);
            Assert.AreEqual(450, ranking[0].TotalTokens);
        }

        [TestMethod]
        public void GetGroupUsageRanking_NoConfig_ReturnsEmptyList()
        {
            var ranking = _svc.GetGroupUsageRanking(999, _baseTime.Date);
            Assert.AreEqual(0, ranking.Count);
        }

        [TestMethod]
        public void DefaultSince_IsToday()
        {
            // 不传 since 时默认今天（运行时的 DateTime.Now.Date），
            // 测试数据是 2024-01-15，如果今天不是这一天则结果为 0
            // 这里显式传 since 来验证方法本身逻辑正确
            var total = _svc.GetUserConfigUsage(1, 1);
            // 由于默认 since = DateTime.Now.Date，而测试数据是 2024 年的，
            // 在不传 since 时结果应为 0（数据日期早于今天）
            Assert.AreEqual(0, total);
        }
    }
}
