﻿using FCloud3.App.Services.Filters;
using FCloud3.App.Services.Utils;
using FCloud3.App.Utils;
using FCloud3.WikiPreprocessor.Util;
using FCloud3.Services;
using FCloud3.Services.Files.Storage.Abstractions;
using FCloud3.Services.Identities;
using FCloud3.Repos.Etc;
using Microsoft.AspNetCore.Http.Timeouts;

namespace FCloud3.App.Services
{
    public static class AppServices
    {
        public static IServiceCollection AddAppServices(this IServiceCollection services, IConfiguration config)
        {
            var debug = config["Debug"] == "on";
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<HttpUserIdProvider>();
            services.AddScoped<HttpUserInfoService>();
            services.AddControllers(options => {
                options.Filters.Add<ApiExceptionFilter>();
            });
            services.AddAuthGrantedActionFilter();
            services.AddUserActiveOperationFilter();
            services.AddUserTypeRestrictedFilter();
            services.AddRateLimitedFilter();
            services.AddFilePathBaseConstraint();
            services.AddMemoryCache(option =>
            {
                option.TrackStatistics = debug;
            });
            services.AddSingleton<ILocatorHash, LocatorHash>();
            services.AddScoped<WikiParserProviderService>();
            services.AddScoped<ICommitingUserIdProvider, HttpUserIdProvider>();
            services.AddScoped<IOperatingUserIdProvider, HttpUserIdProvider>();
            services.AddSingleton<IFileStreamHasher, FileStreamHasher>();
            services.AddSingleton<IUserPwdEncryption, UserPwdEncryption>();
            services.AddRequestTimeouts(options =>
            {
                // 使用 DefaultPolicy 属性设置全局超时策略，超时为 10 秒
                options.DefaultPolicy = new RequestTimeoutPolicy
                {
                    Timeout = TimeSpan.FromSeconds(10)
                };
            });
            
            return services;
        }
    }
}
