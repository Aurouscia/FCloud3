<script setup lang="ts">
import { inject, onMounted, ref,Ref } from 'vue';
import { HttpClient} from '../../utils/httpClient';
import { IdentityInfo,IdentityInfoProvider } from '../../utils/userInfo';
import { userTypeText } from '../../models/identities/user';
import Pop from '../../components/Pop.vue'
import { Api } from '../../utils/api';
import { injectUserInfo } from '../../provides';
import { useRouter } from 'vue-router';
import { jumpToRegister } from './routes';
import { useNotifCount } from '../../utils/notifCountUse';

const props = defineProps<{
    backAfterSuccess:string
}>();

const userName = ref<string>("")
const password = ref<string>("")
var identityInfoProvider:IdentityInfoProvider;
const identityInfo = ref<IdentityInfo|undefined>()
var httpClient:HttpClient;
var api:Api;
var pop:Ref<InstanceType<typeof Pop>>
const notif = useNotifCount();
const router = useRouter();
async function Login(){
    const token = await api.identites.authen.login({
        userName:userName.value,
        password:password.value
    })
    if(token){
        httpClient.setToken(token);
        identityInfoProvider.clearCache();
        if (identityInfoProvider) {
            identityInfo.value = await identityInfoProvider.getIdentityInfo(true);
            notif.clear();
            if(props.backAfterSuccess){
                router.back()
            }else{
                router.push("/")
            }
        }
    }
};
async function Logout() {
    httpClient.clearToken();
    identityInfoProvider.clearCache();
    pop.value.show("已经成功退出登录","success");
    if(identityInfoProvider){
        identityInfo.value = await identityInfoProvider.getIdentityInfo(true);
    }
}
onMounted(async()=>{
    pop = inject('pop') as Ref<InstanceType<typeof Pop>>
    httpClient = inject('http') as HttpClient;
    api = inject('api') as Api;
    identityInfoProvider = injectUserInfo();
    identityInfo.value = await identityInfoProvider.getIdentityInfo(true);
})
</script>

<template>
    <div>
        <h1>登录</h1>
    </div>
    <div>
        <table>
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
        </table>
        <div class="login">
            <button @click="Login" class="confirm">登&nbsp;录</button>
        </div>
        <div class="register" @click="jumpToRegister">
            注册账号
        </div>
    </div>
    <div class="loginInfo" v-if="identityInfo">
        当前登录：
        [{{ userTypeText(identityInfo.Type).type }}]{{ identityInfo?.Name }}<br/>
        登录有效期：{{ identityInfo?.LeftHours }}小时<br/>
        <button @click="Logout" class="logout">退出登录</button>
    </div>
</template>

<style scoped>
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
.login{
    display: flex;
    justify-content: center;
}
button.logout{
    background-color: gray;
    color:white;
    padding: 2px;
}
.loginInfo{
    color:gray;
    font-size:small;
    text-align: center;
    position: fixed;
    margin: 0px;
    left:20px;
    bottom: 20px;
}
.register{
    text-align: center;
    color:gray;
    margin-top: 20px;
    font-size: 16px;
}
.register:hover{
    text-decoration: underline;
    cursor: pointer;
}
</style>