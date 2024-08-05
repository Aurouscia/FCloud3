import { Point } from "../common/point"

export interface DrawerContext{
    cvsctx:CanvasRenderingContext2D,
    uPx:number
    xuPx:number
    yuPx:number
    lineWidth:number
}

export type DrawLineType = "regular" | "endTop" | "endBottom" | "trans"
export interface DrawLineConfig{
    topBias?:number,
    bottomBias?:number,
    topShrink?:boolean,
    bottomShrink?:boolean,
    lineWidthRatio?:number
}
export type DrawStationType = "single" | "cross" | "shareLeft" | "shareRight" | "shareBoth"
export interface DrawStationConfig{
    noStroke?:boolean,
    radiusRatio?:number
}
export type DrawIconType = "middle" | "upper"| "lower"

export interface Drawer{
    ctx:DrawerContext;
    drawLine(pos:Point, color:string, type:DrawLineType, config?:DrawLineConfig):void
    drawStation(pos:Point, color:string, type:DrawStationType, config?:DrawStationConfig):void
    drawStation(pos: Point, color: string, type: DrawStationType): void
    drawIcon(pos:Point, bgColor: string, text:string, type: DrawIconType): void
    drawRiver(pos:Point):void
}

export abstract class DrawerBase implements Drawer{
    ctx: DrawerContext;
    cvs: CanvasRenderingContext2D
    constructor(ctx:DrawerContext){
        this.cvs = ctx.cvsctx
        this.ctx = ctx
    }
    abstract drawLine(pos: Point, color: string, type: DrawLineType, config?:DrawLineConfig): void
    abstract drawStation(pos: Point, color: string, type: DrawStationType, config?:DrawStationConfig): void
    abstract drawIcon(pos:Point, bgColor: string, text:string, type: DrawIconType): void
    abstract drawRiver(pos: Point): void
    protected posToCord(pos:Point, xbias:"c"|"l"|"r", ybias:"c"|"t"|"tt"|"b"|"bb"){
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
        else if(ybias=='bb')
            ybiasNum = 1.5
        else if(ybias=='tt')
            ybiasNum = -0.5
        return {
            x:(pos.x+xbiasNum)*this.ctx.xuPx,
            y:(pos.y+ybiasNum)*this.ctx.yuPx
        }
    }
}