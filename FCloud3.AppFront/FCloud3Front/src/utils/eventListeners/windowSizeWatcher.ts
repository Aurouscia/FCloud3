export function watchWindowWidth(callBack:(width:number)=>void, afterStopMs?:number):()=>void{
    let timer = 0;
    const doSth = ()=>{
        window.clearTimeout(timer)
        timer = window.setTimeout(()=>{
            callBack(window.innerWidth);
        }, afterStopMs||0)
    };
    window.addEventListener('resize',doSth)
    return ()=>{
        window.removeEventListener('resize',doSth)
    }
}