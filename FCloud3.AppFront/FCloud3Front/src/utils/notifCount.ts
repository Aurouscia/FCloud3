import { Ref, ref } from "vue";
import { Api } from "./api";
import { getTimeStamp } from "./timeStamp";
import _ from 'lodash'
import { defineStore } from "pinia";
import { useIdentityInfoStore } from "./userInfo";

const key = "notifInfo"
interface NotifInfo{
    count:number
    update:number
}
const notifCountPollIntervalSec = 180;
const notifCountCacheExpireSec = 170;

export class NotifCountProvider{
    private api:Api
    private showTopbar:Ref<boolean>
    constructor(api:Api, showTopbar:Ref<boolean>){
        this.api = api;
        this.showTopbar = showTopbar;
    }
    async get(){
        const notifStore = useNotifCountStore();
        const idenStore = useIdentityInfoStore();
        if(this.showTopbar.value === false || idenStore.iden.Id == 0){
            notifStore.notifCount = 0;//赋值pinia数据
            return 0;//顶部栏不在或没有登录的时候就没有必要获取了
        }
        let stored = localStorage.getItem(key)
        let obj:NotifInfo|undefined = undefined;
        let needRequest:boolean = true;
        if(stored){
            const storedObj = JSON.parse(stored)
            if(_.has(storedObj, 'count') && _.has(storedObj, 'update')){
                obj = storedObj as NotifInfo;
                if(getTimeStamp() - obj.update < notifCountCacheExpireSec){
                    needRequest = false;
                }
            }
        }
        if(needRequest || !obj){
            const count = await this.api.messages.notification.count();
            console.log("读取服务器响应的未读消息个数:", count)
            const update = getTimeStamp();
            const newObj:NotifInfo = {
                count, update
            }
            localStorage.setItem(key, JSON.stringify(newObj))
            notifStore.notifCount = count;//赋值pinia数据
            return count;
        }else{
            console.log("读取缓存中的未读消息个数:", obj.count)
            notifStore.notifCount = obj.count;//赋值pinia数据
            return obj.count;
        }
    }
    clear(){
        const notifStore = useNotifCountStore();
        notifStore.notifCount = 0;
        localStorage.removeItem(key)
        console.log("清除未读消息个数缓存");
    }
}


export function setupPollCycle(provider:NotifCountProvider){
    setTimeout(()=>provider.get(), 1000) //初始化时立即查询一次
    setInterval(()=>provider.get(), notifCountPollIntervalSec*1000) //每隔x秒查询一次
}


export const useNotifCountStore = defineStore('notifCount', {
    state:()=>{
        return {notifCount: ref(0)}
    },
    actions:{
        readOne(){
            if(this.notifCount > 0)
                this.notifCount--;
        },
        readAll(){
            this.notifCount = 0;
        }
    }
})