//本脚本会在前端项目build前运行，
//在public文件夹（将会被搬到wwwroot文件夹）下的feVersion.txt文件中写入当前时间戳
//用于检查客户端版本是否最新

const fs = require('fs')

const timestamp = String(+(new Date()));
fs.writeFileSync('public/feVersion.txt', timestamp)

const code = `export const feVersion = '${timestamp}'`;
fs.writeFileSync('src/build/feVersion.js', code)