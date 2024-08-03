import { Point } from "../../common/point";
import { DrawerBase, DrawerContext } from "../drawer";

export type DrawLineType = "regular" | "endTop" | "endBottom" | "transLeft" | "transRight"
export type DrawStationType = "single" | "cross" | "shareLeft" | "shareRight" | "shareBoth"
export class DbgDrawer extends DrawerBase{
    constructor(ctx:DrawerContext){
        super(ctx);
    }
    drawLine(pos: Point, color: string, type: DrawLineType): void {
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
        this.cvs.beginPath()
        this.cvs.strokeStyle = color;
        this.cvs.lineWidth = this.ctx.uPx / 3
        this.cvs.moveTo(from.x,from.y)
        this.cvs.lineTo(to.x,to.y)
        this.cvs.stroke()
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