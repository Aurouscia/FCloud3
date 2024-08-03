import { PluginExe } from "./common/pluginExe"

const path = '/plugins/AuRouteRenderer/dist.js'

export class RouteRenderer extends PluginExe{
    constructor(container:HTMLDivElement){
        super(container, path)
    }
}