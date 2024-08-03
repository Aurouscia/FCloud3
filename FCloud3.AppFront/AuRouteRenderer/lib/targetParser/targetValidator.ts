import { activationCode } from "../common/consts";
import { marksDefined, seperator } from "../common/marks";

type TargetValidationResult = {from:number, cells:string[]} | undefined

export function isValidTarget(t:HTMLTableElement):TargetValidationResult{
    if(t.rows.length<=1){
        return undefined;
    }
    let started = false;
    let from = 0;
    const validMarks = Object.values(marksDefined) as string[]
    const cells:string[] = []
    for(let idx=0;idx<t.rows.length;idx++){
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
                cells.push(removeActivationCode(firstCellContent))
            }
            else
                cells.push(firstCellContent)
        }else{
            if(started){
                break;
            }
        }
    }
    if(cells.length < 3){
        return undefined
    }
    return {
        from,
        cells
    }
}

function isValidTargetCell(cellTrimmed:string, validMarks:string[], started:boolean){
    if(!started){
        if(!cellTrimmed.startsWith(activationCode)){
            return false;
        }else{
            cellTrimmed = removeActivationCode(cellTrimmed)
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