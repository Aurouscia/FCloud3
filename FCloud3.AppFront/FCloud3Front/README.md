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
## 插件
约定：
- 插件的入口是单个的js文件（ecma标准），**export一个名为`run`的函数**
- 插件编译到`public/plugins/[插件名]`目录下
- 插件编译时需要在文件名中带哈希，避免更新后客户端不更新
