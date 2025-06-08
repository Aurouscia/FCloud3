import { execSync } from 'child_process'

const pluginsDir = './FCloud3Plugins'
const mainDir = './FCloud3Front'

console.log('=========开始编译插件=========')
execSync(`node buildPlugins.mjs`, { cwd: pluginsDir, stdio: 'inherit' })
console.log('=========开始编译前端主程序=========')
execSync('npm i', { cwd: mainDir, stdio: 'inherit' })
execSync('npm run build', { cwd: mainDir, stdio: 'inherit' })