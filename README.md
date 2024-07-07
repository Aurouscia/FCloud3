# FCloud3
## 概述
本项目是一套内容管理系统，遵循Apache-2.0开源协议，可以私有部署和商用。
## 架构
1. 后端基于[asp.netCore](https://dotnet.microsoft.com/zh-cn/apps/aspnet)
2. ORM使用[EntityFramework](https://learn.microsoft.com/zh-cn/ef/)，支持sqlite和sqlserver
3. 前端使用[vue3](https://vuejs.org)+[ts](https://typescriptlang.org)+[vite](https://vite.dev)
4. 身份验证使用[JWT](https://jwt.io)
5. 图像处理使用[ImageSharp](https://sixlabors.com/products/imagesharp/)
6. excel和word文件导入导出使用[NPOI](https://www.nuget.org/packages/NPOI)
7. 资源文件可选择存储在自己的服务器或[阿里云OSS](https://www.aliyun.com/product/oss)

## 安装
### 前提条件
1. [Visual Studio](https://visualstudio.microsoft.com/zh-hans/) 尽可能新版 + web应用开发负载
2. [node客户端](https://nodejs.org/en) 尽可能新版，并确认命令行中有npm命令可用
3. [git客户端](https://git-scm.com/downloads) 用来下载代码和记录改动
4. （可选）visual studio code用来编辑前端代码
5. 关于网页应用的基本常识
6. 对IIS或docker基本用法的理解

### 步骤
1. 在命令行中输入`git clone 【本仓库链接】`
2. 进入前端文件夹(/FCloud3.AppFront/FCloud3Front)，在命令行中输入`npm install`和`npm run build`
3. 双击项目根目录的sln文件，进入vs
4. 按appsettings.json中的注释调整配置文件
    - 更改数据库连接字符串（如果需要）不作配置默认使用sqlite
    - OSS账号密码（如果需要）不作配置默认使用服务器本地文件存储（Data/FileStorage文件夹）
    - 必须更改总密码（MasterAdminCode）
    - 必须更改jwt密钥（Jwt:SecretKey）
5. 点击顶部绿色启动按钮启动调试，检查是否正常
6. 在浏览器地址栏访问`调试域名/init/{配置文件内的总密码}/initDb`以初始化数据库
7. 停止调试，点击顶部栏`生成-发布`即可选择位置导出
8. 准备服务器环境
    - windows服务器上安装[.net8.0 hosting bundle](https://dotnet.microsoft.com/zh-cn/download/dotnet/8.0) (8.0内尽可能新版)，
    在IIS中新建网站并指向程序目录，为程序目录添加Users用户组完全控制授权
    - linux服务器上安装docker，使用`docker build .`命令取用项目根目录的Dockerfile构建本项目镜像，run时将项目中Data文件夹mount或设为volumn
    （慎用，docker部署未经试验）
9. 尝试启动并进入网站
10. 在浏览器地址栏访问`域名/init/{配置文件内的总密码}/initDb`以初始化数据库

### 注意
本项目暂不成熟，不建议直接投入生产环境  
构建或使用遇到问题欢迎提出issue

### 特别感谢
- 我父母和网友璃夜：帮我渡过困难时期
- 我父亲：给我提供了服务器和域名
- 微软：免费给我使用vs，vsc和.net工具链
- typeScript(也是微软家的)：使我脱离js弱类型苦海
- 尤雨溪等：vue YYDS
- 旋头，滨蜀：积极提供和传达了大量建议
- 我的用户们：鼓励我持续精进技艺

[高性能、多功能的开发平台：.net](https://dotnet.microsoft.com)  
[网站开发必会前端框架：vue](https://vuejs.org)