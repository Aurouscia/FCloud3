import { execSync } from 'child_process'
import { resolve } from 'path'

const baseDir = import.meta.dirname
const pluginsDir = resolve(baseDir, 'FCloud3Plugins')
const mainDir = resolve(baseDir, 'FCloud3Front')

console.log('=========开始编译插件=========')
execSync(`node buildPlugins.mjs`, { cwd: pluginsDir, stdio: 'inherit' })
console.log('=========开始编译前端主程序=========')
execSync('pnpm install', { cwd: mainDir, stdio: 'inherit' })
execSync('npm run build', { cwd: mainDir, stdio: 'inherit' })