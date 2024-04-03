# FCloud3
## 概述
本项目是一套知识库系统，功能类似于mediawiki，遵循Apache-2.0开源协议，可以私有部署和商用。
## 架构
1. 后端基于[asp.netCore](https://dotnet.microsoft.com/zh-cn/apps/aspnet)
2. 数据库使用[EntityFramework](https://learn.microsoft.com/zh-cn/ef/)，将会支持sqlite，sqlserver，mysql，postgreSql
3. 前端使用[vue3](https://vuejs.org)+[ts](https://typescriptlang.org)+[vite](https://vite.dev)
4. 身份验证使用[JWT](https://jwt.io)
5. 图像处理使用[ImageSharp](https://sixlabors.com/products/imagesharp/)
6. excel和word文件导入导出使用[NPOI](https://www.nuget.org/packages/NPOI)

## 安装
### 前提条件
1. [Visual Studio](https://visualstudio.microsoft.com/zh-hans/) 尽可能新版 + web应用开发负载
2. [node客户端](https://nodejs.org/en) 尽可能新版，并确认命令行中有npm命令可用
3. （可选）[git客户端](https://git-scm.com/downloads) 用来下载代码和记录改动
4. （可选）visual studio code用来编辑前端代码
5. 关于网页应用的基本常识

### 步骤
1. 下载代码文件  
    1-1.如果使用git，在命令行中输入`git clone 【本仓库链接】`
2. 进入前端文件夹(/FCloud3.AppFront/FCloud3Front)，在命令行中输入`npm install`和`npm run build`
3. 双击项目根目录的sln文件，进入vs
4. 按appsettings.json中的注释调整配置文件，更改数据库连接字符串（如果需要）和OSS账号密码（如果需要）
5. 点击顶部栏进入`工具-nuget包管理器-包管理器控制台`，输入`update-database`
6. 点击顶部绿色启动按钮启动调试，检查是否正常
7. 停止调试，点击顶部栏`生成-发布`即可选择位置导出
8. windows服务器上安装[.net8.0 hosting bundle](https://dotnet.microsoft.com/zh-cn/download/dotnet/8.0) (8.0内尽可能新版), linux的另外找教程，*“部署asp.netCore应用”*
9. 把导出的程序移动到服务器上，并给予Users用户组该文件夹的控制权限，用IIS新建网站并指向该文件夹
10. 尝试启动并进入网站

### 注意
本项目暂未完成开发，不建议直接投入生产环境  
遇到问题欢迎提出issue