import { cvsLRMarginPx } from "../common/consts"
import { Point, PointLoose } from "../common/point"

export interface DrawerContext{
    cvsctx:CanvasRenderingContext2D,
    uPx:number
    xuPx:number
    yuPx:number
    lineWidth:number
    xUnitCount:number
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
export type DrawBranchType = "upper"|"lower"
export interface DrawBranchConfig{
    lineWidthRatio?:number
}
export type DrawButtType = 'up'|'down'
export interface DrawButtConfig{
    lineWidthRatio?:number
}
export type DrawTurnType = 'top'|'bottom'
export interface DrawTurnConfig{
    lineWidthRatio?:number
    widthBlocks?:number
}

export interface Drawer{
    ctx:DrawerContext;
    drawLine(pos:Point, color:string, type:DrawLineType, config?:DrawLineConfig):void
    drawStation(pos:Point, color:string, type:DrawStationType, config?:DrawStationConfig):void
    drawStation(pos: Point, color: string, type: DrawStationType): void
    drawIcon(pos:Point, bgColor: string, text:string, type: DrawIconType): void
    drawRiver(pos:Point):void
    drawBranch(pos:Point, color: string, type:DrawBranchType, config?:DrawBranchConfig):void
    drawButt(pos: Point, color:string, type: DrawButtType, config?:DrawButtConfig):void
    drawTurn(leftPos: Point, color:string, type:DrawTurnType, config?:DrawTurnConfig):void
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
    abstract drawBranch(pos: Point, color:string, type: DrawBranchType, config?:DrawBranchConfig): void
    abstract drawButt(pos: Point, color:string, type: DrawButtType, config?:DrawButtConfig):void
    abstract drawTurn(leftPos: Point, color:string, type:DrawTurnType, config?:DrawTurnConfig):void
    protected posToCord(pos:Point, xbias:"c"|"l"|"r", ybias:"c"|"t"|"tt"|"b"|"bb", offset?:PointLoose){
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

        let xOffset = 0
        let yOffset = 0
        if(offset){
            xOffset = offset.x || 0
            yOffset = offset.y || 0
        }
        
        return {
            x:(pos.x+xbiasNum)*this.ctx.xuPx + cvsLRMarginPx + xOffset,
            y:(pos.y+ybiasNum)*this.ctx.yuPx + yOffset
        }
    }
}