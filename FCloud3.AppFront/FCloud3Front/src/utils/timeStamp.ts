import { padStart } from "lodash"

export function getTimeStamp(){
    return (new Date()).getTime()/1000
}
export function getTimeStampMs(){
    return (new Date()).getTime()
}
export class TimedLock{
    ms:number
    time:number
    constructor(ms:number){
        this.ms = ms;
        this.time = 0;
    }
    isOk(){
        const now = getTimeStampMs();
        if(now - this.time > this.ms){
            this.time = now;
            return true;
        }
        return false;
    }
}
export function timeReadable(type:'ymd'|'ymdhm'='ymd'){
    const t = new Date()
    const y = t.getFullYear().toString()
    let m = (t.getMonth()+1).toString()
    let d = t.getDate().toString()
    m = padStart(m, 2, '0')
    d = padStart(d, 2, '0')
    if(type==='ymd')
        return `${y}${m}${d}`
    let hour = t.getHours().toString()
    let min = t.getMinutes().toString()
    hour = padStart(hour, 2, '0')
    min = padStart(min, 2, '0')
    return `${y}${m}${d}_${hour}${min}`
}