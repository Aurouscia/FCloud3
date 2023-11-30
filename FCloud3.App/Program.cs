using FCloud3.Utils.Utils;
using FCloud3.Utils.Settings;
using FCloud3.Repos;
using FCloud3.App.Services;
using FCloud3.Services;

var builder = WebApplication.CreateBuilder(args);

//读取配置文件并存入AppSettings静态类
_ = new SettingsHelper(builder.Configuration);

//注册服务容器

//添加数据库读写功能
builder.Services.AddRepos();
//添加业务功能
builder.Services.AddServices();
//添加app本身的功能，例如Controller和用户身份
builder.Services.AddAppServices();
//添加jwt鉴权(authentication)
builder.Services.AddJwtService();

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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action}/{id?}");
app.MapControllerRoute(
    name: "api",
    pattern: "api/{controller}/{action}/{id?}");

app.Run();
