import { sleep } from "../sleep"
import { runJs, runJsFile } from "./common/runJs"

const path = '/plugins/AuRouteRenderer/dist.js'

export class RouteRenderer{
    private state:"checking"|"notAvailable"|"ready"
    private initContainer:HTMLDivElement
    private runContainer:HTMLDivElement
    constructor(container:HTMLDivElement){
        this.initContainer = document.createElement('div')
        this.runContainer = document.createElement('div')
        container.appendChild(this.initContainer)
        container.appendChild(this.runContainer)
        this.state = "checking";
        runJsFile(path, this.initContainer).then(res=>{
            if(res){
                this.state = "ready";
            }else{
                this.state = "notAvailable";
            }
        })
    }
    async run(){
        for(let i=0;i<50;i++){
            if(this.state != 'checking'){
                this.execute();
                break;
            }
            await sleep(100)
        }
    }
    private execute(){
        const code = `
            import {sayHello} from '${path}'
            sayHello()
        `
        runJs(code, this.runContainer)
    }
}