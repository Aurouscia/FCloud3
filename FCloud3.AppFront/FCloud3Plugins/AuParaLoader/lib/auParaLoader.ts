import { triggers } from '../public/options.json'
const pattern = `(?<=(${triggers.join('|')})\\().{1,80}(?=\\))`
const targetCellPattern = new RegExp(pattern)

export async function run(){
    const targets:{
        target:HTMLTableElement,
        targetCell:HTMLTableCellElement,
        pathName?:string
        paraSelect?:string
        urlBase?:string
    }[] = []
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
            const params = matchRes[0].split(',')
            const pathName = params.at(0)?.trim()
            const paraSelect = params.at(1)?.trim()
            const urlBase = params.at(2)?.trim()
            targets.push({
                target: t,
                targetCell: cell,
                pathName,
                paraSelect,
                urlBase
            })
        }
    }

    for(const t of targets){
        if(!t.pathName){
            t.targetCell.innerHTML = errmsgHtml(t, '缺少词条路径名')
            continue
        }
        const url = getFetchUrl(t.pathName, t.urlBase)
        const fetchRes = await fetch(url)
        try{
            const obj = await fetchRes.json()
            const paras = obj['Paras'] as Array<any>
            let targetPara:any
            if(t.paraSelect){
                targetPara = paras.find(p=>{
                    return (p['Title'] as string).includes(t.paraSelect!)
                })
            }else{
                targetPara = paras.at(0)
            }
            if(!targetPara){
                t.targetCell.innerHTML = errmsgHtml(t, `找不到指定段落“${t.paraSelect}”`)
                return
            }
            const content = targetPara['Content']
            const newEle = document.createElement('div')
            newEle.setAttribute('loaded-from', url)
            newEle.innerHTML = content
            t.target.parentElement!.insertBefore(newEle, t.target)
            t.target.remove()
        }
        catch(err){
            t.targetCell.innerHTML = errmsgHtml(t, 'http异常')
            console.error(err)
        }
    }
}

function getFetchUrl(pathName:string, base?:string){
    return `${base??''}/api/WikiParsing/GetParsedWiki?pathName=${pathName}`
}
function errmsgHtml(t:{pathName?:string, urlBase?:string}, msg:string){
    return `<b style="color:red">段落加载器：[${t.pathName??'??'} from ${t.urlBase??'/'}] 加载失败 [${msg}]</b>`
}