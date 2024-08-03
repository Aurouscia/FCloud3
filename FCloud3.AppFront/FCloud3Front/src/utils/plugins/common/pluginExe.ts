import { sleep } from "@/utils/sleep"
import { runJs, runJsFile } from "./runJs"

export class PluginExe{
    private state:"checking"|"notAvailable"|"ready"
    private initContainer:HTMLDivElement
    private runContainer:HTMLDivElement
    private path:string
    constructor(container:HTMLDivElement, path:string){
        this.path = path
        this.initContainer = document.createElement('div')
        this.runContainer = document.createElement('div')
        container.appendChild(this.initContainer)
        container.appendChild(this.runContainer)
        this.state = "checking";
        runJsFile(path, this.initContainer).then(res=>{
            if(res){
                this.state = "ready";
                console.log(`插件${path}已初始化`)
            }else{
                this.state = "notAvailable";
                console.log(`插件${path}未能找到`)
            }
        })
    }
    async run(){
        if(this.state=='ready'){
            this.execute()
            return;
        }
        if(this.state=='notAvailable'){
            return;
        }
        for(let i=0;i<50;i++){
            if(this.state!='checking'){
                if(this.state!='notAvailable'){
                    this.execute();
                }
                break;
            }
            await sleep(100)
        }
    }
    private execute(){
        const code = `
            import {run} from '${this.path}'
            run()
        `
        runJs(code, this.runContainer)
        console.log(`${this.path}已执行`)
    }
}