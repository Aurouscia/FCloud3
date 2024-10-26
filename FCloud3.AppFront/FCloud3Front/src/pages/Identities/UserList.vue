<script setup lang="ts">
import { onMounted, onUnmounted, ref } from 'vue';
import { injectApi } from '@/provides';
import { Api } from '@/utils/com/api';
import Index, { IndexColumn } from '@/components/Index/Index.vue';
import { IndexResult } from '@/components/Index';
import { UserIndexItem, getUserIndexItemsFromIndexResult } from '@/models/identities/user';
import SideBar from '@/components/SideBar.vue';
import ToUserOperation from './ToUserOperation.vue';
import { recoverTitle, setTitleTo } from '@/utils/titleSetter';

const columns:IndexColumn[] = [
    {
        name:"Name",
        alias:"用户名",
        canSearch:true,
        canSetOrder:true,
    },{
        name:"LastOperation",
        alias:"最后操作",
        canSearch:false,
        canSetOrder:true,
    },{
        name:"Type",
        alias:"类型",
        canSearch:false,
        canSetOrder:false,
    }
]

const users = ref<UserIndexItem[]>([]);
const index = ref<InstanceType<typeof Index>>();
function onLoadData(res:IndexResult){
    users.value = getUserIndexItemsFromIndexResult(res);
}

const selectedUser = ref<UserIndexItem>();
const toUserOperationSidebar = ref<InstanceType<typeof SideBar>>();
function selectUser(u:UserIndexItem){
    selectedUser.value = u;
    toUserOperationSidebar.value?.extend();
}

let api:Api;
const injected = ref<boolean>(false);
onMounted(async()=>{
    setTitleTo('用户列表')
    api = injectApi();
    injected.value = true;
})
onUnmounted(()=>{
    recoverTitle()
})
</script>

<template>
<div class="userList">
    <Index v-if="injected" :fetch-index="api.identites.user.index" :columns="columns" @reload-data="onLoadData" ref="index">
        <tr v-for="u in users" @click="selectUser(u)">
            <td>
                <div class="nameAndAvatar">
                    <img :src="u.Avatar"/>
                    <div class="uname">{{ u.Name }}</div>
                </div>
            </td>
            <td>
                {{ u.LastOperation }}
            </td>
            <td :style="{color:u.TypeColor}">
                {{ u.Type }}
            </td>
        </tr>
    </Index>
    <SideBar ref="toUserOperationSidebar">
        <ToUserOperation v-if="selectedUser" :user="selectedUser" @type-changed="index?.reloadData(); toUserOperationSidebar?.fold()"></ToUserOperation>
    </SideBar>
</div>
</template>

<style scoped lang="scss">
@use '@/styles/globalValues';

.userList{
    box-sizing: border-box;
    padding-top: 10px;
    height: globalValues.$body-height;
    position: relative;
}
.nameAndAvatar{
    display: flex;
    gap:10px;
    justify-content: left;
    align-items: center;
    img{
        height: 25px;
        width: 25px;
        border-radius: 50%;
        object-fit: contain;
    }
}
td{
    cursor: pointer;
}
.uname{
    text-align: left;
    width: 220px;
    overflow: hidden;
    white-space: nowrap;
    text-overflow: ellipsis;
}
@media screen and (max-width: 600px){
    .uname{
        width: 120px;
    }
}
</style>