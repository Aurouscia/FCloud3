import { cvsXUnitPx, cvsYUnitPx } from "../common/consts";
import { ValidMark } from "../common/marks";
import { Target } from "../common/target";
import { DrawLineType } from "../drawer/drawer";
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
        if(mark === 'l'){
            drawer.drawLine({x,y}, color, "regular")
        }else if(mark === '/'||mark==='\\'){
            const param = transLineType(t.grid, y, x)
            if(param){
                drawer.drawLine({x,y}, color, param.type, {
                    topBias:param.topBias,
                    bottomBias:param.bottomBias
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
    const rc = grid.length;
    let topConn = true;
    let bottomConn = true;
    let canReachTop = y>0;
    let canReachBottom = y<rc-1;
    if(!canReachTop){
        topConn = false
    }
    else if(grid[y-1][x]==='_'){
        topConn = false
    }
    if(!canReachBottom){
        bottomConn = false
    }else if(grid[y+1][x]==='_'){
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
        :{type:DrawLineType, topBias:number, bottomBias:number}|undefined{
    const trans = grid[y][x]
    const rl = grid[y].length
    let canReachLeft = x>0;
    let canReachRight = x<rl-1;
    if(trans==='/'){
        if(canReachRight && canReachLeft){
            return {type:'trans', topBias:1, bottomBias:-1}
        }
    }
    if(trans==='\\'){
        if(canReachRight && canReachLeft){
            return {type:'trans', topBias:-1, bottomBias:1}
        }
    }
    return undefined
}