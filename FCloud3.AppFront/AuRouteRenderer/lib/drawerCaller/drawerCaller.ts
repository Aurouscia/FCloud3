import { cvsXUnitPx, cvsYUnitPx } from "../common/consts";
import { ValidMark } from "../common/marks";
import { Target } from "../common/target";
import { DbgDrawer, DrawLineType } from "../drawer/Drawer/dbgDrawer";

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
    for(let y=0;y<t.grid.length;y++){
        const gridRow = t.grid[y]
        for(let x=0;x<gridRow.length;x++){
            const mark = gridRow[x] as ValidMark
            if(mark === 'I')
                drawer.drawLine({x,y}, color, "regular")
            else if(mark ==='o'){
                const type = lineType(t.grid, y, x)
                if(type)
                    drawer.drawLine({x,y}, color, type)
                drawer.drawStation({x,y}, color, "single")
            }
        }
    }
}


function lineType(grid:string[][], y:number, x:number):DrawLineType|undefined{
    const rc = grid.length;
    const rl = grid[y].length
    let topConn = true;
    let bottomConn = true;
    let canReachTop = y>0;
    let canReachBottom = y<rc-1;
    let canReachLeft = x>0;
    let canReachRight = x<rl-1;
    if(!canReachTop){
        topConn = false
    }
    else if(grid[y-1][x]==='_'){
        if((canReachLeft && grid[y-1][x-1]==='\\') || (canReachRight && grid[y-1][x+1]==='/')){
        }else{
            topConn = false
        }
    }
    if(!canReachBottom){
        bottomConn = false
    }else if(grid[y+1][x]==='_'){
        if((canReachLeft && grid[y+1][x-1]==='/') || (canReachRight && grid[y+1][x+1]==='\\')){
        }else{
            bottomConn = false
        }
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