import { isValidTarget } from "./targetValidator"
import { Target, TargetConfig, targetConfigDefault } from "../common/target"
import { configKvSeperator, configSeperator, emptyMark, seperator } from "../common/marks"


export function parseTargets(area:HTMLElement):Target[]{
    const targets:Target[] = []
    const tables = area.getElementsByTagName('table')
    for(const t of tables){
        const res = isValidTarget(t)
        if(res){
            const grid:string[][] = [];
            const annotations:string[][] = [];
            let config = targetConfigDefault();
            if(res.config){
                const parsedConfig = parseConfig(res.config)
                if(parsedConfig){
                    config = parsedConfig
                }
            }
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
            })
            const maxRowLength = fillGrid(grid)
            
            let gridTrimmedLengths = []
            for(let i = 0;i<grid.length;i++){
                const rowCount = grid[i].length
                let rowCountTrimmed = rowCount
                for(let c=rowCount-1;c>=0;c--){
                    if(grid[i][c]===emptyMark){
                        rowCountTrimmed--;
                    }else{
                        break;
                    }
                }
                gridTrimmedLengths.push(rowCountTrimmed)
            }
            const newTarget:Target = {
                element:t,
                rowFrom:res.from,
                cells:res.cells,
                grid,
                gridRowCount: grid.length,
                gridColCount: maxRowLength,
                gridTrimmedLengths,
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
    if(s.startsWith("(") && s.endsWith(")")){
        s = s.slice(1, s.length-1)
    }
    const config:TargetConfig = targetConfigDefault()
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
            r.push(emptyMark)
        }
    })
    return maxRowLength
}