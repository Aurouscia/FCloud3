import { autoTextColor } from "../../common/colorUtil";
import { airportCalls, exchangeStaColor } from "../../common/consts";
import { Point } from "../../common/point";
import { drawAirport } from "../../common/specialIconDraw";
import { DrawBranchConfig, DrawBranchType, DrawButtConfig, DrawButtType, DrawerBase, DrawerContext, DrawIconType, DrawLineConfig, DrawLineType, DrawStationConfig, DrawStationType, DrawTurnConfig, DrawTurnType } from "../drawer";


export class DbgDrawer extends DrawerBase{
    constructor(ctx:DrawerContext){
        super(ctx);
    }
    drawLine(pos: Point, color: string, type: DrawLineType, config?:DrawLineConfig): void {
        this.cvs.beginPath()
        this.cvs.strokeStyle = color;
        this.cvs.lineWidth = this.ctx.lineWidth
        if(config?.lineWidthRatio){
            this.cvs.lineWidth *= config.lineWidthRatio
        }
        if(type=='regular'||type=='endTop'||type=='endBottom'){
            let from:Point = {x:0,y:0} 
            let to:Point = {x:0,y:0} 
            if(type=='regular'){
                from = super.posToCord(pos,'c','t')
                to = super.posToCord(pos,'c','b')
            }
            else if(type=='endTop'){
                from = super.posToCord(pos,'c','c')
                to = super.posToCord(pos,'c','b')
            }
            else if(type=='endBottom'){
                from = super.posToCord(pos,'c','t')
                to = super.posToCord(pos,'c','c')
            }
            this.cvs.moveTo(from.x,from.y)
            this.cvs.lineTo(to.x,to.y)
            this.cvs.stroke()
        }else if(type=='trans'){
            const fromPos = {x:pos.x, y:pos.y}
            const toPos = {x:pos.x, y:pos.y}
            if(config && config.topBias){
                fromPos.x += config.topBias
            }
            if(config && config.bottomBias){
                toPos.x += config.bottomBias
            }
            let fromYbias:'t'|'tt' = config?.topShrink ? 't' : 'tt'
            let toYbias:'b'|'bb' = config?.bottomShrink ? 'b' : 'bb'
            let from = super.posToCord(fromPos,'c', fromYbias)
            let to = super.posToCord(toPos,'c',toYbias)
            let cp0x = from.x
            let cp0y = (from.y + to.y) / 2
            let cp1x = to.x
            let cp1y = cp0y
            this.cvs.moveTo(from.x, from.y)
            this.cvs.bezierCurveTo(cp0x, cp0y, cp1x, cp1y, to.x, to.y)
            this.cvs.stroke()
            if(config?.topShrink){
                this.cvs.beginPath()
                let fromFill = super.posToCord(fromPos,'c','tt')
                this.cvs.moveTo(from.x, from.y)
                this.cvs.lineTo(fromFill.x, fromFill.y)
                this.cvs.stroke()
            }
            if(config?.bottomShrink){
                this.cvs.beginPath()
                let toFill = super.posToCord(toPos,'c','bb')
                this.cvs.moveTo(to.x, to.y)
                this.cvs.lineTo(toFill.x, toFill.y)
                this.cvs.stroke()
            }
        }
    }
    drawStation(pos: Point, color: string, type: DrawStationType, config?:DrawStationConfig): void {
        if(type=='single'){
            const {x,y} = super.posToCord(pos,'c','c')
            const radius = this.ctx.uPx / 4 * (config?.radiusRatio || 1)
            this.cvs.beginPath()
            this.cvs.strokeStyle = color
            this.cvs.fillStyle = 'white'
            this.cvs.lineWidth = radius / 3 
            this.cvs.arc(x, y, radius, 0, 2*Math.PI)
            this.cvs.fill()
            if(!config?.noStroke)
                this.cvs.stroke()
        }
        else if(type=='cross'){
            const {x,y} = super.posToCord(pos,'c','c')
            const radius = this.ctx.uPx / 4 * (config?.radiusRatio || 1)
            this.cvs.beginPath()
            this.cvs.fillStyle = 'white'
            this.cvs.lineWidth = radius * 0.8
            this.cvs.arc(x, y, radius*1.2, 0, 2*Math.PI)
            this.cvs.fill()
            if(!config?.noStroke){
                this.cvs.strokeStyle = 'white'
                this.cvs.stroke()
                this.cvs.lineWidth = radius / 3
                this.cvs.strokeStyle = exchangeStaColor
                this.cvs.stroke()
                this.cvs.lineCap = 'round'
                this.cvs.lineWidth = radius / 4
                this.cvs.beginPath()
                this.cvs.arc(x, y, radius*0.6, 0, 0.6*Math.PI)
                this.cvs.stroke()
                this.cvs.beginPath()
                this.cvs.arc(x, y, radius*0.6, Math.PI, 1.6*Math.PI)
                this.cvs.stroke()
            }
        }
    }
    drawIcon(pos: Point, bgColor: string, text: string, type:DrawIconType):void{
        let {x,y} = super.posToCord(pos, 'c', 'c')
        if(type=='upper'){
            y+=this.ctx.uPx/2.4
        }else if(type=='lower'){
            y-=this.ctx.uPx/2.4
        }
        const radius = this.ctx.uPx / 2.7
        this.cvs.beginPath()
        this.cvs.fillStyle = bgColor
        this.cvs.arc(x,y,radius, 0, 2*Math.PI)
        this.cvs.fill()

        //特殊字符串处理
        if(airportCalls.includes(text)){
            this.cvs.lineWidth = radius/10
            this.cvs.strokeStyle = 'white'
            drawAirport(this.cvs, x, y, radius*0.9)
            return
        }

        this.cvs.textAlign = 'center'
        this.cvs.textBaseline = 'middle'
        let fontSize = radius*1.5
        if(text.length>3)
            text = text.substring(0,3)
        let length = 10000
        let vertBias = 0;
        while(true){
            this.cvs.font = `${fontSize}px Calibri 微软雅黑`
            const mea = this.cvs.measureText(text)
            let asc = mea.actualBoundingBoxAscent
            let dsc = mea.actualBoundingBoxDescent
            vertBias = (asc-dsc)/2
            length = mea.width
            if(length>radius*1.8){
                fontSize -= 0.5
            }else{
                break;
            }
        }
        
        const textColor = autoTextColor(bgColor)
        this.cvs.fillStyle = textColor
        this.cvs.fillText(text, x, y+vertBias)
    }
    drawTerrain(pos: Point, color:string): void {
        let {x:fx,y:fy} = this.posToCord(pos,'l','c')
        let {x:tx,y:ty} = this.posToCord(pos,'r','c')
        if(pos.x == 0){
            fx = 0
        }
        if(pos.x == this.ctx.xUnitCount-1){
            tx = this.cvs.canvas.width
        }
        const riverWidth = this.ctx.uPx;
        this.cvs.lineWidth = riverWidth;
        this.cvs.strokeStyle = color
        this.cvs.beginPath()
        this.cvs.moveTo(fx,fy);
        this.cvs.lineTo(tx,ty)
        this.cvs.stroke()
    }
    drawBranch(pos: Point, color: string, type: DrawBranchType, config?:DrawBranchConfig): void {
        const branchWidth = this.ctx.lineWidth * 0.5;
        const xOffset = (this.ctx.lineWidth - branchWidth)/2
        const sizeShrink = this.ctx.lineWidth / 7
        const vertPos:'t'|'b' = type=='upper' ? 't' : 'b' 
        const fyOffset = type=='upper' ? sizeShrink : -sizeShrink
        let {x:fx,y:fy} = this.posToCord(pos, 'c', vertPos, {x:xOffset, y:fyOffset})
        let {x:cx,y:cy} = this.posToCord(pos, 'c', 'c', {x:xOffset})
        let {x:tx,y:ty} = this.posToCord(pos, 'c', 'c', {x:xOffset+this.ctx.yuPx/2-sizeShrink})
        this.cvs.beginPath()
        this.cvs.strokeStyle = color;
        this.cvs.lineWidth = branchWidth * (config?.lineWidthRatio || 1)
        this.cvs.moveTo(fx, fy);
        this.cvs.quadraticCurveTo(cx,cy,tx,ty)

        const arrowSize = branchWidth*1.9;
        let ex = this.cvs.canvas.width - arrowSize 
        let ey = cy
        this.cvs.lineTo(ex, ey)
        this.cvs.stroke()
        this.cvs.beginPath()
        this.cvs.moveTo(ex, ey - arrowSize*0.6)
        this.cvs.lineTo(ex, ey + arrowSize*0.6)
        this.cvs.lineTo(ex+arrowSize, ey)
        this.cvs.lineTo(ex, ey - arrowSize*0.6)
        this.cvs.fillStyle = color
        this.cvs.fill()
    }
    drawButt(pos: Point, color: string, type: DrawButtType, config?:DrawButtConfig): void {
        let a:{x:number,y:number} = {x:0,y:0}
        let b:{x:number,y:number} = {x:0,y:0}
        let c:{x:number,y:number} = {x:0,y:0}
        const drawButt = ()=>{
            this.cvs.beginPath()
            this.cvs.moveTo(a.x, a.y)
            this.cvs.lineTo(b.x, b.y)
            this.cvs.lineTo(c.x, c.y)
            this.cvs.closePath()
            this.cvs.fillStyle = color
            this.cvs.lineWidth = this.ctx.lineWidth * (config?.lineWidthRatio||0)
            this.cvs.lineJoin = 'miter'
            this.cvs.stroke()
            this.cvs.fill()
        }
        if(type=='down'){
            a = super.posToCord(pos, 'c', 'c', {x:-0.8*this.ctx.lineWidth})
            b = super.posToCord(pos, 'c', 'c', {x:0.8*this.ctx.lineWidth})
            c = super.posToCord(pos, 'c', 'c', {y:2*this.ctx.lineWidth})
            drawButt()
        }
        else if(type=='up'){
            a = super.posToCord(pos, 'c', 'c', {x:-0.8*this.ctx.lineWidth})
            b = super.posToCord(pos, 'c', 'c', {x:0.8*this.ctx.lineWidth})
            c = super.posToCord(pos, 'c', 'c', {y:-2*this.ctx.lineWidth})
            drawButt()
        }
    }
    drawTurn(pos: Point, color:string, type: DrawTurnType, config?: DrawTurnConfig): void {
        const leftCord = this.posToCord(pos, 'c', 'c')
        const rightPos = {...pos}
        rightPos.x += (config?.widthBlocks||2)-1;
        const rightCord = this.posToCord(rightPos, 'c', 'c')
        const centerCord = {x: (leftCord.x+rightCord.x)/2, y:leftCord.y }
        const radius = (rightCord.x - leftCord.x)/2
        let startAngle = 0;
        let endAngle = Math.PI;
        if(type=='top'){
            startAngle = Math.PI;
            endAngle = Math.PI*2
        }
        this.cvs.beginPath()
        this.cvs.arc(centerCord.x, centerCord.y, radius, startAngle, endAngle)
        this.cvs.lineWidth = this.ctx.lineWidth * (config?.lineWidthRatio||1)
        this.cvs.strokeStyle = color
        this.cvs.stroke()
    }
}