import { Point } from "../common/point"

export interface DrawerContext{
    cvsctx:CanvasRenderingContext2D,
    uPx:number
    xuPx:number
    yuPx:number
}
export interface Drawer{
    ctx:DrawerContext;
    drawLine(pos:Point, color:string, type:"regular"|"endTop"|"endBottom"|"transLeft"|"transRight", transStep?:number):void
    drawStation(pos:Point, color:string, type:"single"|"cross"|"shareLeft"|"shareRight"|"shareBoth"):void
}

export abstract class DrawerBase implements Drawer{
    ctx: DrawerContext;
    cvs: CanvasRenderingContext2D
    constructor(ctx:DrawerContext){
        this.cvs = ctx.cvsctx
        this.ctx = ctx
    }
    abstract drawLine(pos: Point, color: string, type: "regular" | "endTop" | "endBottom" | "transLeft" | "transRight", transStep?: number): void
    abstract drawStation(pos: Point, color: string, type: "single" | "cross" | "shareLeft" | "shareRight" | "shareBoth"): void
    protected posToCord(pos:Point, xbias:"c"|"l"|"r", ybias:"c"|"t"|"b"){
        let xbiasNum = 0;
        if(xbias=='c')
            xbiasNum = 0.5;
        else if(xbias=='r')
            xbiasNum = 1
        let ybiasNum = 0;
        if(ybias=='c')
            ybiasNum = 0.5;
        else if(ybias=='b')
            ybiasNum = 1
        return {
            x:(pos.x+xbiasNum)*this.ctx.xuPx,
            y:(pos.y+ybiasNum)*this.ctx.yuPx
        }
    }
}