import { displayUnitPx } from "../common/consts"

export function setTableStyle(table:HTMLTableElement){
    table.style.borderCollapse = 'collapse'
}
export function setTrStyle(tr:HTMLTableRowElement){
    tr.style.height = `${displayUnitPx}px`
}
export function setMainTdStyle(td:HTMLTableCellElement){
    td.style.width = `${displayUnitPx}px`
    td.style.padding = '0px'
}
export function setTdStyle(td:HTMLTableCellElement){
    td.style.whiteSpace = 'nowrap'
}
export function setCanvasStyle(cvs:HTMLCanvasElement, rowCount:number){
    cvs.style.width = `${displayUnitPx}px`
    cvs.style.height = `${displayUnitPx*rowCount}px`
}