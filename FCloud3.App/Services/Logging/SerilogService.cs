using Serilog;
using Serilog.Events;

namespace FCloud3.App.Services.Logging
{
    public static class SerilogService
    {
        public static IServiceCollection AddSerilog(this IServiceCollection services, IConfiguration config)
        {
            var logger = new LoggerConfiguration()
                .ReadFrom.Configuration(config)
                .WriteTo.Map(
                    evt => evt.Level.ToString().ToLowerInvariant(),
                    (level, wt) => wt.File(
                        $"./Logs/{level}-.txt",
                        rollingInterval: RollingInterval.Day,
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                        shared: true,
                        rollOnFileSizeLimit: true,
                        fileSizeLimitBytes: 500000,
                        retainedFileCountLimit: 60),
                    sinkMapCountLimit: 10)
                .CreateLogger();
            services.AddSerilog(logger);
            Log.Logger = logger;
            return services;
        }

        public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app) 
        {
            app.UseSerilogRequestLogging(config =>
            {
                config.GetLevel = (ctx, time, ex) =>
                {
                    if (ctx.Response.StatusCode > 499 || ex is not null)
                        return LogEventLevel.Error;
                    if (time > 1000)
                        return LogEventLevel.Warning;
                    return LogEventLevel.Information;
                };
            });
            return app;
        }

        private static string? GetControllerName(HttpContext ctx)
        {
            ctx.Request.RouteValues.TryGetValue("Controller", out object? name);
            return name as string;
        }
        private static string? GetActionName(HttpContext ctx)
        {
            ctx.Request.RouteValues.TryGetValue("Action", out object? name);
            return name as string;
        }
    }
}
