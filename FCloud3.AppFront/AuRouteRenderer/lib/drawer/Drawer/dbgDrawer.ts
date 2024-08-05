import { Point } from "../../common/point";
import { DrawerBase, DrawerContext, DrawLineConfig, DrawLineType, DrawStationType } from "../drawer";


export class DbgDrawer extends DrawerBase{
    constructor(ctx:DrawerContext){
        super(ctx);
    }
    drawLine(pos: Point, color: string, type: DrawLineType, config?:DrawLineConfig): void {
        this.cvs.beginPath()
        this.cvs.strokeStyle = color;
        this.cvs.lineWidth = this.ctx.uPx / 3
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
            console.log(config)
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
    drawStation(pos: Point, color: string, type: DrawStationType): void {
        if(type=='single'){
            const {x,y} = super.posToCord(pos,'c','c')
            const radius = this.ctx.uPx / 4
            this.cvs.beginPath()
            this.cvs.strokeStyle = color
            this.cvs.fillStyle = 'white'
            this.cvs.lineWidth = radius / 3
            this.cvs.moveTo(x + radius, y)
            this.cvs.arc(x, y, radius, 0, 2*Math.PI)
            this.cvs.fill()
            this.cvs.stroke()
        }
    }
}