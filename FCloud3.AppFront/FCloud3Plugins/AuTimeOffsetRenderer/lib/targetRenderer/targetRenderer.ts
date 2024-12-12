import { TargetGroup } from '../targetParser/target'
import { getTargetResult } from './targetResult'
export function renderTargetGroups(targetGroups:TargetGroup[]){
    targetGroups.forEach(renderTargetGroup)
}

export function renderTargetGroup(g:TargetGroup){
    const ele = document.createElement('div')
    g.table.parentNode?.insertBefore(ele, g.table)
    g.table.remove()

    const now = +new Date()
    g.targets.forEach(t=>{
        const child = document.createElement('div')
        ele.appendChild(child)
        setStyle(child)

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

function setStyle(eleChild:HTMLElement){
    eleChild.style.backgroundColor = 'cornflowerblue'
    eleChild.style.color = 'white'
    eleChild.style.margin = '5px'
    eleChild.style.padding = '10px'
    eleChild.style.borderRadius = '1000px'
}