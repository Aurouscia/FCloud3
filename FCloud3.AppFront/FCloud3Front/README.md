# FCloud3前端部分
请确保安装了node客户端（npm命令可用）  
安装依赖项并开始调试：
```
npm config set registry https://registry.npmmirror.com （设置npm国内镜像）
npm ci
npm run prebuild （仅第一次run dev前需要）
npm run dev
```
编译：
```
npm run build （到../../FCloud3.App/wwwroot目录）
npm run build-here （到./dist目录）
```
## 连接远程后端调试前端
如果本地没有.netSDK，可在`./env`文件夹下创建名为`.env.development.local`的文件，内容写：
```
VITE_ApiUrlBase = "[你的后端域名]"
例如：
VITE_ApiUrlBase = "http://wiki.jowei19.com"
```
## 插件
见`../FCloud3Plugins`目录的README.md
