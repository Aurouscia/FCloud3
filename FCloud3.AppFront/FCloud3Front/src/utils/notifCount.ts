import { Ref } from "vue";
import { Api } from "./api";
import { getTimeStamp } from "./timeStamp";
import _ from 'lodash'

const key = "notifInfo"
interface NotifInfo{
    count:number
    update:number
}
export class NotifCount{
    private api:Api
    private showTopbar:Ref<boolean>
    constructor(api:Api, showTopbar:Ref<boolean>){
        this.api = api;
        this.showTopbar = showTopbar;
    }
    async get(){
        if(this.showTopbar.value === false)
            return 0;//顶部栏不在的时候就没有必要获取了
        let stored = localStorage.getItem(key)
        let obj:NotifInfo|undefined = undefined;
        let needRequest:boolean = true;
        if(stored){
            const storedObj = JSON.parse(stored)
            if(_.has(storedObj, 'count') && _.has(storedObj, 'update')){
                obj = storedObj as NotifInfo;
                if(getTimeStamp() - obj.update < 50){
                    needRequest = false;
                }
            }
        }
        if(needRequest || !obj){
            const count = await this.api.messages.notification.count();
            const update = getTimeStamp();
            const newObj:NotifInfo = {
                count, update
            }
            localStorage.setItem(key, JSON.stringify(newObj))
            return count;
        }else{
            console.log("读取缓存中的未读消息个数")
            return obj.count;
        }
    }
    clear(){
        localStorage.removeItem(key)
    }
}