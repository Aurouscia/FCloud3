export function getTopRelativeToClass(of:HTMLElement, relClassName:string){
    let safe = 10
    let focus:HTMLElement = of;
    let sum = 0;
    while(safe>0){
        if(focus.classList.contains(relClassName)){
            break;
        }
        sum += focus.offsetTop;
        if(focus.offsetParent && 'offsetTop' in focus.offsetParent)
            focus = focus.offsetParent as HTMLElement
        else
            break;
        safe--
    }
    console.log(sum)
    return sum
}