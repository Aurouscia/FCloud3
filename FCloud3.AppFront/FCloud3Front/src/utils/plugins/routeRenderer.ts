import { PluginExe } from "./common/pluginExe"

const pluginName = 'AuRouteRenderer'

export class RouteRenderer extends PluginExe{
    constructor(container:HTMLDivElement, onReady?:()=>void){
        super(container, pluginName, onReady)
    }
}