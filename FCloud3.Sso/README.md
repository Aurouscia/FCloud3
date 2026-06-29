# FCloud3.Sso

FCloud3 的简易跨站 SSO（单点登录）类库，采用“签发方 / 受众方”分离的模型，让多个应用共享 FCloud3 的用户体系。

## 角色说明

- **Issuer（签发方）**：拥有用户体系的 FCloud3 站点。负责展示授权页、生成一次性 `code`、验证 `code` 并返回用户信息。
- **Audience（受众方）**：希望使用 FCloud3 账号登录的其他应用。负责跳转到签发方授权页、接收 `code` 并向签发方验证。

一个站点只能是签发方、只能是受众方，或同时承担两种角色。

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
    "Id": "main-issuer",
    "Enabled": true,
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

`RequireLevel` 的具体含义由使用方应用自行约定。

签发方提供以下端点：

| 方法 | 路径 | 说明 |
|------|------|------|
| GET | `/f3sso/iss?clientId=xxx` | 校验当前用户等级，生成一次性 `code` |
| GET | `/f3sso/iss/config` | 返回签发方配置，用于前端授权页展示 |
| GET | `/f3sso/iss/validate/{code}` | 验证 `code`，返回用户信息 |

### 2. 受众方

在 `Program.cs` 中注册服务并映射端点：

```csharp
using FCloud3.Sso.Audience;

builder.Services.AddScoped<F3SsoAudience>();

var app = builder.Build();
app.MapF3SsoAudienceEndpoints();
```

`appsettings.json` 配置示例：

```json
{
  "F3Sso": {
    "Id": "appa",
    "Enabled": true,
    "RedirectPath": "/sso/callback",
    "Issuers": [
      {
        "Id": "main-issuer",
        "DisplayName": "FCloud3 主站",
        "Origin": "https://fcloud3.example.com",
        "Avatar": "",
        "ClientId": "appA"
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
