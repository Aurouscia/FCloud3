import { marksDefined, ValidMarks } from "../common/marks";

type TargetValidationResult = {from:number, marks:ValidMarks[]} | undefined

export function isValidTarget(t:HTMLTableElement):TargetValidationResult{
    if(t.rows.length<=1){
        return undefined;
    }
    let started = false;
    let from = 0;
    const validMarks = Object.values(marksDefined) as string[]
    const foundMarks:ValidMarks[] = []
    for(let idx=0;idx<t.rows.length;idx++){
        const r = t.rows[idx]
        if(r.cells.length==0)
            continue;
        const firstCell = r.cells[0]
        if(firstCell.colSpan>1)
            continue;
        const firstCellContent = firstCell.textContent?.trim()
        console.log(idx,firstCellContent)
        if(firstCellContent && validMarks.includes(firstCellContent)){
            if(!started){
                from = idx;
                started = true;
            }
            foundMarks.push(firstCellContent as ValidMarks)
        }else{
            if(started){
                break;
            }
        }
    }
    if(foundMarks.length < 3){
        return undefined
    }
    return {
        from,
        marks: foundMarks
    }
}