import foldImg from '@/assets/fold.svg';
import { imgClickJumpExcludeClassName } from './imgClickJump';
const clickables = ['H1','H2','H3','H4','H5','H6']
const clickableFoldedClass = 'hFolded'
const nextIdentifyClass = 'indent'
const nextFoldedClass = 'indentFolded'
export const hiddenSubClassName = 'hiddenSub'

export class TitleClickFold{
    clickHandlerBinded:(e:MouseEvent)=>void;
    constructor(){
        this.clickHandlerBinded = this.clickHandler.bind(this)
    }
    listen(target?:HTMLDivElement){
        window.addEventListener("click",this.clickHandlerBinded);
        if(!target)return [];
        const titles:HTMLElement[] = [];
        const needProcessDefaultFold:{h:Element, text:ChildNode}[] = []
        clickables.forEach(tag=>{
            const hs = target.getElementsByTagName(tag);
            for(const h of hs){
                if(!isClickableTitle(h)){continue;}
                titles.push(h as HTMLElement);
                const img = document.createElement('img');
                img.src = foldImg;
                img.classList.add('foldImg');
                img.classList.add(imgClickJumpExcludeClassName);
                const textNode = h.childNodes[0];
                if(textNode && isDefaultFolded(textNode.textContent || "")){
                    needProcessDefaultFold.push({h, text:textNode})
                }
                if(h.tagName != 'H1'){
                    const span = document.createElement('span');
                    span.innerHTML = h.innerHTML;
                    h.innerHTML = '';
                    h.prepend(span);
                }
                h.prepend(img);
            }
        })
        titles.sort((x,y)=>x.offsetTop - y.offsetTop)
        needProcessDefaultFold.forEach(({h, text:textNode})=>{
            this.tryToggleElement(h as HTMLElement)
            textNode.textContent = removeDefaultFoldedMark(textNode.textContent || "");
        })
        return titles;
    }
    dispose(){
        window.removeEventListener("click",this.clickHandlerBinded)
    }
    clickHandler(e:MouseEvent){
        let ele = e.target as HTMLElement;
        this.tryToggleElement(ele)
    }
    tryToggleElement(ele:HTMLElement) {
        if(ele.tagName == "IMG" || ele.tagName == "SPAN" || ele.tagName == "DIV"){
            ele = ele.parentElement as HTMLElement;
        }
        if(isClickableTitle(ele)){
            const currentStatus = ele.classList.contains(clickableFoldedClass);
            const setToStatus = !currentStatus;
            const next = ele.nextSibling as Element
            next.classList.toggle(nextFoldedClass, setToStatus);
            ele.classList.toggle(clickableFoldedClass, setToStatus);
            recursiveSetHiddenSub(next, setToStatus)
        }
        function recursiveSetHiddenSub(ele:Element, hiddenSub:boolean){
            for(const e of ele.children){
                if(clickables.includes(e.tagName))
                    e.classList.toggle(hiddenSubClassName, hiddenSub)
                else
                    recursiveSetHiddenSub(e, hiddenSub)
            }
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

export function findNearestUnhiddenAnces(ele:Element):{ances:Element, text:string}|undefined{
    const parent = ele.parentElement
    if(parent){
        const parentPervSib = parent.previousElementSibling;
        if(parentPervSib && clickables.includes(parentPervSib.tagName)){
            if(!parentPervSib.classList.contains(hiddenSubClassName)){
                let hText = "上级段落"
                for(const c of parentPervSib.children){
                    if(c.tagName == "SPAN"){
                        const t = (c as HTMLElement).innerText
                        if(t){
                            hText = t
                            break
                        }
                    }
                }
                return {ances: parentPervSib, text:hText}
            }
            return findNearestUnhiddenAnces(parentPervSib)
        }
    }
}