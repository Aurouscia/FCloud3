import { Target } from "../targetParser/target";

export interface TimeOffset{
    day:number,
    hour:number,
    min:number,
    sec:number
}

export function getTargetResult(target:Target, timeNow:Date){
    if(!target.specifyTimeOfDay){
        if(isSameDay(target.t, timeNow)){
            return `${target.desc} <b>即为今日</b>`
        }
    }
    let tmsDiff = +target.t - (+timeNow)
    let relation = ''
    if(target.desc){
        if(tmsDiff >= 0)
            relation = `距离 ${target.desc} 剩余`
        else{
            relation = `距离 ${target.desc} 已过`
            tmsDiff = -tmsDiff
        }
    }else{
        if(tmsDiff >= 0)
            relation = `剩余`
        else{
            relation = `已过`
            tmsDiff = -tmsDiff
        }
    }
    const to = msToTimeOffset(tmsDiff)
    const str = timeOffsetToString(to, target.specifyTimeOfDay)
    return `${relation} <b>${str}</b>`
}

const fl = Math.floor
export function msToTimeOffset(tms:number):TimeOffset{
    const tsec = fl(tms/1000)
    const tmin = fl(tsec/60)
    const thour = fl(tmin/60)
    const day = fl(thour/24)
    const hour = thour % 24
    const min = tmin % 60
    const sec = tsec % 60
    return {
        day, hour, min, sec
    }
}
export function timeOffsetToString(to:TimeOffset, specifyTimeOfDay:boolean){
    if(specifyTimeOfDay){
        const hour = padTo2Digits(to.hour)
        const min = padTo2Digits(to.min)
        const sec = padTo2Digits(to.sec)
        let res = to.day > 0 ? `${to.day}天 `: ''
        return res + `${hour}:${min}:${sec}`
    }
    return `${to.day + 1}天`
}

export function padTo2Digits(num:number){
    if(num<10)
        return `0${num}`
    return num.toString()
}
export function isSameDay(t1:Date, t2:Date){
    return t1.getFullYear() === t2.getFullYear()
        && t1.getMonth() === t2.getMonth()
        && t1.getDate() === t2.getDate()
}