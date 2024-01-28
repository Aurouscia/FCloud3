using FCloud3.Repos;
using FCloud3.App.Services;
using FCloud3.Services;
using Serilog;

try
{

    var builder = WebApplication.CreateBuilder(args);

    //读取配置
    var c = builder.Configuration;

    //注册服务容器
    //为服务容器和Log静态对象添加Serilog
    builder.Services.AddSerilog(c);
    //添加数据库读写功能
    builder.Services.AddRepos(c);
    //添加业务功能
    builder.Services.AddFCloudServices(c);
    //添加app本身的功能，例如Controller
    builder.Services.AddAppServices();
    //添加jwt鉴权(authentication)
    builder.Services.AddJwtService(c);

    string localVueCors = "localVueCors";
    builder.Services.AddCors(options =>
    {
        options.AddPolicy(localVueCors, builder =>
        {
            builder.WithOrigins("http://127.0.0.1:5173")
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
            builder.WithOrigins("http://localhost:5173")
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        });
    });

    var app = builder.Build();

    //配置请求管道
    app.UseCors(localVueCors);

    app.UseFileServer();

    app.UseRouting();

    app.UseSerilogRequestLogging();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller}/{action}/{id?}");
    app.MapControllerRoute(
        name: "api",
        pattern: "api/{controller}/{action}/{id?}");

    Log.Information("FCloud3.App启用成功=============================================");
    app.Run();
}
catch(Exception ex)
{
    if(ex is not HostAbortedException)
        Log.Error(ex, "FCloud3.App启动失败=============================================");
}
finally
{
    Log.CloseAndFlush();
}