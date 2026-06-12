using FCloud3.Entities.Ai;
using FCloud3.Repos.Ai;
using FCloud3.Repos.Identities;

namespace FCloud3.Services.Ai
{
    public class AiUsageService(AiUsageRecordRepo repo, AiInstanceConfigRepo configRepo)
    {
        /// <summary>查询某用户在某AI实例的累计用量</summary>
        public int GetUserConfigUsage(int userId, int aiInstanceConfigId, DateTime? since = null)
        {
            since ??= DateTime.Now.Date;
            return repo.Existing
                .Where(x => x.UserId == userId && x.AiInstanceConfigId == aiInstanceConfigId && x.Created >= since)
                .Sum(x => x.TotalTokens);
        }

        /// <summary>查询某AI实例的累计用量（所有成员）</summary>
        public int GetConfigUsage(int aiInstanceConfigId, DateTime? since = null)
        {
            since ??= DateTime.Now.Date;
            return repo.Existing
                .Where(x => x.AiInstanceConfigId == aiInstanceConfigId && x.Created >= since)
                .Sum(x => x.TotalTokens);
        }

        /// <summary>查询某团体的累计用量（通过AI实例配置反查）</summary>
        public int GetGroupUsage(int groupId, DateTime? since = null)
        {
            since ??= DateTime.Now.Date;
            var config = configRepo.GetByGroupId(groupId);
            if (config is null) return 0;
            return repo.Existing
                .Where(x => x.AiInstanceConfigId == config.Id && x.Created >= since)
                .Sum(x => x.TotalTokens);
        }

        /// <summary>查询某AI实例的用量排行</summary>
        public List<ConfigUsageSummary> GetConfigUsageRanking(int aiInstanceConfigId, DateTime? since = null)
        {
            since ??= DateTime.Now.Date;
            var query = repo.Existing
                .Where(x => x.AiInstanceConfigId == aiInstanceConfigId && x.Created >= since);

            return query
                .GroupBy(x => x.UserId)
                .Select(g => new { g.Key, Total = g.Sum(x => x.TotalTokens), Count = g.Count() })
                .ToList()
                .Select(x => new ConfigUsageSummary(x.Key, x.Total, x.Count))
                .OrderByDescending(x => x.TotalTokens)
                .ToList();
        }

        /// <summary>查询某团体的用量排行（通过AI实例配置反查）</summary>
        public List<ConfigUsageSummary> GetGroupUsageRanking(int groupId, DateTime? since = null)
        {
            since ??= DateTime.Now.Date;
            var config = configRepo.GetByGroupId(groupId);
            if (config is null) return [];
            return GetConfigUsageRanking(config.Id, since);
        }
    }

    public record ConfigUsageSummary(int UserId, int TotalTokens, int CallCount);
}
