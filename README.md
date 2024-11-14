# FCloud3
<img src="https://gitee.com/au114514/fcloud3/raw/master/FCloud3.AppFront/FCloud3Front/public/fcloud.svg" height="200"/>

## 概述
本项目是一套基于`asp.netCore`和`vue3`和内容管理系统，其主要功能介于`博客/WIKI/贴吧`之间  
- 用户可在线编辑文本内容（称为词条）、上传图片等文件。词条支持markdown语法，可插入图片或表格。
    - 词条中的表格可选择上传xlsx文件，或通过内置的表格编辑器编辑。
    - 词条可以自动生成提到的其他词条的链接。
    - 词条内容的变动均有操作记录，可查看每次编辑新增/删去的文字部分。
- 可通过目录系统分门别类整理词条和文件，目录可多层嵌套。
- 作者可自由控制允许编辑的范围，允许或拒绝用户/用户组编辑自己的全部或单个词条/目录，实现互不干扰的团体协作。
- 首页可显示最新更新词条，词条下方有评论区，可成为互动性强的社交平台。

查看主要使用案例：  
http://wiki.jowei19.com

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
2. 进入前端文件夹(/FCloud3.AppFront/FCloud3Front)，在命令行中输入
    ```
    npm config set registry https://registry.npmmirror.com（设置npm国内镜像）
    npm ci
    npm run build
    ```
    如果需要调试前端项目，见前端文件夹内的Readme文件
3. 双击项目根目录的sln文件，进入vs
4. 按appsettings.json中的注释调整配置文件
    - （如果需要）更改数据库连接字符串，不作配置默认使用sqlite
    - （如果需要）阿里云OSS账号密码，不作配置默认使用服务器本地文件存储（Data/FileStorage文件夹）
    - **必须**更改总密码（MasterAdminCode）
    - **必须**更改jwt密钥（Jwt:SecretKey）
5. 点击顶部绿色启动按钮启动调试，检查是否正常
6. 在浏览器地址栏访问`调试域名/init/{配置文件内的总密码}/initDb`以初始化数据库
7. 停止调试，点击顶部栏`生成-发布`即可选择位置导出
8. 准备服务器环境
    - windows服务器上安装[.net8.0 hosting bundle](https://dotnet.microsoft.com/zh-cn/download/dotnet/8.0) (8.0内尽可能新版)，
    在IIS中新建网站并指向程序目录，为程序目录添加Users用户组完全控制授权（或者折腾iis权限，让程序能读写当前目录就行）
    - linux服务器上安装docker，使用`docker build .`命令取用项目根目录的Dockerfile构建本项目镜像，run时将项目中Data文件夹mount或设为volumn
    （慎用，docker部署未经试验）
9. 尝试启动并进入网站
10. 在浏览器地址栏访问`域名/init/{配置文件内的总密码}/initDb`以初始化数据库

## 注意
本项目暂不成熟，不建议直接投入生产环境  
**部署或使用遇到问题欢迎提出issue或者PR**  
需要帮助请加qq群：798877093  
<a target="_blank" href="https://qm.qq.com/cgi-bin/qm/qr?k=4oMUQMONpSEqiV8up23fZ_vUn5OjD9JI&jump_from=webapi&authKey=dJxZX5kBCr46IASe9YH6V9aBvwHG/CNs13kgTm6k4nKYrVcsWI+zndBiypF5H4lW"><img src="//pub.idqqimg.com/wpa/images/group.png" alt="Au的软件开发交流群" title="Au的软件开发交流群"></a>

## 主要缺陷
- 只能单实例部署
- 没有代码生成器和布局设计器，二次开发较为麻烦
- 词条内容为动态加载，无法被搜索引擎抓取（完全没有SEO）

## 特别感谢
- 我父母和网友璃夜：帮我渡过困难时期
- 我父亲：给我提供了服务器和域名
- 微软：免费给我使用vs，vsc和.net工具链
- typeScript(也是微软家的)：使我脱离js弱类型苦海
- 尤雨溪等：vue YYDS
- 旋头，滨蜀：积极提供和传达了大量建议
- 我的用户们：鼓励我持续精进技艺

[高性能、MIT开源、跨平台：.net](https://dotnet.microsoft.com)  
[web开发必会前端框架：vue3](https://vuejs.org)

## 开源许可证
Apache-2.0