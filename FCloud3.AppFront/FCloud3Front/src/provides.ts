import { Ref, provide, inject, ref} from 'vue';
import { HttpCallBack, HttpClient } from './utils/httpClient';
import { IdentityInfoProvider } from './utils/userInfo';
import Pop from './components/Pop.vue';
import { Api } from './utils/api';
import { jumpToLogin } from './pages/Identities/routes';

const popKey = 'pop';
const httpKey = 'http';
const apiKey = 'api';
const userInfoKey = 'userInfo';
const setTopBarKey = 'setTopbar';

export type SetTopbarFunc = (display:boolean)=>void

export function useProvidesSetup() {
    const pop = ref<InstanceType<typeof Pop> | null>(null);
    provide(popKey, pop)
    const httpCallBack: HttpCallBack = (result, msg) => {
        if (result == 'ok') { pop.value?.show(msg, 'success') }
        else if (result == 'err') { pop.value?.show(msg, 'failed') }
        else if (result == 'warn') { pop.value?.show(msg, 'warning') }
    }

    const httpClient = new HttpClient(httpCallBack, jumpToLogin)
    provide(httpKey, httpClient)
    const api = new Api(httpClient);
    provide(apiKey, api)
    provide(userInfoKey, new IdentityInfoProvider(api))

    const displayTopbar = ref<boolean>(true);
    provide(setTopBarKey, (display:boolean) => { displayTopbar.value = display })
    return { pop, displayTopbar }
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
export function injectUserInfo(){
    return inject(userInfoKey) as IdentityInfoProvider
}
export function injectSetTopbar(){
    return inject(setTopBarKey) as SetTopbarFunc
}