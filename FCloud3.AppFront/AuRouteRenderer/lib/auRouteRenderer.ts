import { callDrawer } from "./drawerCaller/drawerCaller";
import { parseTargets } from "./targetParser/targetParser";
import { reformTarget } from "./targetReformer/targetReformer";

export function run(area?:HTMLElement):void{
    area = area || document.body
    const targets = parseTargets(area)
    targets.forEach(t=>{
        reformTarget(t)
        callDrawer(t)
    })
}