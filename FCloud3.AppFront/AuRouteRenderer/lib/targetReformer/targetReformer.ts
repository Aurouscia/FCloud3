import { cvsLRMarginPx, cvsXUnitPx, cvsYUnitPx } from "../common/consts";
import { Target } from "../common/target";
import { appendStyleTag, setCanvasStyle, setMainTdStyle, setTableStyle, setTdStyle, setTrStyle } from "./setStyle";

export function reformTarget(t:Target):void{
    appendStyleTag()
    const table = t.element
    setTableStyle(table)
    const rowCount = t.cells.length
    const expandTd = table.rows[t.rowFrom].cells[0]
    let xUnitCount = 1;
    for(let i = 0;i<rowCount;i++){
        const annoCount = t.annotations[i].length
        const annoNeedUnits = Math.ceil(annoCount/2)
        const sum = t.gridTrimmedLengths[i]+annoNeedUnits;
        if(sum > xUnitCount){
            xUnitCount = sum
        }
    }
    expandTd.rowSpan = rowCount;
    expandTd.innerHTML = ''
    setMainTdStyle(expandTd, xUnitCount)
    const cvs = document.createElement('canvas')
    cvs.width = cvsXUnitPx * xUnitCount + 2 * cvsLRMarginPx;
    cvs.height = cvsYUnitPx * rowCount
    setCanvasStyle(cvs, rowCount, xUnitCount)
    expandTd.appendChild(cvs)
    for(let i=0; i<rowCount; i++){
        const row = table.rows[i+t.rowFrom]
        if(i>0){
            const delTd = row.cells[0];
            row.removeChild(delTd)
            for(const td of row.cells){
                setTdStyle(td)
            }
        }
        else{
            let isFirst = true
            for(const td of row.cells){
                if(!isFirst){
                    setTdStyle(td)
                }else{
                    isFirst = false
                }
            }
        }
        setTrStyle(row)
    }
    t.cvs = cvs;
}