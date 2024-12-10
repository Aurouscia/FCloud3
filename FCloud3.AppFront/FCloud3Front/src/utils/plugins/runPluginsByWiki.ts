import { PluginsFound } from "@/build/plugin/pluginsFound"
import { consolePrefix, PluginExe } from "./common/pluginExe"
import pluginsFound from '@/build/plugin/pluginsFound.json'

export function runPluginsByWiki(wikiStringData:string|undefined){
    if(!wikiStringData)
        return
    const exes:PluginExe[] = [];
    const pluginList = pluginsFound as PluginsFound
    pluginList.forEach(plugin=>{
        if(wikiStringData.includes(plugin.trigger)){
            const name = plugin.name
            const eleId = containerDivId(name)
            let ele = document.getElementById(eleId) as HTMLDivElement
            if(!ele){
                ele = document.createElement('div')
                ele.id = eleId
                ele.style.display = 'none'
                document.body.appendChild(ele)
            }
            const exe = new PluginExe(ele, name)
            exes.push(exe)
        }
    })
    if(exes.length>0)
        console.log(`${consolePrefix}检测到插件调用：${exes.map(x=>x.pluginName).join(', ')}`)
    else
        console.log(`${consolePrefix}未检测到插件调用`)
    return
}

function containerDivId(pluginName:string){
    return `plugin_${pluginName}`
}