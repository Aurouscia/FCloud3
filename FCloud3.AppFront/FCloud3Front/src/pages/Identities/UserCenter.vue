<script setup lang="ts">
import { inject, onMounted, ref } from 'vue';
import SideBar from '../../components/SideBar.vue';
import Personal from './PersonalSettings.vue';
import Loading from '../../components/Loading.vue';
import IndexWikiItem from '../../components/Index/IndexWikiItem.vue'
import { User } from '../../models/identities/user';
import { Api } from '../../utils/api';
import SwitchingTabs from '../../components/SwitchingTabs.vue';
import { IdentityInfo, IdentityInfoProvider } from '../../utils/userInfo';

const props = defineProps<{
    username?:string
}>();
const user = ref<User>();
var api:Api;
var iden:IdentityInfo
const editInfoSidebar = ref<InstanceType<typeof SideBar>>();
const ok = ref<boolean>(false);
onMounted(async()=>{
    api = inject('api') as Api;
    var username = props.username;
    if(!username){
        iden = await (inject('userInfo') as IdentityInfoProvider).getIdentityInfo();
        username = iden.Name
    }
    if(iden.Id==0){
        return;
    }
    user.value = await api.identites.getInfoByName(username);
    if(user.value){
        ok.value = true;
    }
})
</script>

<template>
    <div v-if="ok" class="user">
        <div class="info">
            <img src="/vite.svg"/>
            <div class="username">{{ props.username }}</div>
            <div class="motto">暂无简介</div>
            <div class="settings"><button @click="editInfoSidebar?.extend">编辑信息</button></div>
        </div>
        <SwitchingTabs style="width: 300px;height: 400px;" :texts="['用户动态','自荐作品','最新作品']">
            <IndexWikiItem></IndexWikiItem>
            <div>自荐作品</div>
            <div>最新作品</div>
        </SwitchingTabs>
    </div>
    <div v-else><Loading></Loading></div>
    <SideBar ref="editInfoSidebar">
        <personal></personal>
    </SideBar>
</template>

<style scoped>
.info .settings{
    margin-top: 20px;
}
.info .motto{
    color:#aaa;
    max-width: 300px;
    text-align: center;
    word-break: break-all;
}
.info .username{
    font-size: 30px;
    margin-top: 20px;
    color:#444
}
.info{
    display: flex;
    flex-direction: column;
    align-items: center;
    margin: 20px;
}
.info img{
    width: 150px;
    height: 150px;
    border:2px solid #eee;
    border-radius: 1000px;
}

.user{
    display: flex;
    flex-direction: row;
    flex-wrap: wrap;
    justify-content: space-between;
    align-items: start;
}
.user>*{
    flex-grow: 1;
}
</style>