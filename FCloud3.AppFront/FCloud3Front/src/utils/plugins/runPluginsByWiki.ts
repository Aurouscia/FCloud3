import { PluginsFound } from "@/build/plugin/pluginsFound"
import pluginsFound from '@/build/plugin/pluginsFound.json'

const consolePrefix = '[f3-plugin]'

export async function runPluginsByWiki(wikiStringData:(string|undefined)[]|undefined){
    if(!wikiStringData || wikiStringData.length==0)
        return
    const pluginList = pluginsFound as PluginsFound
    const detectedPlugins:string[] = []
    for(const plugin of pluginList){
        if(wikiStringData.some(x=>!!x && x.includes(plugin.trigger))){
            detectedPlugins.push(plugin.name)
            const { name: pluginName, entry: pluginPath } = plugin
            if(pluginPath){
                try{
                    if('runFCloud3Plugin' in window && typeof window.runFCloud3Plugin === 'function'){
                        await window.runFCloud3Plugin(pluginPath)
                        console.log(`${consolePrefix}插件${pluginName}已运行`)
                    }
                    else{
                        throw new Error('未找到插件运行功能（猴子补丁window.runFCloud3Plugin）')
                    }
                }
                catch(err){
                    console.warn(`${consolePrefix}插件${pluginName}运行失败`, err)
                }
            }
            else{
                console.log(`${consolePrefix}插件${pluginName}未能找到`)
            }
        }
    }
    if(detectedPlugins.length>0)
        console.log(`${consolePrefix}检测到插件调用：${detectedPlugins.join('、')}`)
    else
        console.log(`${consolePrefix}未检测到插件调用`)
    return
}