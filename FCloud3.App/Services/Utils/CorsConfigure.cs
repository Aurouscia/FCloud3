namespace FCloud3.App.Services.Utils
{
    public static class CorsConfigure
    {
        public const string corsPolicyName = "FCloud3Cors";
        public static IServiceCollection AddConfiguredCors(
            this IServiceCollection services, IConfiguration config)
        {
            var originsConfig = config.GetSection("Cors:Origins");
            var origins = new List<string>();
            originsConfig.Bind(origins);
            services.AddCors(options =>
            {
                options.AddPolicy(corsPolicyName, b =>
                {
                    b.WithOrigins([.. origins])
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials()
                        .WithExposedHeaders(nameof(HttpResponse.Headers.Authorization));
                });
            });
            return services;
        }
        public static WebApplication UseConfiguredCors(this WebApplication app)
        {
            app.UseCors(corsPolicyName);
            return app;
        }
    }
}
