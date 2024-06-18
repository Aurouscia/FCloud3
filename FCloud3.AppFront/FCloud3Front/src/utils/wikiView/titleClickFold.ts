import foldImg from '@/assets/fold.svg';
const clickables = ['H1','H2','H3','H4','H5','H6']
const clickableFoldedClass = 'hFolded'
const nextIdentifyClass = 'indent'
const nextFoldedClass = 'indentFolded'

export class TitleClickFold{
    listen(target?:HTMLDivElement){
        window.addEventListener("click",this.clickHandler);
        if(!target)return [];
        const titles:HTMLElement[] = [];
        clickables.forEach(tag=>{
            const hs = target.getElementsByTagName(tag);
            for(const h of hs){
                if(!isClickableTitle(h)){continue;}
                titles.push(h as HTMLElement);
                const img = document.createElement('img');
                img.src = foldImg;
                img.className = 'foldImg';
                const textNode = h.childNodes[0];
                if(textNode && isDefaultFolded(textNode.textContent || "")){
                    h.classList.add(clickableFoldedClass);
                    (h.nextSibling as Element).classList.add(nextFoldedClass);
                    textNode.textContent = removeDefaultFoldedMark(textNode.textContent || "");
                }
                h.prepend(img);
            }
        })
        titles.sort((x,y)=>x.offsetTop - y.offsetTop)
        return titles;
    }
    dispose(){
        window.removeEventListener("click",this.clickHandler)
    }
    clickHandler(e:MouseEvent){
        let ele = e.target as HTMLElement;
        if(ele.tagName == "IMG" || ele.tagName == "SPAN" || ele.tagName == "DIV"){
            ele = ele.parentElement as HTMLElement;
        }
        if(isClickableTitle(ele)){
            (ele.nextSibling as Element).classList.toggle(nextFoldedClass);
            ele.classList.toggle(clickableFoldedClass);
        }
    }
}

function isClickableTitle(ele:Element):boolean{
    if(!ele){return false}
    const tagName = ele.tagName;
    if(!tagName || !clickables.includes(tagName)){
        return false;
    }
    const next = ele.nextSibling as Element;
    if(!next || next.tagName !== 'DIV' || !next.classList.contains(nextIdentifyClass)){
        return false;
    }
    return true;
}

export function isDefaultFolded(title:string){
    return title.startsWith("^");
}
export function removeDefaultFoldedMark(title:string):string{
    if(!isDefaultFolded(title)){
        return title;
    }
    return title.slice(1);
}