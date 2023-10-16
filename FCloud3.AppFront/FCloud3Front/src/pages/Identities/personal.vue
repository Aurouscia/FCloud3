<script setup lang="ts">
import { inject, onMounted, ref,Ref } from 'vue';
import { HttpClient,ApiResponse } from '../../utils/httpClient';
import { config } from '../../consts';
import Pop from '../../components/Pop.vue';
import {User} from '../../models/identities/user';

const user = ref<User>();
var httpClient:HttpClient;
var pop:Ref<InstanceType<typeof Pop>>

async function editUserInfo(){
    await httpClient.send(config.api.identities.editExe,user.value,
        pop.value.show,"修改成功");
}

onMounted(async()=>{
    pop = inject('pop') as Ref<InstanceType<typeof Pop>>;
    httpClient = inject('http') as HttpClient;
    const resp:ApiResponse = await httpClient.send(config.api.identities.edit,undefined,pop.value.show)
    if(resp.success){
        user.value = resp.data;
    }
})
</script>

<template>
    <div>
        <h1>个人中心</h1>
    </div>
    <div class="section" v-if="user">
        <h2>编辑个人信息</h2>
        <table>
            <tr>
                <td>昵称</td>
                <td>
                    <input v-model="user.Name"/>
                </td>
            </tr>
            <tr>
                <td>密码</td>
                <td>
                    <input v-model="user.Pwd"/>
                </td>
            </tr>
            <tr class="noneBackground">
                <td></td>
                <td>
                    <button @click="editUserInfo">保存</button>
                </td>
            </tr>
        </table>
        
    </div>
</template>

<style scoped>

</style>