export const imgClickJumpExcludeClassName = "noClickJumpImg"

export class ImageClickJump{
    private srcOperation:(src:string)=>void
    constructor(cb?:(e:string)=>void){
        if(cb){
            this.srcOperation = cb;
        }else{
            this.srcOperation = this.srcOperationDefault
        }
        this.handlerBinded = this.handler.bind(this);
    }
    private imgs:HTMLImageElement[] = [];
    listen(area:HTMLElement|undefined){
        if(!area){return}
        this.imgs = [];
        const imgsFound = area.getElementsByTagName("img")
        for(const img of imgsFound){
            if(img.classList.contains(imgClickJumpExcludeClassName)){
                continue;
            }
            this.imgs.push(img);
            img.addEventListener("click", this.handlerBinded)
        }
    }
    dispose(){
        this.imgs.forEach(i=>i.removeEventListener("click", this.handlerBinded))
    }
    private handlerBinded:(e:MouseEvent)=>void
    private handler(e:MouseEvent){
        if(e.target && 'src' in e.target && typeof(e.target.src)=='string'){
            console.log(this.srcOperation)
            this.srcOperation(e.target.src)
        }
    }
    private srcOperationDefault(src:string){
        location.assign(src)
    }
}