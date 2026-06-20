using FCloud3.Entities.Ai;
using FCloud3.Repos.Ai;
using FCloud3.Services.Ai;
using FCloud3.Services.Test.TestSupport;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace FCloud3.Services.Test.Ai
{
    [TestClass]
    public class AiUsageRecorderTest
    {
        private readonly AiUsageRecorder _recorder;
        private readonly AiUsageRecordRepo _repo;

        public AiUsageRecorderTest()
        {
            var provider = new TestingServiceProvider(1);
            _repo = provider.Get<AiUsageRecordRepo>();
            _recorder = new AiUsageRecorder(_repo, new FakeLogger<AiUsageRecorder>());
        }

        [TestMethod]
        public void Record_CalculatesTotalTokens()
        {
            _recorder.Record(
                userId: 1,
                aiInstanceConfigId: 10,
                modelName: "gpt-4o",
                inputTokens: 100,
                outputTokens: 50,
                success: true,
                promptSummary: "test prompt",
                relatedWikiItemId: 5,
                conversationId: 20);

            var record = _repo.Existing.First();
            Assert.AreEqual(150, record.TotalTokens);
            Assert.AreEqual(0, record.DurationMs);
            Assert.AreEqual(0, record.CachedInputTokens);
            Assert.AreEqual(1, record.UserId);
            Assert.AreEqual(10, record.AiInstanceConfigId);
            Assert.AreEqual("gpt-4o", record.ModelName);
            Assert.IsTrue(record.Success);
            Assert.AreEqual(5, record.RelatedWikiItemId);
            Assert.AreEqual(20, record.ConversationId);
        }

        [TestMethod]
        public void RecordWithFallback_RecordsDurationAndCachedTokens()
        {
            var providerUsage = new UsageDetails
            {
                InputTokenCount = 100,
                OutputTokenCount = 50,
                CachedInputTokenCount = 30
            };

            _recorder.RecordWithFallback(
                userId: 4,
                aiInstanceConfigId: 40,
                modelName: "gpt-4o",
                messages: [],
                response: "ok",
                success: true,
                promptSummary: null,
                relatedWikiItemId: 0,
                providerUsage: providerUsage,
                durationMs: 1234);

            var record = _repo.Existing.First();
            Assert.AreEqual(100, record.InputTokens);
            Assert.AreEqual(50, record.OutputTokens);
            Assert.AreEqual(1234, record.DurationMs);
            Assert.AreEqual(30, record.CachedInputTokens);
        }

        [TestMethod]
        public void RecordWithFallback_UsesProviderUsage_WhenProvided()
        {
            var providerUsage = new UsageDetails
            {
                InputTokenCount = 200,
                OutputTokenCount = 80
            };

            var messages = new List<ChatMessage>
            {
                new(ChatRole.User, "hello world")
            };

            _recorder.RecordWithFallback(
                userId: 2,
                aiInstanceConfigId: 20,
                modelName: "claude-3.5",
                messages: messages,
                response: "response text",
                success: true,
                promptSummary: "hello",
                relatedWikiItemId: 0,
                conversationId: null,
                providerUsage: providerUsage);

            var record = _repo.Existing.First();
            Assert.AreEqual(200, record.InputTokens);
            Assert.AreEqual(80, record.OutputTokens);
            Assert.AreEqual(280, record.TotalTokens);
        }

        [TestMethod]
        public void RecordWithFallback_UsesLocalEstimate_WhenProviderUsageIsNull()
        {
            var messages = new List<ChatMessage>
            {
                new(ChatRole.System, "sys"),
                new(ChatRole.User, "user message")
            };
            string response = "ai response";

            _recorder.RecordWithFallback(
                userId: 3,
                aiInstanceConfigId: 30,
                modelName: "gpt-3.5",
                messages: messages,
                response: response,
                success: false,
                promptSummary: null,
                relatedWikiItemId: 0);

            var record = _repo.Existing.First();
            // 估算: sys(3)*0.5+4=5, user(12)*0.5+4=10, 共15 input
            // response(11)*0.5=5 output
            Assert.IsTrue(record.InputTokens > 0);
            Assert.IsTrue(record.OutputTokens > 0);
            Assert.AreEqual(record.InputTokens + record.OutputTokens, record.TotalTokens);
            Assert.IsFalse(record.Success);
        }

        [TestMethod]
        public void EstimateTokens_String_ReturnsAtLeastOne()
        {
            // 通过 RecordWithFallback 间接测试估算
            _recorder.RecordWithFallback(
                userId: 1, aiInstanceConfigId: 1, modelName: "m",
                messages: [],
                response: "",  // 空字符串
                success: true,
                promptSummary: null,
                relatedWikiItemId: 0);

            var record = _repo.Existing.First();
            // 空字符串估算也应至少返回 1
            Assert.IsTrue(record.OutputTokens >= 1);
        }

        [TestMethod]
        public void EstimateTokens_Messages_Accuracy()
        {
            var messages = new List<ChatMessage>
            {
                new(ChatRole.User, "abcd")  // 4 chars * 0.5 = 2, +4 = 6
            };

            _recorder.RecordWithFallback(
                userId: 1, aiInstanceConfigId: 1, modelName: "m",
                messages: messages,
                response: "x",  // 1 char * 0.5 = 1 (max 1)
                success: true,
                promptSummary: null,
                relatedWikiItemId: 0);

            var record = _repo.Existing.First();
            Assert.AreEqual(6, record.InputTokens);
            Assert.AreEqual(1, record.OutputTokens);
            Assert.AreEqual(7, record.TotalTokens);
        }
    }
}
