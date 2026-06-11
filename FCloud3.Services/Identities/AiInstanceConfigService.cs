using FCloud3.Entities.Ai;
using FCloud3.Entities.Identities;
using FCloud3.Repos.Identities;

namespace FCloud3.Services.Identities
{
    public class AiInstanceConfigService(
        AiInstanceConfigRepo repo,
        UserGroupRepo userGroupRepo,
        UserToGroupRepo userToGroupRepo,
        IOperatingUserIdProvider userIdProvider)
    {
        private readonly int _userId = userIdProvider.Get();

        public AiInstanceConfig? GetConfig(int groupId, out string? errmsg)
        {
            errmsg = null;
            var relation = userToGroupRepo.GetRelation(groupId, _userId);
            if (relation is null || !relation.Type.IsFormalMember())
            {
                errmsg = "你不是该团体成员";
                return null;
            }
            return repo.GetByGroupId(groupId);
        }

        public bool SetConfig(int groupId, string apiBaseUrl, string apiKey, string modelName,
            string? systemPrompt, bool enabled, int defaultDirId, int maxContextMessages,
            int dailyTokenLimit, int monthlyTokenLimit, out string? errmsg)
        {
            var group = userGroupRepo.GetById(groupId);
            if (group is null)
            {
                errmsg = "团体不存在";
                return false;
            }
            if (group.OwnerUserId != _userId)
            {
                errmsg = "仅团体所有者可以修改 AI 配置";
                return false;
            }

            var config = repo.GetByGroupId(groupId);
            if (config is null)
            {
                config = new AiInstanceConfig
                {
                    GroupId = groupId,
                    ApiBaseUrl = apiBaseUrl,
                    ApiKey = apiKey,
                    ModelName = modelName,
                    SystemPrompt = systemPrompt,
                    Enabled = enabled,
                    DefaultDirId = defaultDirId,
                    MaxContextMessages = maxContextMessages,
                    DailyTokenLimit = dailyTokenLimit,
                    MonthlyTokenLimit = monthlyTokenLimit
                };
                repo.AddConfig(config);
            }
            else
            {
                config.ApiBaseUrl = apiBaseUrl;
                config.ApiKey = apiKey;
                config.ModelName = modelName;
                config.SystemPrompt = systemPrompt;
                config.Enabled = enabled;
                config.DefaultDirId = defaultDirId;
                config.MaxContextMessages = maxContextMessages;
                config.DailyTokenLimit = dailyTokenLimit;
                config.MonthlyTokenLimit = monthlyTokenLimit;
                repo.UpdateConfig(config);
            }
            errmsg = null;
            return true;
        }
    }
}
