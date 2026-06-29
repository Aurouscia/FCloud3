# FCloud3.Sso

FCloud3 的简易跨站 SSO（单点登录）类库，采用“签发方 / 受众方”分离的模型，让多个应用共享同一套用户体系。

## 角色说明

- **Issuer（签发方）**：拥有用户体系的站点。负责展示授权页、生成一次性 `code`、验证 `code` 并返回用户信息。
- **Audience（受众方）**：希望使用上述账号登录的其他应用。负责跳转到签发方授权页、接收 `code` 并向签发方验证。

一个站点只能是签发方、只能是受众方，或同时承担两种角色。

## 完整流程

以“用户通过 FCloud3（Issuer）登录到应用 A（Audience）”为例：

1. **用户点击登录**：应用 A 的前端请求 `/f3sso/aud/config`，获取可用签发方列表。
2. **选择签发方**：用户选定某个 Issuer 后，前端跳转到受众方授权入口：
   ```
   https://appa.example.com/f3sso/aud/auth?issuerId=fcloud3.example.com
   ```
   该端点内部调用 `F3SsoAudience.BuildAuthorizeUrl` 生成签发方授权 URL，并 302 跳转到：
   ```
   https://fcloud3.example.com/f3sso/iss/entry?clientId=appa.example.com
   ```
3. **跳转授权**：浏览器跳到签发方（上述 URL）。签发方的 `/f3sso/iss/entry` handler 会根据其 `EntryPath` 配置重定向到签发方自己实现的授权页面，并保留 `clientId` 等所有查询参数。
4. **签发 code**：用户在签发方页面点击“确认授权”后，签发方前端调用 `/f3sso/iss?clientId=xxx`。该端点会校验当前用户等级，返回一次性 `code`。
5. **回跳受众方**：签发方前端页面拿到 `code` 后，应将浏览器重定向回应用 A，URL 例子如下：
   ```
   https://appa.example.com/f3sso/aud/validate?code=xxx&issuerId=fcloud3.example.com
   ```
6. **验证并登录**：应用 A 的 `/f3sso/aud/validate` 端点 handler 内会用 `code` 向签发方 `/f3sso/iss/validate/{code}` 换取用户信息，然后调用宿主实现的 `IF3SsoSignInHandler.HandleAsync` 写入登录态。
7. **最终重定向**：验证成功后 302 到 `RedirectPath?success=1`；失败时 302 到 `RedirectPath?success=0&errmsg=...`，由应用 A 自行展示结果。

- `code` 在签发方内存缓存中有效期为 1 分钟，且只能使用一次。
- 开发者需要实现的：
  - **签发方**：实现并注册 `IUserInfoProvider`，用于提供当前登录用户的信息；实现 `EntryPath` 指向的授权页面，页面中调用 `/f3sso/iss?clientId=xxx` 获取 `code`，再引导浏览器跳回应用 A 的 `/f3sso/aud/validate?code=xxx&issuerId=xxx`。
  - **受众方（应用 A）**：
    - 实现并注册 `IF3SsoSignInHandler`，在验证成功后写入登录态（Cookie / Session / JWT 等）。
    - 登录页调用 `/f3sso/aud/config` 展示可用签发方列表，用户点击后跳转到 `/f3sso/aud/auth?issuerId=xxx`。
    - 实现 `RedirectPath` 指向的结果页，根据 `success` 和 `errmsg` 参数展示登录结果，并可进行前端状态同步（如 localStorage）。

## 快速开始

### 1. 签发方

在 `Program.cs` 中注册服务并映射端点：

```csharp
using FCloud3.Sso.Issuer;

// 注册：IUserInfoProvider 需要自行实现
builder.Services.AddScoped<IUserInfoProvider, MyUserInfoProvider>();
builder.Services.AddScoped<F3SsoIssuerService>();

var app = builder.Build();
app.MapF3SsoIssuerEndpoints();
```

`appsettings.json` 配置示例：

```json
{
  "F3Sso": {
    "Id": "fcloud3.example.com",
    "Enabled": true,
    "EntryPath": "/sso/entry",
    "Audiences": [
      {
        "Id": "appA",
        "DisplayName": "应用 A",
        "Origin": "https://appa.example.com",
        "Avatar": "",
        "RequireLevel": 1
      }
    ]
  }
}
```

- `RequireLevel` 的具体含义由使用方应用自行约定
- `Id` 建议都使用域名

签发方提供以下端点：

| 方法 | 路径 | 说明 |
|------|------|------|
| GET | `/f3sso/iss?clientId=xxx` | 校验当前用户等级，生成一次性 `code` |
| GET | `/f3sso/iss/config` | 返回签发方配置，用于前端授权页展示 |
| GET | `/f3sso/iss/validate/{code}` | 验证 `code`，返回用户信息 |
| GET | `/f3sso/iss/entry?xxx=yyy` | 302 跳转到 `EntryPath`，并保留原查询字符串 |

### 2. 受众方

在 `Program.cs` 中注册服务并映射端点：

```csharp
using FCloud3.Sso.Audience;

builder.Services.AddF3SsoAudience();

var app = builder.Build();
app.MapF3SsoAudienceEndpoints();
```

自定义验证成功后的登录行为：需要注册自己的 `IF3SsoSignInHandler` 实现：

```csharp
builder.Services.AddScoped<IF3SsoSignInHandler, MyCookieSignInHandler>();
```

`appsettings.json` 配置示例：

```json
{
  "F3Sso": {
    "Id": "appa.example.com",
    "Enabled": true,
    "RedirectPath": "/sso/callback",
    "Issuers": [
      {
        "Id": "fcloud3.example.com",
        "DisplayName": "FCloud3 主站",
        "Origin": "https://fcloud3.example.com",
        "Avatar": "",
        "ClientId": "appa.example.com"
      }
    ]
  }
}
```

`Issuers[].ClientId` 为可选字段。不填时，`BuildAuthorizeUrl` 会自动使用当前受众方的 `Id`（即 `F3Sso.Id`）作为 `clientId`。

受众方提供以下端点：

| 方法 | 路径 | 说明 |
|------|------|------|
| GET | `/f3sso/aud/config` | 返回受众方配置，用于登录入口展示 |
| GET | `/f3sso/aud/auth?issuerId=xxx` | 302 跳转到签发方授权入口 |
| GET | `/f3sso/aud/validate?code=xxx&issuerId=xxx` | 向签发方验证 `code`，成功后 302 到 `RedirectPath?success=1` |

验证失败时重定向到 `RedirectPath?success=0&errmsg=...`。

## 实现 `IUserInfoProvider`

签发方需要知道“当前登录用户是谁”。实现 `IUserInfoProvider` 接口并注册到 DI：

```csharp
public interface IUserInfoProvider
{
    int GetUserId();
    string GetUserName();
    byte GetUserLevel();
}
```

示例实现可基于 `HttpContext.User` 或项目中的 `HttpUserInfoService`。

## 测试

`FCloud3.Sso.Test` 项目包含：

- **单元测试**：验证 `F3SsoIssuerService` 的等级判断、`code` 生成与读取。
- **集成测试**：使用 `Microsoft.AspNetCore.TestHost` 启动内存中的 TestServer，完整测试 Issuer 与 Audience 的端点交互，包括跨服务 `code` 验证流程。

运行测试：

```bash
dotnet test FCloud3.Sso.Test/FCloud3.Sso.Test.csproj
```
