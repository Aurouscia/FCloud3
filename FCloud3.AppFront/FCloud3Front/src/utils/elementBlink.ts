import { sleep } from "./sleep";

export async function elementBlink(ele:HTMLElement, color:string, times:number = 3, transitionMs:number = 200){
    ele.style.transition = '0.5s';
    for(let i = 0; i < times; i++){
        ele.style.backgroundColor = color;
        await sleep(transitionMs)
        ele.style.backgroundColor = '';
        if(i != times-1)
            await sleep(transitionMs)
    }
    ele.style.transition = '';
}

export async function elementBlinkClass(ele:HTMLElement, className:string, times:number = 3, transitionMs:number = 200){
    for(let i = 0; i < times; i++){
        ele.classList.toggle(className, true)
        await sleep(transitionMs)
        ele.classList.toggle(className, false)
        if(i != times-1)
            await sleep(transitionMs)
    }
}