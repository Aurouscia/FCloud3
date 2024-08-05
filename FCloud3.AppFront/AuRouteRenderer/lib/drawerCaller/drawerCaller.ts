import { cvsXUnitPx, cvsYUnitPx } from "../common/consts";
import { emptyMark, isTransMark, lineMark, staMark, transLeftMark, transRightMark, ValidMark } from "../common/marks";
import { gridNeighbor, gridNeighborEmpty, Target } from "../common/target";
import { DrawLineConfig, DrawLineType } from "../drawer/drawer";
import { DbgDrawer } from "../drawer/Drawer/dbgDrawer";

export function callDrawer(t:Target){
    const ctx = t.cvs?.getContext('2d')
    if(!ctx){
        return
    }
    const drawer = new DbgDrawer({
        cvsctx:ctx,
        uPx: cvsXUnitPx,
        xuPx: cvsXUnitPx,
        yuPx: cvsYUnitPx
    })
    const color = t.config.c
    const enumerateGrid = (cb:(x:number,y:number,mark:ValidMark)=>void)=>{
        for(let y=0;y<t.grid.length;y++){
            const gridRow = t.grid[y]
            for(let x=0;x<gridRow.length;x++){
                const mark = gridRow[x] as ValidMark
                cb(x,y,mark)
            }
        }
    }
    enumerateGrid((x,y,mark)=>{
        if(mark === lineMark){
            const type = staLineType(t.grid, y, x)
            if(type)
                drawer.drawLine({x,y}, color, type)
        }else if(isTransMark(mark)){
            const param = transLineType(t.grid, y, x)
            if(param){
                drawer.drawLine({x,y}, color, param.type, {
                    topBias:param.topBias,
                    bottomBias:param.bottomBias,
                    topShrink: param.topShrink,
                    bottomShrink:param.bottomShrink
                })
            }
        }
    })
    enumerateGrid((x,y,mark)=>{
        if(mark ==='o'){
            const type = staLineType(t.grid, y, x)
            if(type)
                drawer.drawLine({x,y}, color, type)
            drawer.drawStation({x,y}, color, "single")
        }
    })
}


function staLineType(grid:string[][], y:number, x:number):DrawLineType|undefined{
    let topConn = true;
    let bottomConn = true;
    const upNei = gridNeighbor(grid, x, y, "middle", "up")
    const downNei = gridNeighbor(grid, x, y, "middle", "down")
    if(!upNei || upNei===emptyMark || isTransMark(upNei)){
        topConn = false
    }
    if(!downNei || downNei===emptyMark || isTransMark(downNei)){
        bottomConn = false
    }
    if(topConn){
        if(bottomConn)
            return 'regular'
        return 'endBottom'
    }
    if(bottomConn)
        return 'endTop'
    return undefined
}

function transLineType(grid:string[][], y:number, x:number)
        :(DrawLineConfig & {type:DrawLineType})|undefined{
    const trans = grid[y][x]
    if(trans===transLeftMark){
        let topBias = 1
        let bottomBias = -1
        let topShrink = false
        if(gridNeighborEmpty(grid, x, y, "right", "up") && !gridNeighborEmpty(grid, x, y, "middle", "up")){
            topBias = 0
        }
        if(gridNeighborEmpty(grid, x, y, "left", "down") && !gridNeighborEmpty(grid, x, y, "middle", "down")){
            bottomBias = 0
        }
        if(topBias == 0 && bottomBias == -1 && gridNeighbor(grid,x,y,"left","middle") === staMark){
            topShrink = true
        }
        return {type:'trans', topBias, bottomBias, topShrink}
    }
    if(trans===transRightMark){
        let topBias = -1
        let bottomBias = 1
        let bottomShrink = false
        if(gridNeighborEmpty(grid, x, y, "left", "up") && !gridNeighborEmpty(grid, x, y, "middle", "up")){
            topBias = 0
        }
        if(gridNeighborEmpty(grid, x, y, "right", "down") && !gridNeighborEmpty(grid, x, y, "middle", "down")){
            bottomBias = 0
        }
        if(topBias == -1 && bottomBias === 0 && gridNeighbor(grid,x,y,"left","middle") === staMark){
            bottomShrink = true
        }
        return {type:'trans', topBias, bottomBias, bottomShrink}
    }
    return undefined
}