const trigger = 'AuParaMover'
const pattern = `(?<=${trigger}\\().{1,32}(?=\\))`
const targetCellPattern = new RegExp(pattern)

const searchInTitleTagName = 'h1'

export function run(){
    const targets:{
        target:HTMLTableElement,
        targetCell:HTMLTableCellElement,
        param:string,
        resolved:boolean}[] = []
    const tables = document.getElementsByTagName('table')
    for(const t of tables){
        if(t.rows.length!==1)
            continue
        const firstRow = t.rows[0]
        if(firstRow.cells.length!==1)
            continue
        const cell = firstRow.cells[0]
        const matchRes = targetCellPattern.exec(cell.innerText.trim())
        if(matchRes && matchRes.length==1){
            targets.push({
                target: t,
                targetCell: cell,
                param: matchRes[0].trim(),
                resolved: false
            })
        }
    }

    const titles = document.getElementsByTagName(searchInTitleTagName)
    for(const title of titles){
        const h1Text = title.innerText
        const target = targets.find(t=>{
            return h1Text.includes(t.param)
        })
        if(!target)
            continue
        const titleNext = title.nextElementSibling
        if(!titleNext)
            continue
        target.target.parentNode?.insertBefore(titleNext, target.target)
        title.remove()
        target.target.remove()
        target.resolved = true
    }
    for(const target of targets){
        if(!target.resolved){
            target.targetCell.innerHTML = `<b style="color:red">${trigger}：未找到包含[${target.param}]的段落标题</b>`
        }
    }
}