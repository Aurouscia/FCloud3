const targetTagName = 'A';
const targetAttrName = 'pathName'

export class WikiLinkClick{
    routerAction: (path:string)=>void;
    constructor(routerAction:(path:string)=>void){
        this.routerAction = routerAction;
        this.clickHandler = this.clickHandler.bind(this);
    }
    listen(target?:HTMLDivElement){
        if(!target)return [];
        const links = target.getElementsByTagName(targetTagName);
        for(const link of links){
            if(isWikiLink(link)){
                (link as HTMLElement).addEventListener("click",this.clickHandler);
            }
        }
    }
    clickHandler(e:MouseEvent){
        let ele = e.target as HTMLElement;
        const path = ele.attributes.getNamedItem(targetAttrName)?.value;
        if(path){
            this.routerAction(path);
        }
    }
}

function isWikiLink(ele:Element){
    const attr = ele.attributes.getNamedItem(targetAttrName);
    if(attr && attr.value){
        return true
    }
    return false;
}