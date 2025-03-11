export function stickyContainTableRestrict(){
    document.querySelectorAll('[data-autb-sticky-contain]').forEach((el:any)=>{
        try{
            const table = el as HTMLTableElement
            const parent = table.parentElement
            if(parent){
                parent.style.maxHeight = '55vh'
            }
        }
        catch(e){}
    })
}