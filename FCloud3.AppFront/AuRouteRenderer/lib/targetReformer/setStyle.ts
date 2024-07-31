import { displayYUnitPx, displayXUnitPx } from "../common/consts"

export function setTableStyle(table:HTMLTableElement){
    table.style.borderCollapse = 'collapse'
}
export function setTrStyle(tr:HTMLTableRowElement){
    tr.style.height = `${displayYUnitPx}px`
}
export function setMainTdStyle(td:HTMLTableCellElement, xUnitCount:number){
    td.style.width = `${displayXUnitPx*xUnitCount}px`
    td.style.padding = '0px'
}
export function setTdStyle(td:HTMLTableCellElement){
    td.style.whiteSpace = 'nowrap'
}
export function setCanvasStyle(cvs:HTMLCanvasElement, rowCount:number, xUnitCount:number){
    cvs.style.width = `${displayXUnitPx*xUnitCount}px`
    cvs.style.height = `${displayYUnitPx*rowCount}px`
}