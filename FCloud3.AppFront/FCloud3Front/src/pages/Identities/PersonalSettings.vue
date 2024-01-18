<script setup lang="ts">
import { inject, onMounted, ref,Ref } from 'vue';
import Pop from '../../components/Pop.vue';
import {User} from '../../models/identities/user';
import { Api } from '../../utils/api';

const user = ref<User>();
var api:Api;
var pop:Ref<InstanceType<typeof Pop>>

async function editUserInfo(){
    if(user.value){
        if(user.value.Name){
            await api.identites.editExe(user.value);
        }
        else{
            pop.value.show("请填写用户名","failed");
        }
    }
}

onMounted(async()=>{
    pop = inject('pop') as Ref<InstanceType<typeof Pop>>;
    api = inject('api') as Api;
    const userInfo = await api.identites.edit()
    if(userInfo){
        user.value = userInfo;
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