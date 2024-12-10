import { PluginsFound } from "@/build/plugin/pluginsFound"
import { runJs, runJsFile } from "./runJs"
import pluginsFound from '@/build/plugin/pluginsFound.json'

export const consolePrefix = '[f3-plugin]'

export class PluginExe{
    private state:"checking"|"notAvailable"|"ready"
    private initContainer:HTMLDivElement
    private runContainer:HTMLDivElement
    public pluginName:string
    private pluginPath?:string
    constructor(container:HTMLDivElement, pluginName:string){
        this.pluginName = pluginName
        this.initContainer = document.createElement('div')
        this.runContainer = document.createElement('div')
        container.innerHTML = ''
        container.appendChild(this.initContainer)
        container.appendChild(this.runContainer)
        this.state = "checking";
        this.pluginName = pluginName
        const pluginList = pluginsFound as PluginsFound
        const plugin = pluginList.find(x=>x.name===pluginName)
        this.pluginPath = plugin?.entry
        if(this.pluginPath){
            runJsFile(this.pluginPath, this.initContainer).then(_=>{
                this.state = "ready";
                console.log(`${consolePrefix}插件${pluginName}已初始化`)
                this.execute()
            })
        }else{
            this.state = "notAvailable";
            console.log(`${consolePrefix}插件${pluginName}未能找到`)
        }
    }
    execute(){
        if(this.pluginPath && this.state == 'ready'){
            const code = `
                import {run} from '${this.pluginPath}'
                run()
            `
            runJs(code, this.runContainer)
            console.log(`${consolePrefix}插件${this.pluginName}已执行`)
        }
    }
}