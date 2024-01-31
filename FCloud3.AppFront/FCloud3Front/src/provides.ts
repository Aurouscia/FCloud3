import { Ref, provide, inject, ref} from 'vue';
import { HttpCallBack, HttpClient } from './utils/httpClient';
import { IdentityInfoProvider } from './utils/userInfo';
import Pop from './components/Pop.vue';
import { router } from './main';
import { Api } from './utils/api';

const popKey = 'pop';
const httpKey = 'http';
const apiKey = 'api';
const userInfoKey = 'userInfo';
const hideTopbarKey = 'hideTopbar';

export function useProvidesSetup() {
    const pop = ref<InstanceType<typeof Pop> | null>(null);
    provide(popKey, pop)
    const httpCallBack: HttpCallBack = (result, msg) => {
        if (result == 'ok') { pop.value?.show(msg, 'success') }
        else if (result == 'err') { pop.value?.show(msg, 'failed') }
        else if (result == 'warn') { pop.value?.show(msg, 'warning') }
    }

    const httpClient = new HttpClient(httpCallBack)
    provide(httpKey, httpClient)
    const api = new Api(httpClient);
    provide(apiKey, api)
    provide(userInfoKey, new IdentityInfoProvider(api))

    const displayTopbar = ref<boolean>(true);
    provide(hideTopbarKey, () => { displayTopbar.value = false })

    router.afterEach(() => {
        displayTopbar.value = true;
    })
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
export function injectHideTopbar(){
    return inject(hideTopbarKey) as ()=>void
}