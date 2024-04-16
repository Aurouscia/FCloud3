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