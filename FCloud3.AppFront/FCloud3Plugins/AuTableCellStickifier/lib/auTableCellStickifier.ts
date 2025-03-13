//匹配“AuTcs(below)”、“AuTcs(right)”、“AuTcs(right, below)”，括号内可以有空字符
const callPattern = /^AuTcs\(\s?[a-z, ]{5,13}\s?\)/g
const stickyContainAttr = "data-autb-sticky-contain"
const stickyTopAttr = "data-autb-sticky-top"
const stickyLeftAttr = "data-autb-sticky-left"
const searchRange = 5;

export function run(){
    const tables = document.getElementsByTagName('table')
    for(const t of tables){
        let rightDetected:{r:number,c:number}|undefined = undefined
        let belowDetected:{r:number,c:number}|undefined = undefined
        const rows = t.rows
        for(let r=0; r<rows.length && r<=searchRange; r++){
            const row = rows[r]
            const cells = row.cells
            for(let c=0; c<cells.length && c<=searchRange; c++){
                const text = cells[c].innerText
                const callCheck = callingMe(text)
                if(callCheck){
                    if(callCheck.right){
                        rightDetected = {r, c}
                    }
                    if(callCheck.below){
                        belowDetected = {r, c}
                    }
                    cells[c].innerText = callCheck.filtered
                }
            }
        }
        if(!rightDetected && !belowDetected){
            break
        }
        if(rightDetected){
            const {r,c} = rightDetected
            const cells = rows[r].cells
            for(let i=c; i<cells.length; i++){
                cells[i].setAttribute(stickyTopAttr, '')
            }
        }
        if(belowDetected){
            const {r,c} = belowDetected
            for(let i=r; i<rows.length; i++){
                rows[i].cells[c].setAttribute(stickyLeftAttr, '')
            }
        }
        t.setAttribute(stickyContainAttr, '')
    }
}

function callingMe(text?:string):{right:boolean, below:boolean, filtered:string}|undefined{
    if(!text){
        return
    }
    const res = callPattern.exec(text)
    if(!res || res.length==0)
        return
    const matched = res[0]
    const filtered = text.replace(callPattern, '').trim()
    const right = matched.includes('right')
    const below = matched.includes('below')
    return {right, below, filtered}
}