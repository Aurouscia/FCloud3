import { locateTargets } from "./targetLocator/targetLocator";
import { reformTarget } from "./targetReformer/targetReformer";

export function run(area?:HTMLElement):void{
    area = area || document.body
    const targets = locateTargets(area)
    targets.forEach(t=>reformTarget(t))
}