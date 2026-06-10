import { triggers } from '../public/options.json'

const searchInTitleTagName = 'h1'

function getTargetCellPattern() {
    const pattern = `(?<=(?:${triggers.join('|')})\\().{1,32}(?=\\))`
    return new RegExp(pattern)
}

export function run(){
    const targetCellPattern = getTargetCellPattern()
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
        const matchRes = targetCellPattern.exec(cell.textContent?.trim() ?? '')
        if(matchRes && matchRes[0].length > 0){
            targets.push({
                target: t,
                targetCell: cell,
                param: matchRes[0].trim(),
                resolved: false
            })
        }
    }

    //getElementsByTagName返回的是一个动态集合，ChildNode.remove()之后集合会变化
    //导致for循环跳过元素，因此需要使用数组来存储元素
    const titles = [...document.getElementsByTagName(searchInTitleTagName)]
    for(const title of titles){
        const h1Text = title.textContent ?? ''
        const target = targets.find(t=>{
            return !t.resolved && h1Text.includes(t.param)
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
            target.targetCell.innerHTML = `<b style="color:red">段落移动器：未找到包含[${target.param}]的段落标题</b>`
        }
    }
}
