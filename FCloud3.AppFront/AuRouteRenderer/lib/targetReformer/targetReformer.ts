import { cvsUnitPx } from "../common/consts";
import { Target } from "../common/target";
import { setCanvasStyle, setMainTdStyle, setTableStyle, setTdStyle, setTrStyle } from "./setStyle";

export function reformTarget(t:Target):HTMLCanvasElement{
    const table = t.element
    setTableStyle(table)
    const rowCount = t.marks.length
    const expandTd = table.rows[t.rowFrom].cells[0]
    expandTd.rowSpan = rowCount;
    expandTd.innerHTML = ''
    setMainTdStyle(expandTd)
    const cvs = document.createElement('canvas')
    cvs.width = cvsUnitPx;
    cvs.height = cvsUnitPx*rowCount
    setCanvasStyle(cvs, rowCount)
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
    return cvs;
}