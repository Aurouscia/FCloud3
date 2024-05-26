export function watchWindowWidth(callBack:(width:number)=>void):()=>void{
    const doSth = ()=>{
        callBack(window.innerWidth);
    };
    window.addEventListener('resize',doSth)
    return ()=>{
        window.removeEventListener('resize',doSth)
    }
}