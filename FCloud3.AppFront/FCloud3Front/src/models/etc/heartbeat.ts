import { Api } from "@/utils/com/api";

const heartbeatIntervalSec = 40;

export enum HeartbeatObjType{
    None = 0,
    TextSection = 1,
    FreeTable = 2
}

export interface HeartbeatRequest{
    objType: HeartbeatObjType,
    objId: number
}

export class HeartbeatSender{
    api: Api;
    timer: number = 0;
    id: number;
    type: HeartbeatObjType;
    constructor(api:Api, type:HeartbeatObjType, id:number){
        this.api = api;
        this.id = id;
        this.type = type;
    }
    start(){
        const type = this.type;
        const id = this.id;
        this.timer = window.setInterval(() => {
            this.api.etc.heartbeat.do({objType:type, objId:id});
        }, 1000 * heartbeatIntervalSec);
    }
    stop(){
        clearInterval(this.timer);
    }
}

export class HeartbeatSenderForWholeWiki{
    api: Api;
    timer: number = 0;
    wikiId: number;
    constructor(api:Api, wikiId:number){
        this.api = api;
        this.wikiId = wikiId;
    }
    start(){
        const wikiId = this.wikiId;
        this.timer = window.setInterval(() => {
            this.api.etc.heartbeat.doRangeForWiki(wikiId);
        }, 1000 * heartbeatIntervalSec);
    }
    stop(){
        clearInterval(this.timer);
    }
}