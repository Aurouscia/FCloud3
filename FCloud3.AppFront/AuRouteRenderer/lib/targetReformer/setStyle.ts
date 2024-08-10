import { displayYUnitPx, displayXUnitPx, displayLRMargin } from "../common/consts"

const tableClassName = "aurtrd"
const tableRowClassName = "aurtrd-row"
const tdContentWrapperClassName = 'aurtrd-tdwrapper'
export function appendStyleTag(){
    const st = document.createElement('style')
    st.innerText = styleCode;
    document.body.appendChild(st)
}
export function setTableStyle(table:HTMLTableElement){
    table.style.borderCollapse = 'collapse'
    table.classList.toggle(tableClassName, true)
}
export function setMainTdStyle(td:HTMLTableCellElement, xUnitCount:number){
    td.style.width = `${displayXUnitPx*xUnitCount + 2*displayLRMargin}px`
    td.style.padding = '0px'
}
export function setTrStyle(tr:HTMLTableRowElement){
    tr.classList.toggle(tableRowClassName, true)
}
export function setTdStyle(td:HTMLTableCellElement){
    const div = document.createElement('div')
    div.classList.toggle(tdContentWrapperClassName, true)
    div.innerHTML = td.innerHTML
    td.innerHTML = ''
    td.appendChild(div)
    let needWrapP = false;
    if(div.childNodes.length>1)
        for(let c of div.childNodes){
            if('tagName' in c && c.tagName == 'P'){
                continue;
            }
            needWrapP = true;
            break;
        }
    if(needWrapP){
        const p = document.createElement('p')
        p.innerHTML = div.innerHTML;
        div.innerHTML = '';
        div.appendChild(p)
    }
}
export function setCanvasStyle(cvs:HTMLCanvasElement, rowCount:number, xUnitCount:number){
    cvs.style.width = `${displayXUnitPx*xUnitCount + 2*displayLRMargin}px`
    cvs.style.height = `${displayYUnitPx*rowCount}px`
    cvs.style.margin = '0px'
    cvs.style.verticalAlign = 'middle' //否则canvas底部出现空隙撑大表格
}

const styleCode = `
    .aurtrd-row td, .aurtrd-row td *{
        white-space:nowrap !important;
        text-align:left !important;
    }
    .aurtrd-row td, .aurtrd-row td p{
        padding: 0px !important;
        margin: 0px !important;
    }
    .aurtrd .aurtrd-tdwrapper{
        height:${displayYUnitPx}px !important;
        padding: 0px !important;
        margin: 0px 10px 0px 10px !important;
        display: flex;
        flex-direction: column;
        justify-content: center;
        align-items: flex-start;
        overflow: auto !important;
    }
`