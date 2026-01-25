import { Ref, provide, inject, ref } from 'vue';
import { HttpCallBack, HttpClient } from './utils/com/httpClient';
import { IdentityInfoProvider } from './utils/globalStores/identityInfo';
import Pop from './components/Pop.vue';
import { Api } from './utils/com/api';
import { useIdentityRoutesJump } from '@/pages/Identities/routes/routesJump';
import NeedMemberWarning from './components/NeedMemberWarning.vue';
import Wait from './components/Wait.vue';
import { TimedLock } from './utils/timeStamp';
import { WikiViewScrollMemory } from './utils/wikiView/wikiViewScrollMemory'
import { useNotifCountStore } from './utils/globalStores/notifCount';

const popKey = 'pop';
const httpKey = 'http';
const apiKey = 'api';
const IdentityInfoKey = 'userInfo';
const setTopBarKey = 'setTopbar';
const wikiViewScrollMemoryKey = 'wikiViewScrollMemory'

export type SetTopbarFunc = (display:boolean)=>void

export function useProvidesSetup(
    pop:Ref<InstanceType<typeof Pop>|null>,
    wait:Ref<InstanceType<typeof Wait>|null>,
    needMemberWarning:Ref<InstanceType<typeof NeedMemberWarning>|null>
){
    const { jumpToLogin } = useIdentityRoutesJump();
    provide(popKey, pop)
    const httpCallBack: HttpCallBack = (result, msg) => {
        if (result == 'ok') { pop.value?.show(msg, 'success') }
        else if (result == 'err') { pop.value?.show(msg, 'failed') }
        else if (result == 'warn') { pop.value?.show(msg, 'warning') }
    }

    const showWait = (s:boolean)=>{
        wait.value?.setShowing(s)
    }

    const loginJumpTimeLock:TimedLock = new TimedLock(30000);
    const httpClient = new HttpClient(
        httpCallBack,
        ()=>{
            if(loginJumpTimeLock.isOk()){
                jumpToLogin(true)
            }
        },
        ()=>needMemberWarning.value?.setShow(true),
        showWait)
    provide(httpKey, httpClient)
    const api = new Api(httpClient);
    provide(apiKey, api)

    const idenProvider = new IdentityInfoProvider(api);
    idenProvider.getIdentityInfo();//启动时获取一次身份信息(可能读缓存)
    provide(IdentityInfoKey, idenProvider)

    const displayTopbar = ref<boolean>(true);
    provide(setTopBarKey, (display:boolean) => { 
        displayTopbar.value = display;
    })

    const notifCountStore = useNotifCountStore()
    notifCountStore.setupStore(api, displayTopbar)

    const wikiViewScrollMemory = new WikiViewScrollMemory();
    provide(wikiViewScrollMemoryKey, wikiViewScrollMemory);

    return { pop, displayTopbar, needMemberWarning, wait }
}

export function injectPop(){
    return inject(popKey) as Ref<InstanceType<typeof Pop>>
}
export function injectHttp(){
    return inject(httpKey) as HttpClient;
}
export function injectApi(){
    return inject(apiKey) as Api;
}
export function injectIdentityInfoProvider(){
    return inject(IdentityInfoKey) as IdentityInfoProvider
}
export function injectSetTopbar(){
    return inject(setTopBarKey) as SetTopbarFunc
}
export function injectWikiViewScrollMemory(){
    return inject(wikiViewScrollMemoryKey) as WikiViewScrollMemory
}