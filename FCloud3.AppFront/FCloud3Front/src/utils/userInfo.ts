import { defineStore, storeToRefs } from "pinia"
import { UserType } from "../models/identities/user"
import { Api } from "./api"
import { ref } from "vue"

export interface IdentityInfo{
    Name:string
    Id:number
    LeftHours:number
    Type: UserType,
    AvtSrc: string
}

const defaultAvatar = "/defaultAvatar.svg"
const defaultValue:IdentityInfo = {
    Name:"游客",
    Id:0,
    LeftHours:0,
    Type: UserType.Tourist,
    AvtSrc: defaultAvatar
}

export class IdentityInfoProvider{
    api:Api
    constructor(api:Api){
        this.api = api;
    }
    public async getIdentityInfo(enforceNew?:boolean):Promise<IdentityInfo> {
        let res:IdentityInfo|undefined = this.readCache()?.info;
        if(res === undefined || enforceNew){
            res = await this.api.identites.authen.identityTest()
            if (res) {
                console.log("获取身份信息为:", res)
            }
            else{
                res = defaultValue;
            }
        }
        this.setCache(res)
        const {iden} = storeToRefs(useIdentityInfoStore())
        iden.value = res;
        return res;
    }

    private localStorageKey = "identityInfo";
    public clearCache(){
        localStorage.removeItem(this.localStorageKey);
    }
    public readCache():{update:number,info:IdentityInfo}|undefined{
        const stored = localStorage.getItem(this.localStorageKey);
        if(stored){
            const data = JSON.parse(stored)
            if(data.update && data.info){
                return data;
            }
        }
    }
    public setCache(info:IdentityInfo){
        const stored = {
            update: new Date().getTime(),
            info: info
        };
        localStorage.setItem(this.localStorageKey, JSON.stringify(stored));
    }
}


export const useIdentityInfoStore = defineStore('iden', {
    state:()=>{
        return {iden: ref(defaultValue)}
    }
})