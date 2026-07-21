using FCloud3.DbContexts;
using FCloud3.Entities.Ai;
using FCloud3.Entities.Identities;
using FCloud3.Repos.Etc;

namespace FCloud3.Repos.Identities
{
    public class AiInstanceConfigRepo(FCloudContext context, ICommitingUserIdProvider userIdProvider)
        : RepoBase<AiInstanceConfig>(context, userIdProvider)
    {
        public AiInstanceConfig? GetByGroupId(int groupId)
            => Existing.FirstOrDefault(x => x.GroupId == groupId);

        public List<AiInstanceConfig> GetByGroupIdAll(int groupId)
            => Existing.Where(x => x.GroupId == groupId).ToList();

        public bool TryAddConfig(AiInstanceConfig config, out string? errmsg)
        {
            errmsg = Validate(config);
            if (errmsg is not null)
                return false;
            AddAndGetId(config);
            return true;
        }

        public bool TryUpdateConfig(AiInstanceConfig config, out string? errmsg)
        {
            errmsg = Validate(config);
            if (errmsg is not null)
                return false;
            Update(config);
            return true;
        }

        private static string? Validate(AiInstanceConfig config)
        {
            if (config.InstanceName?.Length > AiInstanceConfig.InstanceNameMaxLength)
                return $"实例名称长度不能超过 {AiInstanceConfig.InstanceNameMaxLength}";
            if (config.ApiBaseUrl?.Length > AiInstanceConfig.ApiBaseUrlMaxLength)
                return $"API 地址长度不能超过 {AiInstanceConfig.ApiBaseUrlMaxLength}";
            if (config.ApiKey?.Length > AiInstanceConfig.ApiKeyMaxLength)
                return $"API Key 长度不能超过 {AiInstanceConfig.ApiKeyMaxLength}";
            if (config.DefaultModelName?.Length > AiInstanceConfig.DefaultModelNameMaxLength)
                return $"默认模型名长度不能超过 {AiInstanceConfig.DefaultModelNameMaxLength}";
            if (config.SystemPrompt?.Length > AiInstanceConfig.SystemPromptMaxLength)
                return $"系统提示词长度不能超过 {AiInstanceConfig.SystemPromptMaxLength}";
            return null;
        }
    }
}
