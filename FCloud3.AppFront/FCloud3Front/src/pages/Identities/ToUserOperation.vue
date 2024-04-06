<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { Api } from '../../utils/api';
import { UserIndexItem, UserType } from '../../models/identities/user';
import { injectApi, injectUserInfo } from '../../provides';
import { IdentityInfo } from '../../utils/userInfo';

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

let api:Api;
let iden:IdentityInfo
const ready = ref<boolean>(false);
onMounted(async()=>{
    api = injectApi();
    iden = await injectUserInfo().getIdentityInfo()
    ready.value = true;
})

const emits = defineEmits<{
    (e:'typeChanged'):void
}>();
</script>

<template>
    <div v-if="ready">
        <div v-if="iden.Type>=UserType.Admin" style="text-align: center;">
            <select v-model="selectedType">
                <option :value="UserType.Tourist">游客</option>
                <option :value="UserType.Member">会员</option>
                <option :value="UserType.Admin">管理</option>
                <option :value="UserType.SuperAdmin">超管</option>
            </select>
            <button @click="setType">确定</button>
        </div>
    </div>
</template>

<style scoped>
select{
    width: 150px;
}
</style>