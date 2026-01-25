import { getTimeStamp } from '@/utils/timeStamp';

const wikiViewScrollMemoryExpireSecs = 60*10
interface MemoryItem{
    top:number,
    time:number
}
export class WikiViewScrollMemory{
    private memory:Record<string, MemoryItem>;
    constructor(){
        this.memory = {}
    }
    read(wikiPathName:string, wikiView?:HTMLElement|null){
        const now = getTimeStamp();
        if(!wikiView){
            return;
        }
        const m = this.memory[wikiPathName]
        if(m){
            if(now - m.time < wikiViewScrollMemoryExpireSecs)
                wikiView.scrollTop = m.top;
        }
    }
    save(wikiPathName:string, wikiView?:HTMLElement|null){
        if(!wikiView){
            return;
        }
        this.memory[wikiPathName] = {
            top: wikiView.scrollTop,
            time: getTimeStamp()
        }
    }
}