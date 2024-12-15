<script setup lang="ts">
import { computed, inject, onMounted, onUnmounted, ref,Ref } from 'vue';
import { HttpClient} from '@/utils/com/httpClient';
import { IdentityInfoProvider, useIdentityInfoStore } from '@/utils/globalStores/identityInfo';
import { userTypeText } from '@/models/identities/user';
import Pop from '@/components/Pop.vue'
import { Api } from '@/utils/com/api';
import { injectIdentityInfoProvider } from '@/provides';
import { useRouter } from 'vue-router';
import { useIdentityRoutesJump } from './routes/routesJump';
import { recoverTitle, setTitleTo } from '@/utils/titleSetter';
import { storeToRefs } from 'pinia';
import Footer from '@/components/Footer.vue';
import { saveLocalConfig, readLocalConfig } from '@/utils/localConfig/localConfig';
import { useNotifCountStore } from '@/utils/globalStores/notifCount';
import { authConfigDefault, AuthLocalConfig } from '@/utils/localConfig/models/auth';
import { guideInfo } from '@/utils/guideInfo';

const props = defineProps<{
    backAfterSuccess:string
}>();

const userName = ref<string>("")
const password = ref<string>("")
const needExpire = ref<number>(72);
const setExpire = ref<boolean>(false);
const failedGuide = ref<string>();
var identityInfoProvider:IdentityInfoProvider;
const {iden} = storeToRefs(useIdentityInfoStore())
var httpClient:HttpClient;
var api:Api;
var pop:Ref<InstanceType<typeof Pop>>
const router = useRouter();
const { jumpToRegister } = useIdentityRoutesJump();
const notifCountStore = useNotifCountStore()

async function Login(){
    authLocalConfig.expireHours = needExpire.value;
    saveLocalConfig(authLocalConfig);
    const token = await api.identites.auth.login({
        userName:userName.value,
        password:password.value,
        expHours:needExpire.value
    })
    if (token) {
        httpClient.setToken(token);
        identityInfoProvider.clearCache();
        await identityInfoProvider.getIdentityInfo(true);
        notifCountStore.enforceRefresh();//立即重新获取消息个数
        if (props.backAfterSuccess) {
            router.back()
        } else {
            router.push("/")
        }
    }else{
        const way = guideInfo.resetPassword || "请联系管理员重置"
        failedGuide.value = "如果忘记密码，" + way
    }
};
async function Logout() {
    httpClient.clearToken();
    identityInfoProvider.clearCache();
    pop.value.show("已经成功退出登录","success");
    notifCountStore.enforceRefresh()
}

const leftTimeDisplay = computed<string>(()=>{
    if(!iden){
        return '0小时';
    }
    const hours = iden.value.LeftHours;
    if(hours>72){
        return Math.round(hours/24)+'天';
    }
    return hours+'小时';
})

let authLocalConfig:AuthLocalConfig = authConfigDefault();
onMounted(async()=>{
    setTitleTo('登录')
    pop = inject('pop') as Ref<InstanceType<typeof Pop>>
    httpClient = inject('http') as HttpClient;
    api = inject('api') as Api;
    identityInfoProvider = injectIdentityInfoProvider();
    authLocalConfig = (readLocalConfig('auth') || authConfigDefault()) as AuthLocalConfig;
    needExpire.value = authLocalConfig.expireHours || 72;
    await identityInfoProvider.getIdentityInfo(true);
})
onUnmounted(()=>{
    recoverTitle()
})
</script>

<template>
    <div>
        <h1>登录</h1>
    </div>
    <div>
        <table><tbody>
            <tr>
                <td>昵称</td>
                <td>
                    <input v-model="userName" type="text"/>
                </td>
            </tr>
            <tr>
                <td>密码</td>
                <td>
                    <input v-model="password" type="password"/>
                </td>
            </tr>
        </tbody></table>
        <div class="login">
            <button @click="Login" class="confirm">登&nbsp;录</button>
        </div>
        <div class="needExpire">
            <div @click="setExpire=!setExpire" style="cursor: pointer;">登录状态保持</div>
            <select v-show="setExpire" v-model="needExpire">
                <option :value="3">3小时</option>
                <option :value="24">24小时</option>
                <option :value="72">3天</option>
                <option :value="720">30天</option>
                <option :value="8760">365天</option>
            </select>
            <div v-show="setExpire" style="color:red">仅在自己的设备上选择较长时间</div>
        </div>
        <div class="register" @click="jumpToRegister">
            注册账号
        </div>
        <div class="guide" style="color:red" v-if="failedGuide">{{ failedGuide }}</div>
        <div class="guide" style="color:#aaa" v-else>请在较新设备上使用新版edge或chrome系浏览器以正常使用编辑功能</div>
    </div>
    <div class="loginInfo" v-if="iden">
        当前登录：
        [{{ userTypeText(iden.Type).type }}]{{ iden?.Name }}<br/>
        登录有效期：{{ leftTimeDisplay }}<br/>
        <button @click="Logout" class="logout">退出登录</button>
    </div>
    <div class="footer">
        <Footer></Footer>
    </div>
</template>

<style scoped>
.guide{
    margin: 10px;
    text-align: center;
    border-radius: 5px;
}
table{
    margin:auto;
    background-color: transparent;
    font-size: large;
    color:gray
}
td{
    background-color: transparent;
}
input{
    background-color: #eee;
}
.needExpire{
    text-align: center;
    height: 60px;
    margin-top: 20px;
    font-size: small;
    color: #999;
    display: flex;
    justify-content: center;
    align-items: center;
    white-space: nowrap;
    flex-wrap: wrap;
}
.needExpire select{
    padding: 2px;
    margin: 2px;
    margin-left: 10px;
}
.login{
    display: flex;
    justify-content: center;
}
button.logout{
    background-color: gray;
    color:white;
    padding: 2px;
}
.footer{
    position: fixed;
    bottom: 0px;
    left: 0px;
    right: 0px;
}
.loginInfo{
    color:gray;
    font-size:small;
    text-align: center;
    position: fixed;
    margin: 0px;
    left:20px;
    bottom: 35px;
}
.register{
    text-align: center;
    color:gray;
    margin-top: 20px;
    font-size: 16px;
    text-decoration: underline;
    cursor: pointer;
}
</style>