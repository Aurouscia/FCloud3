<script setup lang="ts">
import { nextTick, onMounted, ref } from 'vue';
import { injectApi } from '../../provides';
import { Api } from '../../utils/api';
import Index, { IndexColumn } from '../../components/Index/Index.vue';
import { IndexResult } from '../../components/Index';
import { UserIndexItem, getUserIndexItemsFromIndexResult } from '../../models/identities/user';
import SideBar from '../../components/SideBar.vue';
import ToUserOperation from './ToUserOperation.vue';

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
    api = injectApi();
    injected.value = true;
})
</script>

<template>
<Index v-if="injected" :fetch-index="api.identites.user.index" :columns="columns" @reload-data="onLoadData" ref="index">
    <tr v-for="u in users" @click="selectUser(u)">
        <td>
            <div class="nameAndAvatar">
                <img :src="u.Avatar"/>
                <div>{{ u.Name }}</div>
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
</template>

<style scoped lang="scss">
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
</style>