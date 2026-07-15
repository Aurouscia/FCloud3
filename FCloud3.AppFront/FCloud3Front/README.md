# FCloud3前端部分
请确保安装了node客户端（npm命令可用）  
安装依赖项并开始调试：
```
npm install -g pnpm --registry=https://registry.npmmirror.com（如果没有pnpm需要）
pnpm install（仅第一次run dev前需要）
npm run build （仅第一次run dev前需要）
npm run dev
```
编译：
```
npm run build （到../../FCloud3.App/wwwroot目录）
npm run build-here （到./dist目录）
```

## 拉取代码后
如果有包更新，需要重新 pnpm install

## 连接远程后端调试前端
如果本地没有.netSDK，可在`./env`文件夹下创建名为`.env.development.local`的文件，内容写：
```
VITE_ApiUrlBase = "[你的后端域名]"
例如：
VITE_ApiUrlBase = "http://wiki.jowei19.com"
```

## 插件
见`../FCloud3Plugins`目录的README.md

## 渲染器资源自托管
Wiki 内容中的代码高亮、数学公式、Mermaid 图表依赖 KaTeX、PrismJS、Mermaid 三个外部库。生产环境为了避免依赖 unpkg 等公共 CDN，这些资源通过脚本下载到 `public/renderers/` 下，由后端直接提供静态文件服务。

### 生成自托管资源
```
pnpm collect-renderers
```

该脚本会：
- 从 npm registry / unpkg 下载对应版本的资源
- 输出到 `public/renderers/`，目录名包含版本号，例如：
  - `katex@0.17.0/`
  - `mermaid@11.15.0/`
  - `prismjs@1.30.0/`

### 版本号与缓存
目录名中的版本号用于防止客户端缓存旧资源。升级版本时，旧的版本目录不会自动删除，可以手动清理；新版本的目录路径不同，浏览器会重新请求。

### 引用位置
生成后，前端通过以下位置引用本地资源：
- `index.html`：KaTeX、PrismJS 的 CSS
- `src/utils/wikiView/renderWikiContent.ts`：三个库的 JS

升级版本时，需要同步更新上述文件中的版本号。

### Git
`public/renderers/` 下的版本化资源目录已加入 `.gitignore`，不会被提交；只有 `public/renderers/collect-renderers.ts` 脚本本身会随仓库维护。
