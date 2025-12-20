import { defineStore } from "pinia"
import { UserType } from "@/models/identities/user"
import { Api } from "../com/api"
import { computed, ref } from "vue"
import { getTimeStamp } from "../timeStamp"
import { userDefaultAvatar } from "@/models/files/material"

export interface IdentityInfo{
    Name:string
    Id:number
    LeftHours:number
    LeftSeconds:number
    Type: UserType,
    AvtSrc: string
}

const defaultValue:IdentityInfo = {
    Name:"游客",
    Id:0,
    LeftHours:0,
    LeftSeconds:0,
    Type: UserType.Tourist,
    AvtSrc: userDefaultAvatar
}
const identityCacheExpireSec = 60*60
export { defaultValue as defaultIdentity }

//获取身份信息的途径（有缓存）获取时会自动更新piniaStore
export class IdentityInfoProvider{
    api:Api
    constructor(api:Api){
        this.api = api;
    }
    public async getIdentityInfo(enforceNew?:boolean):Promise<IdentityInfo> {
        let res:IdentityInfo|undefined = this.readCache()?.info;
        if(res === undefined || enforceNew){
            res = await this.api.identites.auth.identityTest()
            if (res) {
                console.log("获取服务器响应的身份信息:", res)
            }
            else{
                res = defaultValue;
            }
            this.setCache(res)
        }
        else{
            console.log("获取缓存中的身份信息:",res)
        }
        useIdentityInfoStore().iden = res//更新pinia中的数据
        return res;
    }

    private localStorageKey = "identityInfo";
    public clearCache(){
        localStorage.removeItem(this.localStorageKey);
        useIdentityInfoStore().iden = defaultValue;//更新pinia中的数据
        console.log("清除缓存中的身份信息")
    }
    public readCache():{update:number,info:IdentityInfo}|undefined{
        const stored = localStorage.getItem(this.localStorageKey);
        if(stored){
            const data = JSON.parse(stored)
            if(data.update && data.info){
                if(getTimeStamp() - identityCacheExpireSec < (data.update as number)) //缓存未过期
                    return data;
            }
        }
    }
    public setCache(info:IdentityInfo){
        const stored = {
            update: getTimeStamp(),
            info: info
        };
        localStorage.setItem(this.localStorageKey, JSON.stringify(stored));
    }
}

//存储身份信息的piniaStore，获取时应该通过这里获取
export const useIdentityInfoStore = defineStore('iden', ()=>{
    const iden = ref(defaultValue)
    const isAdmin = computed<boolean>(()=>iden.value.Type>=UserType.Admin)
    const isSuperAdmin = computed<boolean>(()=>iden.value.Type>=UserType.SuperAdmin)
    const isLoginValid = computed<boolean>(()=>iden.value.Id>0&&iden.value.LeftSeconds>0)
    return { iden, isAdmin, isSuperAdmin, isLoginValid }
})