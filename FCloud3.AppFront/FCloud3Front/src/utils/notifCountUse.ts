import { Ref, ref } from "vue";
import { injectNotifCount } from "../provides";

const notifCounts:Array<Ref<number>> = [];
let intervalSet = false;
export function useNotifCount(){
    const notifCount = ref(0);
    notifCounts.push(notifCount);
    const notifCountSource = injectNotifCount();
    const get = ()=>{
        notifCountSource.get().then(x => notifCounts.forEach(n => n.value=x));
    }
    const readOne = ()=>{notifCounts.forEach(n => n.value -= 1)}
    const readAll = ()=>{notifCounts.forEach(n => n.value = 0); clear()}
    const clear = () => {notifCountSource.clear(); get()};
    if(!intervalSet){
        get();
        setInterval(get, 60000);
        intervalSet = true;
    }
    return {notifCount, readOne, readAll, clear}
}