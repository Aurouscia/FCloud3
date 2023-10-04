import { getTimeStamp } from "./timeStamp";

export class MouseDragListener{
    started:boolean = false
    updateTime:number = 0
    startX:number = 0
    startY:number = 0
    updateInterval:number
    constructor(updateInterval:number = 0.05){
        this.updateInterval = updateInterval;
    }

    private start(ex:number,ey:number){
        this.startX = ex;
        this.startY = ey;
        this.started = true;
        this.updateTime = getTimeStamp();
    }
    private end(ex:number,ey:number,finish:(x:number,y:number)=>void){
        if(this.started){
            const x = ex;
            const y = ey;
            finish(x-this.startX, y-this.startY);
            this.started = false
        }
    }
    private move(ex:number,ey:number,update:(x:number,y:number)=>void){
        if(this.started){
            const x = ex;
            const y = ey;
            const now = getTimeStamp();
            if(now - this.updateTime > this.updateInterval){
                update(x-this.startX, y-this.startY);
                this.updateTime = now;
            }
        }
    }

    startListen(update:(x:number, y:number)=>void,
        finish:(x:number, y:number)=>void,
        working:()=>boolean=()=>{return true})
    {
        window.addEventListener("mousedown",(e:MouseEvent)=>{
            this.start(e.clientX,e.clientY);
            e.preventDefault();
        })
        window.addEventListener("mouseup",(e:MouseEvent)=>{
            this.end(e.clientX,e.clientY,finish)
            e.preventDefault();
        });
        window.addEventListener("mousemove",(e:MouseEvent)=>{
            this.move(e.clientX,e.clientY,update)
            e.preventDefault();
        });

        var cacheX:number;
        var cacheY:number;
        window.addEventListener("touchstart",(e:TouchEvent)=>{
            if(working()){
                this.start(e.touches[0].clientX,e.touches[0].clientY);
                e.preventDefault();
            }
        },{passive:false});
        window.addEventListener("touchend",(e:TouchEvent)=>{
            if(working()){
                this.end(cacheX,cacheY,finish);
                e.preventDefault();
            }
        },{passive:false});
        window.addEventListener("touchmove",(e:TouchEvent)=>{
            if(working()){
                this.move(e.touches[0].clientX,e.touches[0].clientY,update);
                cacheX = e.touches[0].clientX;
                cacheY = e.touches[0].clientY;
                e.preventDefault();
            }
        },{passive:false});
    }
}