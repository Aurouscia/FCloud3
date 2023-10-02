import { config } from "../consts"
import { HttpClient } from "./httpClient"

export interface IdentityInfo{
    Name:string
    Id:number
    LeftHours:number
}

const defaultValue = {
    Name:"游客",
    Id:0,
    LeftHours:0
}

export class IdentityInfoProvider{
    cache:IdentityInfo = defaultValue;
    update:number = 0;
    http:HttpClient;
    constructor(http:HttpClient){
        this.http = http;
    }
    public async getIdentityInfo():Promise<IdentityInfo> {
        const now:number = new Date().getTime();
        if(now - this.update < 600000){
            return this.cache;
        }
        const res = await this.http.send(config.api.identities.identityTest)
        if (res.success) {
            try{
                const data:IdentityInfo = res.data as IdentityInfo;
                this.update=new Date().getTime();
                this.cache = data;
                console.log("获取身份信息为:",data)
                return data;
            }
            catch{}
        }
        this.update =  new Date().getTime();
        this.cache = defaultValue;
        return this.cache;
    }
    public clearCache(){
        console.log("清空身份信息");
        this.update = 0;
        this.cache = defaultValue;
    }
}