import { Point } from "../../common/point";
import { DrawerBase, DrawerContext } from "../drawer";

export class DbgDrawer extends DrawerBase{
    constructor(ctx:DrawerContext){
        super(ctx);
    }
    drawLine(pos: Point, color: string, type: "regular" | "endTop" | "endBottom" | "transLeft" | "transRight"): void {
        const {x:tx,y:ty} = super.posToCord(pos,'c','t')
        const {x:bx,y:by} = super.posToCord(pos,'c','b')
        console.log({tx,ty}, {bx,by})
        this.cvs.beginPath()
        this.cvs.strokeStyle = color;
        this.cvs.lineWidth = this.ctx.uPx / 3
        this.cvs.moveTo(tx,ty)
        this.cvs.lineTo(bx,by)
        this.cvs.stroke()
    }
    drawStation(pos: Point, color: string, type: "single" | "cross" | "shareLeft" | "shareRight" | "shareBoth"): void {
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