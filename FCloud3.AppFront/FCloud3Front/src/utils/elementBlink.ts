import { sleep } from "./sleep";

export async function elementBlink(ele:HTMLElement, color:string, times:number = 3){
    ele.style.transition = '0.5s';
    for(let i = 0; i < times; i++){
        ele.style.backgroundColor = color;
        await sleep(200)
        ele.style.backgroundColor = '';
        await sleep(200)
    }
    ele.style.transition = '';
}

export async function elementBlinkClass(ele:HTMLElement, className:string, times:number = 3){
    for(let i = 0; i < times; i++){
        ele.classList.toggle(className, true)
        await sleep(200)
        ele.classList.toggle(className, false)
        await sleep(200)
    }
}