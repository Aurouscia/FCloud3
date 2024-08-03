import { isValidTarget } from "./isValidTarget"
import { Target, TargetConfig, targetConfigDefault } from "../common/target"
import { configKvSeperator, configSeperator, seperator } from "../common/marks"

/*
*   正确目标的每一行应该都有内容（至少有空格）
*   目标的每一行由seperator分为几个部分，第一部分为主要描述，描述这块应该绘制什么东西
*       第二部分及以后是一个或多个标注(annotation)
*   第一行的第一个标注会被试图读取为合法的配置，如果是合法配置就从标注数组中移除
*       配置必须以configSeperator开头和结尾，中间为数个由configSeperator隔开的键值对组成
*       键值对由configKvSeperator分开
*/

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
                for(let char of firstPart){
                    gridHere.push(char.trim())
                }
                for(let i=1;i<parts.length;i++){
                    annoHere.push(parts[i])
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