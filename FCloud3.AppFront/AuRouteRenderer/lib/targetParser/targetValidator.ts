import { activationCode } from "../common/consts";
import { marksDefined, seperator } from "../common/marks";

type TargetValidationResult = {from:number, cells:string[], config?:string} | undefined

export function isValidTarget(t:HTMLTableElement):TargetValidationResult[]{
    if(t.rows.length<=1){
        return [];
    }
    const validMarks = Object.values(marksDefined) as string[]
    const res:TargetValidationResult[] = []
    let cursor = 0;
    let safety = 100;
    while(true){
        const cells:string[] = []
        let idx = 0;
        let started = false;
        let from = 0;
        let config:string|undefined
        for(idx=cursor;idx<t.rows.length;idx++){
            const r = t.rows[idx]
            if(r.cells.length==0)
                continue;
            const firstCell = r.cells[0]
            if(firstCell.colSpan>1)
                continue;
            const firstCellContent = firstCell.textContent?.trim()
            if(firstCellContent && isValidTargetCell(firstCellContent, validMarks, started)){
                if(!started){
                    from = idx;
                    started = true;
                    const withoutCode = removeActivationCode(firstCellContent)
                    const configInfo = extractConfig(withoutCode)
                    cells.push(configInfo.otherVals)
                    config = configInfo.config
                }
                else
                    cells.push(firstCellContent)
            }else{
                if(started){
                    cursor = idx;
                    break;
                }
            }
        }
        if(cells.length < 2){
            continue;
        }
        res.push({
            from,
            cells,
            config
        })
        if(idx>=t.rows.length){
            break;
        }
        safety--;
        if(safety<=0){
            break;
        }
    }
    return res;
}

function isValidTargetCell(cellTrimmed:string, validMarks:string[], started:boolean){
    if(!started){
        if(!cellTrimmed.startsWith(activationCode)){
            return false;
        }else{
            cellTrimmed = removeActivationCode(cellTrimmed)
            cellTrimmed = cellTrimmed.replace(/^\(.*?\)/, "")
        }
    }
    const splitted = cellTrimmed.split(seperator, 1)
    const firstPart = splitted[0].trim()
    for(let c of firstPart){
        if(!validMarks.includes(c)){
            return false;
        }
    }
    return true
}
function removeActivationCode(cellTrimmed:string){
    return cellTrimmed.substring(activationCode.length).trim()
}
function extractConfig(cellTrimmed:string):{otherVals:string, config?:string}{
    const res = /^\(.*?\)/.exec(cellTrimmed)
    if(res && res.length>0){
        const m = res[0]
        return {
            otherVals: cellTrimmed.substring(m.length),
            config: m
        }
    }
    return {
        otherVals: cellTrimmed
    }
}