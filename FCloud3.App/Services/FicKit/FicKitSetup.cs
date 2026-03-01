using Aurouscia.FicKit.Common.Abstractions;
using Aurouscia.FicKit.Currency;
using Aurouscia.FicKit.Currency.Options;
using FCloud3.Entities.Identities;

namespace FCloud3.App.Services.FicKit
{
    public static class FicKitSetup
    {
        public static void SetupFicKitCurrency(this WebApplicationBuilder builder)
        {
            // 注册 FicKit 模块
            var dataPath = builder.Configuration.GetValue<string>("FicKit:Currency:DataPath") 
                ?? throw new Exception("缺少配置项 FicKit:Currency:DataPath");
            // 转换为绝对路径（相对于 ContentRootPath）
            if (!Path.IsPathRooted(dataPath))
            {
                dataPath = Path.Combine(builder.Environment.ContentRootPath, dataPath);
            }

            // 配置货币模块访问权限
            var currencyOptions = new CurrencyOptions
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

            builder.Services.AddScoped<IUserInfoProvider, FicKitUserInfoProvider>();

            builder.Services.AddFicKitCurrency(currencyOptions);
        }
    }
}
