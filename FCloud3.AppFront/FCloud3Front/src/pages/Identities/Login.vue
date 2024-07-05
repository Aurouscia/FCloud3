<script setup lang="ts">
import { inject, onMounted, onUnmounted, ref,Ref } from 'vue';
import { HttpClient} from '@/utils/com/httpClient';
import { IdentityInfoProvider, useIdentityInfoStore } from '@/utils/globalStores/identityInfo';
import { userTypeText } from '@/models/identities/user';
import Pop from '@/components/Pop.vue'
import { Api } from '@/utils/com/api';
import { injectNotifCountProvider, injectIdentityInfoProvider } from '@/provides';
import { useRouter } from 'vue-router';
import { useIdentityRoutesJump } from './routes/routesJump';
import { recoverTitle, setTitleTo } from '@/utils/titleSetter';
import { storeToRefs } from 'pinia';
import Footer from '@/components/Footer.vue';

const props = defineProps<{
    backAfterSuccess:string
}>();

const userName = ref<string>("")
const password = ref<string>("")
const failedGuide = ref<string>();
var identityInfoProvider:IdentityInfoProvider;
const {iden} = storeToRefs(useIdentityInfoStore())
var httpClient:HttpClient;
var api:Api;
var pop:Ref<InstanceType<typeof Pop>>
const notifProvider = injectNotifCountProvider();
const router = useRouter();
const { jumpToRegister } = useIdentityRoutesJump();

async function Login(){
    const token = await api.identites.auth.login({
        userName:userName.value,
        password:password.value
    })
    if (token) {
        httpClient.setToken(token);
        identityInfoProvider.clearCache();
        await identityInfoProvider.getIdentityInfo(true);
        notifProvider.clear();//立即清除消息个数缓存，重新获取消息个数
        notifProvider.get();
        if (props.backAfterSuccess) {
            router.back()
        } else {
            router.push("/")
        }
    }else{
        const way = await api.etc.utils.applyBeingMember() || "请联系管理员重置"
        failedGuide.value = "如果忘记密码，" + way
    }
};
async function Logout() {
    httpClient.clearToken();
    identityInfoProvider.clearCache();
    pop.value.show("已经成功退出登录","success");
    notifProvider.clear();
}
onMounted(async()=>{
    setTitleTo('登录')
    pop = inject('pop') as Ref<InstanceType<typeof Pop>>
    httpClient = inject('http') as HttpClient;
    api = inject('api') as Api;
    identityInfoProvider = injectIdentityInfoProvider();
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
        <div class="guide" style="color:red" v-if="failedGuide">{{ failedGuide }}</div>
        <div class="guide" style="color:#999" v-else>请使用新版edge或chrome系浏览器以正常使用编辑功能</div>
    </div>
    <div class="loginInfo" v-if="iden">
        当前登录：
        [{{ userTypeText(iden.Type).type }}]{{ iden?.Name }}<br/>
        登录有效期：{{ iden?.LeftHours }}小时<br/>
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
}
.register:hover{
    text-decoration: underline;
    cursor: pointer;
}
</style>