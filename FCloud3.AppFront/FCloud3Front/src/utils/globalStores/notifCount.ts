import { Ref, ref } from "vue";
import { Api } from "../com/api";
import { getTimeStamp } from "../timeStamp";
import { has } from 'lodash-es'
import { defineStore } from "pinia";
import { useIdentityInfoStore } from "./identityInfo";

const key = "notifInfo"
interface NotifInfo{
    count:number
    update:number
}
const notifCountPollIntervalSec = 10;
const notifCountCacheExpireSec = 60;

//获取未读消息数量的途径（有缓存）会自动更新piniaStore
export const useNotifCountStore = defineStore('notifCount', ()=>{
    let api:Api|undefined
    let showTopbar:Ref<boolean>|undefined
    const unreadNotifCount = ref(0)
    const idenStore = useIdentityInfoStore();
    function setupStore(apiInstance:Api, showTopbarStatus:Ref<boolean>){
        api = apiInstance;
        showTopbar = showTopbarStatus;
        setTimeout(() => refresh(), 1000) //初始化时立即查询一次
        setInterval(() => refresh(), notifCountPollIntervalSec*1000) //每隔x秒查询一次
    }

    async function refresh(enforce?:boolean):Promise<void>{
        if(!showTopbar?.value || idenStore.iden.Id == 0 || !api){
            unreadNotifCount.value = 0;//顶部栏不在或没有登录的时候就没有必要获取了
            clearCache()
            return
        }
        let obj:NotifInfo|undefined = undefined;
        let needRequest:boolean = true;
        if(!enforce){
            let cache = localStorage.getItem(key)
            if(cache){
                const cachedObj = JSON.parse(cache)
                if(has(cachedObj, 'count') && has(cachedObj, 'update')){
                    obj = cachedObj as NotifInfo;
                    if(getTimeStamp() - obj.update < notifCountCacheExpireSec){
                        needRequest = false;
                    }
                }
            }
        }
        if(needRequest || !obj){
            const count = await api.messages.notification.count();
            console.log("读取服务器响应的未读消息个数:", count)
            const update = getTimeStamp();
            const newObj:NotifInfo = {
                count, update
            }
            localStorage.setItem(key, JSON.stringify(newObj))
            unreadNotifCount.value = count;
        }else{
            console.log("读取缓存中的未读消息个数:", obj.count)
            unreadNotifCount.value = obj.count;
        }
    }
    function setCount(count:number){
        unreadNotifCount.value = count;
        const update = getTimeStamp();
        const newObj:NotifInfo = {
            count, update
        }
        localStorage.setItem(key, JSON.stringify(newObj))
    }
    function readOne(){
        if(unreadNotifCount.value > 0)
            unreadNotifCount.value -= 1;
        clearCache()
    }
    function readAll(){
        unreadNotifCount.value = 0;
        clearCache()
    }
    function enforceRefresh(){
        refresh(true)
    }
    function clearCache(){
        localStorage.removeItem(key);
    }
    return { setupStore, readAll, readOne, enforceRefresh, setCount, unreadNotifCount }
})