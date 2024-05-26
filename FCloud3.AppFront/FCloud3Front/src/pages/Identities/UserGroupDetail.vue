<script setup lang="ts">
import { inject, onMounted, ref, watch } from 'vue';
import { Api } from '../../utils/com/api';
import { UserGroup, UserGroupDetailResult } from '../../models/identities/userGroup';
import Loading from '../../components/Loading.vue';
import Search from '../../components/Search.vue';

const props = defineProps<{
    id:number
}>()

const data = ref<UserGroupDetailResult>();
const info = ref<UserGroup>();
async function loadData(){
    if(props.id==0){return;}
    var timer = setTimeout(()=>{
        data.value = undefined;
    },500);
    var res = await api.identites.userGroup.getDetail(props.id);
    if(res){
        info.value = {
            Id:res.Id,
            Name:res.Name
        }
        data.value = res;
        clearTimeout(timer);
    }
}
const inviteUserSearch = ref<InstanceType<typeof Search>>();
async function inviteUser(_name:string, userId:number){
    var res = await api.identites.userGroup.addUserToGroup(userId,props.id)
    if(res){
        inviteUserSearch.value?.clear();
        loadData();
        emit('needRefresh');
    }
}
async function leaveGroup() {
    var res = await api.identites.userGroup.leave(props.id);
    if(res){
        loadData();
        emit('needRefresh');
    }
}
async function editInfo() {
    if(!info.value){return;}
    var res = await api.identites.userGroup.editExe(info.value)
    if(res){
        loadData();
        emit('needRefresh');
    }
}
async function removeUser(userId:number) {
    const res = await api.identites.userGroup.removeUserFromGroup(userId, props.id);
    if(res){
        loadData();
        emit('needRefresh');
    }
}

const emit = defineEmits<{
    (e:'needRefresh'):void
}>()
defineExpose({loadData})

var api:Api
onMounted(async()=>{
    api = inject('api') as Api;
    loadData();
})
watch(props,async ()=>{
    await loadData();
})
</script>

<template>
    <div v-if="props.id!==0" class="userGroupDetail">
        <div v-if="data" class="items">
            <div class="info">
                <div class="name">{{ data.Name }}</div>
                <div>组长: <RouterLink :to="`/u/${data.Owner}`">{{ data.Owner }}</RouterLink></div>
                <div>人数: {{ data.FormalMembers.length }}</div>
            </div>
            <div v-if="data.CanInvite" class="search">
                <Search ref="inviteUserSearch" :source="api.utils.quickSearch.userName" :allow-free-input="false" 
                    :no-result-notice="'未找到该用户'" :placeholder="'邀请用户加入'" @done="inviteUser"></Search>
            </div>
            <table>
                <tr v-if="data.Inviting && data.Inviting.length>0">
                    <th :colspan="data.CanEdit?2:1">已邀请用户</th>
                </tr>
                <tr v-for="i in data.Inviting">
                    <td class="memberName">{{ i.Name }}</td>
                    <td v-if="data.CanEdit"></td>
                </tr>
                <tr>
                    <th :colspan="data.CanEdit?2:1">成员列表</th>
                </tr>
                <tr v-for="m in data.FormalMembers">
                    <td class="memberName">{{ m.Name }}</td>
                    <td v-if="data.CanEdit">
                        <button class="minor" @click="removeUser(m.Id)">移出</button>
                    </td> 
                </tr>
            </table>
            <table v-if="info && data.CanEdit">
                <th colspan="2">编辑信息</th>
                <tr>
                    <td>名称</td>
                    <td><input v-model="info.Name"/></td>
                </tr>
                <tr>
                    <td colspan="2"><button @click="editInfo">保存</button></td>
                </tr>
            </table>
            <button v-if="data.IsMember" @click="leaveGroup" class="cancel">退出群组</button>
        </div>
        <Loading v-else></Loading>
    </div>
    <div v-else class="userGroupDetail" style="color:#999">
        请点击用户组名称查看详情
    </div>
</template>

<style scoped>
a{
    color:white
}
.memberName{
    width: 200px;
}
.memberName a{
    color:black;
}
th{
    background-color: #999;
    color:white;
}
.userGroupDetail{
    flex-basis: 250px;
    flex-grow: 1;
}
.items{
    display: flex;
    flex-direction: column;
    gap:10px;
    align-items: stretch;
}
.ops{
    display: flex;
    flex-direction: column;
    align-items: center;
}
.info{
    background-color: cornflowerblue;
    color:white;
    padding: 5px;
    border-radius: 5px;
}
.info>div{
    margin-bottom: 5px;
    margin-left: 10px;
}
.name{
    font-size: 20px;
    font-weight: bold;
}
</style>