import { isValidTarget } from "./isValidTarget"
import { Target } from "../common/target"

export function locateTargets(area:HTMLElement):Target[]{
    const targets:Target[] = []
    const tables = area.getElementsByTagName('table')
    for(const t of tables){
        const res = isValidTarget(t)
        if(res){
            targets.push({
                element:t,
                rowFrom:res.from,
                marks:res.marks
            })
        }
    }
    console.log(targets)
    return targets
}