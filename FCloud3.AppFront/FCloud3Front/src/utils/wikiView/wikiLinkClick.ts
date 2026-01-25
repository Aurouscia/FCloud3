const targetTagName = 'A';
const targetAttrName = 'pathName'
const redLinkClassName = 'redLink'

export class WikiLinkClick{
    getHref: (pathName:string)=>string
    redLinkAction: (pathName:string, name:string)=>void
    constructor(getHref:(pathName:string)=>string, redLinkAction:(pathName:string, name:string)=>void){
        this.getHref = getHref;
        this.redLinkAction = redLinkAction;
        this.redLinkClickHandler = this.redLinkClickHandler.bind(this);
    }
    listen(target?:HTMLDivElement|null){
        if(!target)return [];
        const links = target.getElementsByTagName(targetTagName);
        let converted = 0
        for(const link of links){
            const attrs = readAttrs(link)
            if(attrs.isWikiLink){
                if(attrs.isRedLink){
                    (link as HTMLElement).addEventListener("click",this.redLinkClickHandler);
                }
                else{
                    (link as HTMLAnchorElement).href = this.getHref(attrs.path)
                }
                converted++
            }
        }
        console.log(`转化 ${converted} 个pathName链接`)
    }
    redLinkClickHandler(e:MouseEvent){
        let ele = e.target as HTMLElement;
        const path = ele.attributes.getNamedItem(targetAttrName)?.value;
        const name = ele.innerText;
        if(path){
            this.redLinkAction(path, name);
        }
    }
}

function readAttrs(ele:Element){
    let isWikiLink = false;
    let path = ''
    const attr = ele.attributes.getNamedItem(targetAttrName);
    if(attr && attr.value){
        path = attr.value;
        isWikiLink = true
        ele.attributes.removeNamedItem(targetAttrName);
    }
    const isRedLink = ele.classList.contains(redLinkClassName);
    return{
        path,
        isWikiLink,
        isRedLink
    }
}
