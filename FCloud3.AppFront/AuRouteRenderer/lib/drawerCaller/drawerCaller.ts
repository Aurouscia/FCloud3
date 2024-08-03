import { cvsXUnitPx, cvsYUnitPx } from "../common/consts";
import { ValidMark } from "../common/marks";
import { Target } from "../common/target";
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
    for(let y=0;y<t.grid.length;y++){
        const isFirstRow = y===0;
        const isLastRow = y===t.grid.length-1
        const gridRow = t.grid[y]
        for(let x=0;x<gridRow.length;x++){
            const mark = gridRow[x] as ValidMark
            if(mark === 'I')
                drawer.drawLine({x,y}, color, "regular")
            else if(mark ==='o'){
                if(isFirstRow || t.grid[y-1].length <= x || t.grid[y-1][x]===' '){
                    drawer.drawLine({x,y}, color, "endTop")
                }else if(isLastRow || t.grid[y-1].length <= x || t.grid[y-1][x]===' '){
                    drawer.drawLine({x,y}, color, "endBottom")
                }else{
                    drawer.drawLine({x,y}, color, "regular")
                }
                drawer.drawStation({x,y}, color, "single")
            }
        }
    }
}