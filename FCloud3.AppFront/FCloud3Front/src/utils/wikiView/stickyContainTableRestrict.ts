/**
 * au-table-editor 以及某些插件会把 sticky 设置写入特定的属性中，此处检测 data-autb-sticky-contain，对其外层限制高度
 */

export function stickyContainTableRestrict(){
    document.querySelectorAll('[data-autb-sticky-contain]').forEach((el:any)=>{
        try{
            const table = el as HTMLTableElement
            const parent = table.parentElement
            if(parent){
                parent.style.maxHeight = '55vh'
                parent.style.overflow = 'auto'
            }
        }
        catch(e){}
    })
}