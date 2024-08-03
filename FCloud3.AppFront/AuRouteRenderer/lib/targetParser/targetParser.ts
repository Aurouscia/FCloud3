import { isValidTarget } from "./targetValidator"
import { Target, TargetConfig, targetConfigDefault } from "../common/target"
import { configKvSeperator, configSeperator, seperator } from "../common/marks"


export function parseTargets(area:HTMLElement):Target[]{
    const targets:Target[] = []
    const tables = area.getElementsByTagName('table')
    for(const t of tables){
        const res = isValidTarget(t)
        if(res){
            const grid:string[][] = [];
            const annotations:string[][] = [];
            let isFirstCell = true;
            let config = targetConfigDefault;
            res.cells.forEach(c=>{
                const parts = c.split(seperator)
                const firstPart = parts[0]
                const gridHere:string[] = []
                const annoHere:string[] = []
                for(let char of firstPart.trim()){
                    gridHere.push(char)
                }
                for(let i=1;i<parts.length;i++){
                    const anno = parts[i].trim()
                    if(anno)
                        annoHere.push(anno)
                }
                grid.push(gridHere)
                annotations.push(annoHere)
                if(isFirstCell){
                    isFirstCell = false;
                    if(annoHere.length>0){
                        const seemsConfig = annoHere[0]
                        const cfg = parseConfig(seemsConfig)
                        console.log(cfg)
                        if(cfg){
                            config = cfg
                            annoHere.shift()
                        }
                    }
                }
            })
            fillGrid(grid)
            const newTarget:Target = {
                element:t,
                rowFrom:res.from,
                cells:res.cells,
                grid,
                annotations,
                config
            }
            targets.push(newTarget);
        }
    }
    console.log(targets)
    return targets
}

function parseConfig(s:string):TargetConfig|undefined{
    if(!s.startsWith(configSeperator) || !s.endsWith(configSeperator)){
        return undefined
    }
    const config:TargetConfig = targetConfigDefault
    const parts = s.split(configSeperator)
    parts.forEach(p=>{
        if(p.includes(configKvSeperator)){
            const kv = p.split(configKvSeperator,2)
            config[kv[0].trim()] = kv[1]
        }
    })
    return config
}

function fillGrid(grid:string[][]){
    let maxRowLength = 0;
    grid.forEach(r=>{
        if(r.length>maxRowLength){
            maxRowLength = r.length
        }
    })
    grid.forEach(r=>{
        const needFillCount = maxRowLength - r.length;
        for(let i=0;i<needFillCount;i++){
            r.push('_')
        }
    })
}