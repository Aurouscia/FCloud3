<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { injectApi } from '../provides';
import { Api } from '../utils/api';
import { AuthGrantOnText, AuthGrantToText, AuthGrantViewModel, authGrantOn, authGrantTo } from '../models/identities/authGrant';
import Search from './Search.vue';
import { elementBlinkClass } from '../utils/elementBlink';

const props = defineProps<{
    on:AuthGrantOnText
    onId:number
}>();

const data = ref<AuthGrantViewModel[]>();
const touchedOrder = ref<boolean>(false);
async function load(){
    const resp = await api.identites.authGrant.getList(props.on,props.onId);
    if(resp){
        data.value = resp;
    }
    touchedOrder.value = false;
}

const adding = ref<boolean>(false);
const addingTo = ref<AuthGrantToText>("UserGroup");
const addingIsReject = ref<boolean>(false);
async function toSearchDone(_value:string, id:number){
    const to = authGrantTo(addingTo.value);
    const on = authGrantOn(props.on);
    const resp = await api.identites.authGrant.add({
        Id: 0,
        To: to,
        ToId: id,
        On: on,
        OnId: props.onId,
        IsReject:addingIsReject.value
    })
    if(resp){
        await load();
        adding.value = false;
    }
}
async function remove(id:number) {
    const resp = await api.identites.authGrant.remove(id);
    if(resp){
        await load();
    }
}

const rowId = (id:number)=>`auth_grant_row_${id}`;
async function moveUp(id:number) {
    if(!data.value){return;}
    const idx = data.value.findIndex(x=>x.Id==id);
    if(idx>0){
        const ele = document.getElementById(rowId(id));
        if(ele){
            await elementBlinkClass(ele, 'blinking', 1);
        }
        const it = data.value.splice(idx,1);
        data.value.splice(idx-1,0,...it);
        touchedOrder.value = true;
        if(ele){
            setTimeout(()=>{
                elementBlinkClass(ele, 'blinking', 1);
            }, 10);
        }
    }
}
async function saveOrder() {
    if(!data.value){return;}
    const ids = data.value.map(x=>x.Id);
    const resp = await api.identites.authGrant.setOrder(props.on, props.onId, ids);
    if(resp){
        await load();
    }
}

var api:Api;
onMounted(()=>{
    api = injectApi();
    load();
})
</script>

<template>
<div class="authGrants">
    <div>
        <button @click="adding = !adding">添加</button>
        <button v-show="touchedOrder" @click="saveOrder">保存顺序更改</button>
        <div class="addPanel" v-if="api" v-show="adding">
            <table>
                <tr>
                    <td>对象类型</td>
                    <td>
                        <select v-model="addingTo">
                            <option :value="'UserGroup'">用户组</option>
                            <option :value="'User'">单个用户</option>
                            <option :value="'EveryOne'">所有人</option>
                        </select>
                    </td>
                </tr>
                <tr>
                    <td>设置类型</td>
                    <td>
                        <select v-model="addingIsReject">
                            <option :value="false">允许</option>
                            <option :value="true">阻止</option>
                        </select>
                    </td>
                </tr>
                <tr>
                    <td colspan="2">
                        <Search v-show="addingTo == 'UserGroup'" :source="api.utils.quickSearch.userGroupName" @done="toSearchDone"></Search>
                        <Search v-show="addingTo == 'User'" :source="api.utils.quickSearch.userName" @done="toSearchDone"></Search>
                        <button v-show="addingTo == 'EveryOne'" @click="toSearchDone('',0)">确认</button>
                    </td>
                </tr>
            </table>
        </div>
    </div>
    <table v-if="data && data.length>0">
        <tr v-for="ag in data" :key="ag.Id" :id="rowId(ag.Id)">
            <td class="creator">
                {{ ag.CreatorName }}
            </td>
            <td>
                <div v-if="ag.IsReject" class="reject">阻止</div>
                <div v-else class="allow">允许</div>
            </td>
            <td>
                {{ ag.ToName }}
            </td>
            <td>
                <div class="agOp">
                    <button class="lite" @click="moveUp(ag.Id)">上移</button>
                    <button class="lite" @click="remove(ag.Id)">删除</button>
                </div>
            </td>
        </tr>
    </table>
    <div v-else>
        暂无
    </div>
</div>
</template>

<style scoped>
.agOp{
    display: flex;
    justify-content: space-around;
    align-items: center;
}
button.lite{
    font-size: small;
}
.addPanel{
    position: absolute;
    top:35px;
    width: 300px;
    overflow: visible;
    background-color: white;
    border: 2px solid #aaa;
    border-radius: 5px;
    padding: 10px;
}
select{
    margin: 0px;
}
.reject{
    color:palevioletred
}
.allow{
    color:olivedrab
}
.authGrants{
    min-height: 500px;
}
table{
    table-layout: fixed;
    width: 100%;
    max-width: 500px;
}
tr.blinking td{
    background-color: #bbb;
}
td{
    white-space: wrap;
    word-break: break-all;
    transition: 0.2s;
}
</style>