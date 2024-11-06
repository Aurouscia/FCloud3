//本脚本会在前端项目build前运行，
//在public文件夹（将会被搬到wwwroot文件夹）下的feVersion.txt文件中写入当前时间戳
//用于检查客户端版本是否最新
const fs = require('fs')

module.exports = {
    markVersion: function(){
        const timestamp = String(+(new Date()));
        //会被放到网页根目录
        fs.writeFileSync('public/feVersion.txt', timestamp)


        const code = `export const feVersion = '${timestamp}'`;
        //会被pack到代码里
        fs.writeFileSync('src/build/version/feVersion.js', code)
    }
}