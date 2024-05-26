import { Api } from "../../utils/api";

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
            this.api.utils.heartbeat({objType:type, objId:id});
        }, 1000 * 40);
    }
    stop(){
        clearInterval(this.timer);
    }
}