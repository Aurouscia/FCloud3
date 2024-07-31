import { cvsXUnitPx, cvsYUnitPx } from "../common/consts";
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
    for(let y=0;y<t.grid.length;y++){
        const gridRow = t.grid[y]
        for(let x=0;x<gridRow.length;x++){
            drawer.drawLine({x,y}, "#ff0000", "regular")
            drawer.drawStation({x,y}, "#ff0000", "single")
        }
    }
}