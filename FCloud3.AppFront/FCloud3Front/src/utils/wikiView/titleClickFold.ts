const clickables = ['H1','H2','H3','H4','H5','H6']
const clickableFoldedClass = 'hFolded'
const nextIdentifyClass = 'indent'
const nextFoldedClass = 'indentFolded'

export class TitleClickFold{
    listen(){
        window.addEventListener("click",this.clickHandler)
    }
    dispose(){
        window.removeEventListener("click",this.clickHandler)
    }
    clickHandler(e:MouseEvent){
        const ele = e.target as HTMLElement;
        const tagName = ele.tagName;
        if(!tagName || !clickables.includes(tagName)){
            return;
        }
        const next = ele.nextSibling as HTMLElement;
        if(!next){
            return;
        }
        if(next.classList.contains(nextIdentifyClass)){
            next.classList.toggle(nextFoldedClass);
            ele.classList.toggle(clickableFoldedClass);
        }
    }
}