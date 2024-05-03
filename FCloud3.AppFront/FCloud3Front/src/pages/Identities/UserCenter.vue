<script setup lang="ts">
import { Ref, onMounted, onUnmounted, ref, watch } from 'vue';
import SideBar from '../../components/SideBar.vue';
import Personal from './PersonalSettings.vue';
import Loading from '../../components/Loading.vue';
import IndexWikiItem from '../../components/Index/IndexWikiItem.vue'
import { User } from '../../models/identities/user';
import { Api } from '../../utils/api';
import SwitchingTabs from '../../components/SwitchingTabs.vue';
import { IdentityInfo } from '../../utils/userInfo';
import { injectApi, injectPop, injectUserInfo } from '../../provides';
import { jumpToLogin } from './routes';
import Pop from '../../components/Pop.vue';
import { recoverTitle, setTitleTo } from '../../utils/titleSetter';

const props = defineProps<{
    username?:string
}>();
const user = ref<User>();
var api:Api;
const editInfoSidebar = ref<InstanceType<typeof SideBar>>();
const ok = ref<boolean>(false);

watch(props, _newVal=>{
    location.reload();
})

async function load(){
    let nameChanged = false;
    //user.value可能被个人设置组件所更改，这里需要检查是否有改动，有改动就跳转去登录页面
    if(user.value){
        if(username != user.value.Name){
            nameChanged = true;
        }
        username = user.value.Name;
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
let identity:IdentityInfo|undefined;
let pop: Ref<InstanceType<typeof Pop>>;
onMounted(async()=>{
    api = injectApi();
    pop = injectPop();
    username = props.username;
    if(username)
        setTitleTo(username)
    else
        setTitleTo('用户中心')
    identity = await injectUserInfo().getIdentityInfo();
    if(!username){    
        if(identity.Id==0){
            pop.value.show("请登录","failed");
            jumpToLogin();
            return;
        }
        username = identity.Name
    }
    await load();
})
onUnmounted(()=>{
    recoverTitle()
})
</script>

<template>
    <div v-if="ok" class="user">
        <div class="info">
            <img :src="user?.AvatarSrc"/>
            <div class="username">{{ user?.Name }}</div>
            <div class="motto">暂无简介</div>
            <div class="settings"><button v-if="username==identity?.Name" @click="editInfoSidebar?.extend">编辑信息</button></div>
        </div>
        <SwitchingTabs style="width: 300px;height: 400px;" :texts="['用户动态','自荐作品','最新作品']">
            <IndexWikiItem></IndexWikiItem>
            <div>自荐作品</div>
            <div>最新作品</div>
        </SwitchingTabs>
    </div>
    <div v-else><Loading></Loading></div>
    <SideBar ref="editInfoSidebar">
        <Personal v-if="user" :user="user" @require-reload="load"></Personal>
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
    object-fit: contain;
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