import { Target } from "../targetLocator/target";
import { setTdStyle, setTrStyle } from "./setStyle";

export function reformTarget(t:Target){
    const table = t.element
    const expandTd = table.rows[t.rowFrom].cells[0]
    expandTd.rowSpan = t.marks.length;
    expandTd.innerHTML = ''
    for(let i=0; i<t.marks.length; i++){
        const row = table.rows[i+t.rowFrom]
        if(i>0){
            const delTd = row.cells[0];
            row.removeChild(delTd)
        }
        setTrStyle(row)
        for(const td of row.cells){
            setTdStyle(td)
        }
    }
}