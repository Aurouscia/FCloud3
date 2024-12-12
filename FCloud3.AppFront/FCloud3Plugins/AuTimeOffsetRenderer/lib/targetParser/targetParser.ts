import { Target, TargetGroup, trigger } from "./target"

export function parseTargets():TargetGroup[]{
    const targetGroups:TargetGroup[] = []
    const tables = document.getElementsByTagName('table')
    for(const table of tables){
        const targetsHere:Target[] = []
        for(const row of table.rows){
            const rowRes = parseTableRow(row)
            if(!rowRes){
                //有异常表格行，放弃该表格，清空目标列表
                targetsHere.splice(0, targetsHere.length)
                break;
            }
            targetsHere.push(rowRes)
        }
        if(targetsHere.length>0){
            const group:TargetGroup = {
                table,
                targets: targetsHere
            }
            targetGroups.push(group)
        }
    }
    return targetGroups
}

const mainCellPattern = `(?<=${trigger}\\()[0-9-/:\\+\\* ]{0,32}(?=\\))`
const mainCellPatternRegex = new RegExp(mainCellPattern)
export function parseTableRow(row:HTMLTableRowElement):Target|false{
    const cellCount = row.cells.length
    if(cellCount < 1 || cellCount > 2){
        return false
    }
    let desc:string|undefined = undefined
    if(cellCount == 2)
        desc = row.cells[1].innerHTML
    const mainText = row.cells[0].innerText
    const matchRes = mainCellPatternRegex.exec(mainText)
    if(!matchRes)
        return false
    const match = [...matchRes].at(0)
    if(!match)
        return false
    const parts = match.split(' ')
    if(parts.length>2 || parts.length==0)
        return false
    const ymd = parseYMD(parts[0])
    if(!ymd)
        return false
    let {y, m, d} = ymd
    let h=0, min=0, s=0
    if(parts.length>1){
        const hms = parseHMS(parts[1])
        if(!hms){
            return false
        }
        h = hms.h
        min = hms.m
        s = hms.s
    }

    const now = new Date();
    const nowYear = now.getFullYear()
    const nowMonth = now.getMonth()+1
    const nowDay = now.getDate()
    if(typeof(y) == 'string'){
        if(y=='*')
            y = nowYear //星号表示当年
        else{
            //加号表示下一年（如果过了当天）
            let passedMD = false
            if(nowMonth > m)
                passedMD = true
            else if(nowMonth === m && nowDay > d)
                passedMD = true
            if(passedMD)
                y = nowYear + 1
            else
                y = nowYear
        }
    }
    return {
        t:new Date(y, m-1, d, h, min, s),
        desc
    }
}
function parseYMD(str:string):{y:number|'*'|'+', m:number, d:number}|undefined{
    const parts = str.split(/[/-]/)
    if(parts.length==2){
        return {
            y:'*',
            m:parseInt(parts[0]),
            d:parseInt(parts[1])
        }
    }
    if(parts.length==3){
        const first = parts[0]
        let y:'*'|'+'|number
        if(first=='*' || first=='+')
            y = first
        else{
            const tryYear = parseInt(first)
            if(isNaN(tryYear))
                return
            y = tryYear
        }
        return {
            y,
            m:parseInt(parts[1]),
            d:parseInt(parts[2])
        }
    }
}
function parseHMS(str:string):{h:number, m:number, s:number}|undefined{
    const parts = str.split(/[:：]/)
    let h = 0, m = 0, s = 0
    if(parts.length>3 || parts.length==0)
        return
    if(parts.length>=1)
        h = parseInt(parts[0])
    if(parts.length>=2)
        m = parseInt(parts[1])
    if(parts.length==3)
        s = parseInt(parts[2])
    return {h,m,s}
}