import { cvsXUnitPx, cvsYUnitPx } from "../common/consts";
import { Target } from "../common/target";
import { setCanvasStyle, setMainTdStyle, setTableStyle, setTdStyle, setTrStyle } from "./setStyle";

export function reformTarget(t:Target):void{
    const table = t.element
    setTableStyle(table)
    const rowCount = t.cells.length
    const expandTd = table.rows[t.rowFrom].cells[0]
    let xUnitCount = 1;
    for(let i = 0;i<rowCount;i++){
        const gridCount = t.grid[i].length
        const annoCount = t.annotations[i].length
        const sum = gridCount+annoCount;
        if(sum > xUnitCount){
            xUnitCount = sum
        }
    }
    expandTd.rowSpan = rowCount;
    expandTd.innerHTML = ''
    setMainTdStyle(expandTd, xUnitCount)
    const cvs = document.createElement('canvas')
    cvs.width = cvsXUnitPx * xUnitCount;
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