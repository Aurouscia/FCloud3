export const imgClickJumpExcludeClassName = "noClickJumpImg"
export const imgClickJumpRequireClassName = "clickJumpImg"

export class ImageClickJump{
    private srcOperation:(src:string, alt?:string)=>void
    private requireClassOnly:boolean
    constructor(cb?:typeof this.srcOperation, requireClassOnly?:boolean){
        if(cb){
            this.srcOperation = cb;
        }else{
            this.srcOperation = this.srcOperationDefault
        }
        this.handlerBinded = this.handler.bind(this);
        this.requireClassOnly = !!requireClassOnly
    }
    private imgs:HTMLImageElement[] = [];
    listen(area:HTMLElement|undefined|null){
        if(!area){return}
        this.imgs = [];
        const imgsFound = area.getElementsByTagName("img")
        for(const img of imgsFound){
            if(img.classList.contains(imgClickJumpExcludeClassName)){
                continue;
            }
            if(this.requireClassOnly && !img.classList.contains(imgClickJumpRequireClassName)){
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
            let alt:undefined|string = undefined;
            if('alt' in e.target && typeof(e.target.alt)=='string'){
                alt = e.target.alt;
            }
            this.srcOperation(e.target.src, alt)
        }
    }
    private srcOperationDefault(src:string){
        location.assign(src)
    }
}