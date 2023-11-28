<script setup lang="ts">
import { inject, onMounted, ref,Ref } from 'vue';
import { HttpClient} from '../../utils/httpClient';
import { IdentityInfo,IdentityInfoProvider } from '../../utils/userInfo';
import Pop from '../../components/Pop.vue';
import { Api } from '../../utils/api';

const userName = ref<string>("")
const password = ref<string>("")
var identityInfoProvider:IdentityInfoProvider;
const identityInfo = ref<IdentityInfo|undefined>()
var httpClient:HttpClient;
var api:Api;
var pop:Ref<InstanceType<typeof Pop>>
async function Login(){
    const token = await api.identites.login({
        userName:userName.value,
        password:password.value
    },pop.value.show)
    if(token){
        httpClient.setToken(token);
        identityInfoProvider.clearCache();
        if (identityInfoProvider) {
            identityInfo.value = await identityInfoProvider.getIdentityInfo();
        }
    }
};
async function Logout() {
    httpClient.clearToken();
    identityInfoProvider.clearCache();
    pop.value.show("已经成功退出登录","success");
    if(identityInfoProvider){
        identityInfo.value = await identityInfoProvider.getIdentityInfo();
    }
}
onMounted(async()=>{
    pop = inject('pop') as Ref<InstanceType<typeof Pop>>
    httpClient = inject('http') as HttpClient;
    api = inject('api') as Api;
    identityInfoProvider = inject('userInfo') as IdentityInfoProvider
    identityInfo.value = await identityInfoProvider.getIdentityInfo();
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
        <div>
            <button @click="Login" class="login">登&nbsp;录</button>
        </div>
    </div>
    <div class="loginInfo" v-if="identityInfo">
        当前登录：
        [{{ identityInfo?.Id }}]{{ identityInfo?.Name }}<br/>
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
button.login{
    background-color: cornflowerblue;
    margin: auto;
    font-size: large;
    color:white;
    padding: 5px;
    padding-bottom: 6px;
    display: flex;
    align-items: center;
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
}
</style>../../consts