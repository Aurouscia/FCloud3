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

        public AiInstanceConfig? GetConfig(int id, out string? errmsg)
        {
            errmsg = null;
            var config = repo.GetById(id);
            if (config is null)
            {
                errmsg = "找不到该 AI 实例";
                return null;
            }
            var relation = userToGroupRepo.GetRelation(config.GroupId, _userId);
            if (relation is null || !relation.Type.IsFormalMember())
            {
                errmsg = "你不是该团体成员";
                return null;
            }
            return config;
        }

        public List<AiInstanceConfigSummary> GetMyAvailableInstances()
        {
            var myGroupIds = userToGroupRepo.Existing
                .Where(x => x.UserId == _userId && x.Type.IsFormalMember())
                .Select(x => x.GroupId)
                .ToList();

            var configs = repo.Existing
                .Where(x => myGroupIds.Contains(x.GroupId) && x.Enabled)
                .ToList();

            var groupNames = userGroupRepo.Existing
                .Where(x => configs.Select(c => c.GroupId).Contains(x.Id))
                .ToDictionary(x => x.Id, x => x.Name);

            return configs.Select(x => new AiInstanceConfigSummary(
                x.Id,
                x.GroupId,
                groupNames.GetValueOrDefault(x.GroupId),
                x.InstanceName,
                x.DefaultModelName,
                x.SystemPrompt,
                x.Enabled
            )).ToList();
        }

        public List<AiInstanceConfigSummary> GetListByGroupId(int groupId)
        {
            var groupName = userGroupRepo.GetById(groupId)?.Name;
            return repo.Existing
                .Where(x => x.GroupId == groupId)
                .Select(x => new AiInstanceConfigSummary(
                    x.Id,
                    x.GroupId,
                    groupName,
                    x.InstanceName,
                    x.DefaultModelName,
                    x.SystemPrompt,
                    x.Enabled
                ))
                .ToList();
        }

        public record AiInstanceConfigSummary(
            int Id,
            int GroupId,
            string? GroupName,
            string? InstanceName,
            string? DefaultModelName,
            string? SystemPrompt,
            bool Enabled);

        public bool SetConfig(AiInstanceConfig model, out string? errmsg)
        {
            int groupId;
            AiInstanceConfig? config;

            if (model.Id > 0)
            {
                config = repo.GetById(model.Id);
                if (config is null)
                {
                    errmsg = "找不到该 AI 实例";
                    return false;
                }
                groupId = config.GroupId;
            }
            else
            {
                groupId = model.GroupId;
                config = null;
            }

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

            if (config is null)
            {
                config = new AiInstanceConfig
                {
                    GroupId = groupId,
                    InstanceName = model.InstanceName,
                    ApiBaseUrl = model.ApiBaseUrl,
                    ApiKey = model.ApiKey,
                    DefaultModelName = model.DefaultModelName,
                    SystemPrompt = model.SystemPrompt,
                    Enabled = model.Enabled,
                    DefaultDirId = model.DefaultDirId,
                    MaxContextMessages = model.MaxContextMessages,
                    DailyTokenLimit = model.DailyTokenLimit,
                    MonthlyTokenLimit = model.MonthlyTokenLimit
                };
                repo.AddConfig(config);
            }
            else
            {
                config.InstanceName = model.InstanceName;
                config.ApiBaseUrl = model.ApiBaseUrl;
                config.ApiKey = model.ApiKey;
                config.DefaultModelName = model.DefaultModelName;
                config.SystemPrompt = model.SystemPrompt;
                config.Enabled = model.Enabled;
                config.DefaultDirId = model.DefaultDirId;
                config.MaxContextMessages = model.MaxContextMessages;
                config.DailyTokenLimit = model.DailyTokenLimit;
                config.MonthlyTokenLimit = model.MonthlyTokenLimit;
                repo.UpdateConfig(config);
            }
            errmsg = null;
            return true;
        }
    }
}
