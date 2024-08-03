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
            const fromPos = {x:pos.x, y:pos.y}
            const toPos = {x:pos.x, y:pos.y}
            if(config && config.topBias){
                fromPos.x += config.topBias
            }
            if(config && config.bottomBias){
                toPos.x += config.bottomBias
            }
            let from = super.posToCord(fromPos,'c','tt')
            let cp0 = super.posToCord(fromPos,'c','c')
            let to = super.posToCord(toPos,'c','bb')
            let cp1 = super.posToCord(toPos,'c','c')
            this.cvs.moveTo(from.x, from.y)
            this.cvs.bezierCurveTo(cp0.x, cp0.y, cp1.x, cp1.y, to.x, to.y)
            this.cvs.stroke()
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