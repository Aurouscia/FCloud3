# 这里是前端可配置常量目录
## 文件说明
- `.env`：基础配置
- `.env.development`：调试时使用的配置  
- `.env.production`：编译时使用的配置  
后两个文件的配置会覆盖`.env`文件的基础配置 

## 如果你希望把内容提交到git：
直接修改上述三个文件，并提交更改

## 如果不希望提交到git：
复制一份文件，并在文件名后添加`.local`  
例如：`.env.production.local`

## 无本地后端时调试前端
创建名为`.env.development.local`的文件，内容写：
```
VITE_ApiUrlBase = "[你的后端域名]"
例如：
VITE_ApiUrlBase = "http://wiki.jowei19.com"
```