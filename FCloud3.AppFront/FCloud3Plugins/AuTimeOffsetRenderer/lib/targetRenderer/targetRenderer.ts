import { TargetGroup } from '../targetParser/target'
import { getTargetResult } from './targetResult'
export function renderTargetGroups(targetGroups:TargetGroup[]){
    targetGroups.forEach(renderTargetGroup)
}

export function renderTargetGroup(g:TargetGroup){
    const ele = document.createElement('div')
    ele.className = 'au-time-offset-renderer'
    g.table.parentNode?.insertBefore(ele, g.table)
    g.table.remove()

    const now = +new Date()
    setStyle()
    g.targets.forEach(t=>{
        const child = document.createElement('div')
        ele.appendChild(child)

        let timeStampHere = now
        let updateIntervalMs = t.specifyTimeOfDay ? 1000: 10000
        const updateTargetHtml = ()=>{
            const virtualNow = new Date(timeStampHere)
            const str = getTargetResult(t, virtualNow)
            child.innerHTML = str
            timeStampHere += updateIntervalMs;
        }
        updateTargetHtml()
        window.setInterval(updateTargetHtml, updateIntervalMs)
    })
}

function setStyle(){
    let style = document.getElementById('au-time-offset-renderer-style') as HTMLStyleElement | null
    if (!style) {
        style = document.createElement('style')
        style.id = 'au-time-offset-renderer-style'
        style.textContent = `
            .au-time-offset-renderer > div {
                background-color: cornflowerblue;
                color: white;
                margin: 5px;
                padding: 10px;
                border-radius: 1000px;
            }
            .au-time-offset-renderer > div a {
                color: white;
                text-decoration: underline;
            }
            .au-time-offset-renderer > div a:hover {
                color: white;
            }
        `
        document.head.appendChild(style)
    }
}