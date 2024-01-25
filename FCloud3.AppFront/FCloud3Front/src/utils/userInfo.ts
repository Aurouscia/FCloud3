import { Api } from "./api"

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
    api:Api
    constructor(api:Api){
        this.api = api;
    }
    public async getIdentityInfo():Promise<IdentityInfo> {
        const now:number = new Date().getTime();
        if(now - this.update < 600000){
            return this.cache;
        }
        const res = await this.api.identites.authen.identityTest()
        if (res) {
            const data: IdentityInfo = res;
            this.update = new Date().getTime();
            this.cache = data;
            console.log("获取身份信息为:", data)
            return data;
        }
        else{
            this.update =  new Date().getTime();
            this.cache = defaultValue;
        }
        return this.cache;
    }
    public clearCache(){
        console.log("清空身份信息");
        this.update = 0;
        this.cache = defaultValue;
    }
}