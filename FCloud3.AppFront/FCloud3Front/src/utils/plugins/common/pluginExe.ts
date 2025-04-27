import { PluginsFound } from "@/build/plugin/pluginsFound"
import { runJs, runJsFile } from "./runJs"
import pluginsFound from '@/build/plugin/pluginsFound.json'

export const consolePrefix = '[f3-plugin]'

export class PluginExe{
    private state:"checking"|"notAvailable"|"ready" = "checking"
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
    }
    async init(exeImmediately:boolean = true){
        this.state = "checking";
        this.pluginName = this.pluginName
        const pluginList = pluginsFound as PluginsFound
        const plugin = pluginList.find(x=>x.name===this.pluginName)
        this.pluginPath = plugin?.entry
        if(this.pluginPath){
            await runJsFile(this.pluginPath, this.initContainer)
            this.state = "ready";
            console.log(`${consolePrefix}插件${this.pluginName}已初始化`)
            if(exeImmediately)
                await this.execute()
        }else{
            this.state = "notAvailable";
            console.log(`${consolePrefix}插件${this.pluginName}未能找到`)
        }
    }
    async execute(){
        if(this.pluginPath && this.state == 'ready'){
            const codeImport = `
                import {run} from '${this.pluginPath}'
            `
            const code = `
                await run()
            `
            await runJs(code, this.runContainer, codeImport)
            console.log(`${consolePrefix}插件${this.pluginName}已执行`)
        }
    }
}