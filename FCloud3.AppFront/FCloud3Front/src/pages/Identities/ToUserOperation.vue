<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { Api } from '@/utils/com/api';
import { UserIndexItem, UserType } from '@/models/identities/user';
import { injectApi } from '@/provides';
import { useIdentityInfoStore } from '@/utils/globalStores/identityInfo';
import { storeToRefs } from 'pinia';
import { useIdentityRoutesJump } from './routes/routesJump';

const props = defineProps<{
    user:UserIndexItem
}>();

const selectedType = ref<UserType>(props.user.TypeEnum);
async function setType(){
    const resp = await api.identites.user.setUserType(props.user.Id, selectedType.value);
    if(resp){
        emits('typeChanged')
    }
}

const resetPwdTo = ref("")
async function resetPwd() {
    await api.identites.user.resetPwd(props.user.Id, resetPwdTo.value);
}

let api:Api;
const {iden} = storeToRefs(useIdentityInfoStore())
const ready = ref<boolean>(false);
const { jumpToUserCenter } = useIdentityRoutesJump();
onMounted(async()=>{
    api = injectApi();
    ready.value = true;
})

const emits = defineEmits<{
    (e:'typeChanged'):void
}>();
</script>

<template>
    <div v-if="ready" class="outer">
        <div>
            <img class="bigAvatar" :src="user?.Avatar"/>
            <div class="username">{{ user.Name }}</div>
            <button @click="jumpToUserCenter(user.Name)">查看个人页面</button>
        </div>
        <div v-if="iden.Type>=UserType.Admin" style="text-align: center;">
            <select v-model="selectedType">
                <option :value="UserType.Tourist">游客</option>
                <option :value="UserType.Member">会员</option>
                <option :value="UserType.Admin">管理</option>
                <option :value="UserType.SuperAdmin">超管</option>
            </select>
            <button @click="setType">确定</button>
        </div>
        <div v-if="iden.Type==UserType.SuperAdmin">
            <input v-model="resetPwdTo" placeholder="新密码"/>
            <button @click="resetPwd">重置密码</button>
        </div>
        <div>
            私聊功能暂未开发，若有需要请在其任意作品评论区留下qq号等联系方式
        </div>
    </div>
</template>

<style scoped>
.username{
    font-size: 20px;
    margin: 8px;
}
.outer{
    display: flex;
    flex-direction: column;
    gap:30px;
}
.outer div{
    text-align: center;
}
select{
    width: 150px;
}
</style>