import { execSync } from 'child_process'
import fs from 'fs'
import path from 'path'
import { pluginsEnabled } from './buildPluginsOptions.mjs';

function getPluginsEnabledDirs(directory) {
    return fs.readdirSync(directory)
        .filter(item => {
            if(pluginsEnabled.includes(item)){
                const itemPath = path.join(directory, item);
                return fs.lstatSync(itemPath).isDirectory()
            }
            return false
        });
}

function runPnpmBuild(directory) {
    console.log(`=========正在编译: ${directory}=========`);
    execSync('pnpm build', { cwd: directory, stdio: 'inherit' })
}

const frontPluginsPath = '../FCloud3Front/public/plugins'

function cleanOutputDir() {
    const absPath = path.resolve(frontPluginsPath)
    if(!fs.existsSync(absPath)){
        return
    }
    const items = fs.readdirSync(absPath)
    for(const item of items){
        const itemPath = path.join(absPath, item)
        const stat = fs.lstatSync(itemPath)
        if(stat.isDirectory() && item !== 'runPlugin.js'){
            fs.rmSync(itemPath, { recursive: true })
            console.log(`已清理残留插件目录: ${item}`)
        }
    }
}

async function main() {
    const currentDir = process.cwd(); // 获取当前工作目录
    const pluginDirs = getPluginsEnabledDirs(currentDir);

    if (pluginDirs.length === 0) {
        console.log('没有启用的插件');
        return;
    }
    
    cleanOutputDir()

    console.log('=========开始安装依赖=========')
    execSync('pnpm install', { cwd: currentDir, stdio: 'inherit' })

    console.log('=========开始编译所有插件=========');
    for (const subdir of pluginDirs) {
        const subdirPath = path.join(currentDir, subdir);
        try {
            runPnpmBuild(subdirPath);
        } catch (error) {
            console.error(`目录 ${subdirPath} 编译出错:`, error);
        }
    }
    console.log('=========执行完毕=========');
}

main().catch(err => console.error('脚本运行出错:', err));