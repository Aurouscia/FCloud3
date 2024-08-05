import { cvsUnitPx, cvsXUnitPx, cvsYUnitPx } from "./common/consts";
import { DbgDrawer } from "./drawer/Drawer/dbgDrawer";
import { callDrawer } from "./drawerCaller/drawerCaller";
import { parseTargets } from "./targetParser/targetParser";
import { reformTarget } from "./targetReformer/targetReformer";

export function run(area?:HTMLElement):void{
    area = area || document.body
    const targets = parseTargets(area)
    targets.forEach(t=>{
        reformTarget(t)

        const ctx = t.cvs?.getContext('2d')
        if(!ctx)
            return
        const drawer = new DbgDrawer({
            cvsctx:ctx,
            uPx: cvsUnitPx,
            xuPx: cvsXUnitPx,
            yuPx: cvsYUnitPx
        })

        callDrawer(t,drawer)
    })
}