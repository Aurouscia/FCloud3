using Aurouscia.FicKit.Article;
using Aurouscia.FicKit.Article.Options;
using Aurouscia.FicKit.Common.Abstractions;
using Aurouscia.FicKit.Currency;
using Aurouscia.FicKit.Currency.Options;
using FCloud3.Entities.Identities;

namespace FCloud3.App.Services.FicKit
{
    public static class FicKitSetup
    {
        public static void AddFicKitServices(this WebApplicationBuilder builder)
        { 
            builder.Services.AddScoped<IUserInfoProvider, FicKitUserInfoProvider>();
            builder.Services.AddScoped<IUserProfileProvider, FicKitUserProfileProvider>();
        }

        public static void SetupFicKitCurrency(this WebApplicationBuilder builder)
        {
            // 注册 FicKit 模块
            var dataPath = builder.Configuration.GetValue<string>("FicKit:Currency:DataPath");
            if (string.IsNullOrWhiteSpace(dataPath))
                return;

            // 转换为绝对路径（相对于 ContentRootPath）
            if (!Path.IsPathRooted(dataPath))
            {
                dataPath = Path.Combine(builder.Environment.ContentRootPath, dataPath);
            }

            // 配置模块访问权限
            var options = new CurrencyOptions
            {
                DataPath = dataPath,
                // 示例配置：根据需求调整
                ListAccess = new() { UserLevel = (int)UserType.Tourist },   // 任何人都能查看列表
                GetAccess = new() { UserLevel = (int)UserType.Tourist },    // 任何人都能查看详情
                CreateAccess = new() { UserLevel = (int)UserType.Member },  // 注册用户才能创建
                UpdateAccess = new() { UserLevel = (int)UserType.Member, OwnerOnly = true, UserLevelBypass = (int)UserType.Admin }, // 所有者或等级5+用户可更新
                DeleteAccess = new() { UserLevel = (int)UserType.Member, OwnerOnly = true, UserLevelBypass = (int)UserType.Admin }, // 所有者或等级5+用户可删除
                ConvertAccess = new() { UserLevel = (int)UserType.Tourist } // 任何人都能使用换算
            };

            builder.Services.AddFicKitCurrency(options);
        }

        public static void SetupFicKitArticle(this WebApplicationBuilder builder)
        {
            // 注册 FicKit 模块
            var dataPath = builder.Configuration.GetValue<string>("FicKit:Article:DataPath");
            if (string.IsNullOrWhiteSpace(dataPath))
                return;

            // 转换为绝对路径（相对于 ContentRootPath）
            if (!Path.IsPathRooted(dataPath))
            {
                dataPath = Path.Combine(builder.Environment.ContentRootPath, dataPath);
            }

            // 配置模块访问权限
            var options = new ArticleOptions
            {
                DataPath = dataPath,
                // 示例配置：根据需求调整
                ListAccess = new() { UserLevel = (int)UserType.Tourist },   // 任何人都能查看列表
                GetAccess = new() { UserLevel = (int)UserType.Tourist },    // 任何人都能查看详情
                CreateAccess = new() { UserLevel = (int)UserType.Member },  // 注册用户才能创建
                UpdateAccess = new() { UserLevel = (int)UserType.Member, OwnerOnly = true, UserLevelBypass = (int)UserType.Admin }, // 所有者或等级5+用户可更新
                DeleteAccess = new() { UserLevel = (int)UserType.Member, OwnerOnly = true, UserLevelBypass = (int)UserType.Admin }, // 所有者或等级5+用户可删除
                LikeAccess = new() { UserLevel = (int)UserType.Member },
                CollectAccess = new() { UserLevel = (int)UserType.Member },
                SubscribeAccess = new() { UserLevel = (int)UserType.Member },
                HideAccess = new() { UserLevel = (int)UserType.Admin }
            };

            builder.Services.AddFicKitArticle(options);
        }
    }
}
