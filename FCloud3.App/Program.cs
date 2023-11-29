using FCloud3.Utils.Utils;
using FCloud3.Utils.Utils.Settings;
using FCloud3.Repos;
using FCloud3.App.Services;
using FCloud3.Services;

var builder = WebApplication.CreateBuilder(args);

//读取配置文件
SettingsAccessor<AppSettingsModel> settings = new();

//注册服务容器
builder.Services.AddControllersWithViews();

builder.Services.AddUtils(settings);
builder.Services.AddRepos(settings);
builder.Services.AddServices();

builder.Services.AddAppServices();
builder.Services.AddJwtService(settings);
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
app.UseExceptionHandler("/Error");

app.UseCors(localVueCors);

app.UseFileServer();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action}/{id?}");
app.MapControllerRoute(
    name: "api",
    pattern: "api/{controller}/{action}/{id?}");

app.Run();
