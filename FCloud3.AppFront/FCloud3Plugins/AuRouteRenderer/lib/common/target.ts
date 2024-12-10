import { emptyMark, isTransMark, noActiveLink } from "./marks"

export interface Target{
    element:HTMLTableElement
    rowFrom:number
    cells:string[]
    grid:string[][]
    gridTrimmedLengths:number[]
    gridRowCount:number
    gridColCount:number
    annotations:string[][]
    config:TargetConfig
    cvs?:HTMLCanvasElement
}
export type TargetConfig = {
    c:string
} & Record<string,string|null|undefined>
export function targetConfigDefault():TargetConfig{
    return{
        c:"#ff0000"
    }
}

export function gridNeighbor<T>(grid: T[][], x:number, y:number, 
        xb:"left"|"middle"|"right", yb:"up"|"middle"|"down"):T|undefined {
    const rc = grid.length;
    const rl = grid[y].length
    let canReachTop = y>0;
    let canReachBottom = y<rc-1;
    let canReachLeft = x>0;
    let canReachRight = x<rl-1;
    let xbNum = 0
    let ybNum = 0
    if(xb=="left" && !canReachLeft)
        return undefined
    else if(xb=="right" && !canReachRight)
        return undefined
    if(yb=="up" && !canReachTop)
        return undefined
    if(yb=="down" && !canReachBottom)
        return undefined
    if(xb=="left")
        xbNum = -1
    else if(xb=="right")
        xbNum = 1
    if(yb=="up")
        ybNum = -1
    else if(yb=="down")
        ybNum = 1
    return grid[y+ybNum][x+xbNum]
}
export function gridNeighborEmpty(grid: string[][], x:number, y:number, 
        xb:"left"|"middle"|"right", yb:"up"|"middle"|"down"):boolean {
    const res = gridNeighbor(grid, x, y, xb, yb);
    return res === undefined || res === emptyMark
}
export function gridNeighborActiveLink(grid: string[][], x:number, y:number, 
        xb:"left"|"middle"|"right", yb:"up"|"middle"|"down"):boolean {
    const res = gridNeighbor(grid, x, y, xb, yb);
    return !noActiveLink(res) && res !== emptyMark && !isTransMark(res)
}