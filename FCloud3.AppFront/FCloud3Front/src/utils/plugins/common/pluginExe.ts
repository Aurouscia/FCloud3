//import { sleep } from "@/utils/sleep"
import { runJs, runJsFile } from "./runJs"
import { pluginsFound } from '@/build/plugin/pluginsFound'

export class PluginExe{
    private state:"checking"|"notAvailable"|"ready"
    private initContainer:HTMLDivElement
    private runContainer:HTMLDivElement
    private pluginName:string
    private pluginPath?:string
    constructor(container:HTMLDivElement, pluginName:string, onReady?:()=>void){
        this.pluginName = pluginName
        this.initContainer = document.createElement('div')
        this.runContainer = document.createElement('div')
        container.appendChild(this.initContainer)
        container.appendChild(this.runContainer)
        this.state = "checking";
        this.pluginName = pluginName
        this.pluginPath = pluginsFound[pluginName]
        if(this.pluginPath){
            runJsFile(this.pluginPath, this.initContainer).then(_=>{
                this.state = "ready";
                console.log(`插件${pluginName}已初始化`)
                if(onReady)
                    onReady()
            })
        }else{
            this.state = "notAvailable";
            console.log(`插件${pluginName}未能找到`)
        }
    }
    execute(){
        if(this.pluginPath && this.state == 'ready'){
            const code = `
                import {run} from '${this.pluginPath}'
                run()
            `
            runJs(code, this.runContainer)
            console.log(`插件${this.pluginName}已执行`)
        }
    }
}