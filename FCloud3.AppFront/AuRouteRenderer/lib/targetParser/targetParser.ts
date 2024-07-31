import { isValidTarget } from "./isValidTarget"
import { Target } from "../common/target"
import { seperator } from "../common/marks"

export function parseTargets(area:HTMLElement):Target[]{
    const targets:Target[] = []
    const tables = area.getElementsByTagName('table')
    for(const t of tables){
        const res = isValidTarget(t)
        if(res){
            const grid:string[][] = [];
            const annotations:string[][] = [];
            res.cells.forEach(c=>{
                const splitted = c.split(seperator)
                const firstPart = splitted[0]
                const gridHere:string[] = []
                const annoHere:string[] = []
                for(let char of firstPart){
                    gridHere.push(char)
                }
                for(let i=1;i<splitted.length;i++){
                    annoHere.push(splitted[i])
                }
                grid.push(gridHere)
                annotations.push(annoHere)
            })
            const newTarget:Target = {
                element:t,
                rowFrom:res.from,
                cells:res.cells,
                grid,
                annotations
            }
            targets.push(newTarget);
        }
    }
    console.log(targets)
    return targets
}