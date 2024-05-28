<script setup lang="ts">
import { Ref, onMounted, onUnmounted, ref, watch } from 'vue';
import SideBar from '@/components/SideBar.vue';
import Personal from './PersonalSettings.vue';
import Loading from '@/components/Loading.vue';
import LatestWork from '@/components/LatestWork.vue';
import OpRecord from '@/components/Messages/OpRecord.vue';
import { User } from '@/models/identities/user';
import { Api } from '@/utils/com/api';
import SwitchingTabs from '@/components/SwitchingTabs.vue';
import { useIdentityInfoStore } from '@/utils/globalStores/identityInfo';
import { injectApi, injectPop, injectIdentityInfoProvider } from '@/provides';
import { useIdentityRoutesJump } from '@/pages/Identities/routes/routesJump';
import Pop from '@/components/Pop.vue';
import { recoverTitle, setTitleTo } from '@/utils/titleSetter';
import { storeToRefs } from 'pinia';

const props = defineProps<{
    username?:string
}>();
const user = ref<User>();
var api:Api;
const editInfoSidebar = ref<InstanceType<typeof SideBar>>();
const ok = ref<boolean>(false);
const { jumpToLogin } = useIdentityRoutesJump();

watch(props, _newVal=>{
    location.reload();
})

async function load(clearCache?:boolean){
    let nameChanged = false;
    //user.value可能被个人设置组件所更改，这里需要检查是否有改动，有改动就跳转去登录页面
    if(user.value){
        if(username != user.value.Name){
            nameChanged = true;
        }
        username = user.value.Name;
    }
    if(clearCache){
        idenProvider.clearCache();
        idenProvider.getIdentityInfo();
    }
    if(username){
        user.value = await api.identites.user.getInfoByName(username);
        if(user.value){
            ok.value = true;
        }
        if(nameChanged){
            pop.value.show("请立即使用新用户名登录",'warning')
            jumpToLogin();
        }
    }
}

let username:string|undefined;
const idenStore = useIdentityInfoStore()
const {iden} = storeToRefs(idenStore)
let pop: Ref<InstanceType<typeof Pop>>;
const idenProvider = injectIdentityInfoProvider();
onMounted(async()=>{
    api = injectApi();
    pop = injectPop();
    username = props.username;
    if(username)
        setTitleTo(username)
    else
        setTitleTo('用户中心')
    if(!username){    
        if(iden.value.Id==0){
            pop.value.show("请登录","failed");
            jumpToLogin();
            return;
        }
        username = iden.value.Name
    }
    await load();
})
onUnmounted(()=>{
    recoverTitle()
})
</script>

<template>
    <div v-if="ok && user" class="user">
        <div class="info">
            <img :src="user?.AvatarSrc"/>
            <div class="username">{{ user?.Name }}</div>
            <div class="motto">暂无简介</div>
            <div class="settings"><button v-if="username==iden?.Name" @click="editInfoSidebar?.extend">编辑信息</button></div>
        </div>
        <SwitchingTabs style="width: 300px;height: 400px;" :texts="['最新作品','最近动态','自荐']">
            <div><LatestWork :uid="user.Id" :noWrap="true"></LatestWork></div>
            <OpRecord :user="user.Id"></OpRecord>
            <div style="text-align:center">暂未开放</div>
        </SwitchingTabs>
    </div>
    <div v-else><Loading></Loading></div>
    <SideBar ref="editInfoSidebar">
        <Personal v-if="user" :user="user" @require-reload="()=>load(true)"></Personal>
    </SideBar>
</template>

<style scoped lang="scss">
@import '@/styles/globalValues';

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
    object-fit: contain;
}

.user{
    display: flex;
    flex-direction: row;
    flex-wrap: wrap;
    justify-content: space-between;
    align-items: start;
    padding-top: 20px;
    box-sizing: border-box;
    height: $body-height;
}
.user>*{
    flex-grow: 1;
}
</style>