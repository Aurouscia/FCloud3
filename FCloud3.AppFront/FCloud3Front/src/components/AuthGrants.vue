<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { injectApi } from '../provides';
import { Api } from '../utils/api';
import { AuthGrantOn, AuthGrantTo, AuthGrantViewModel } from '../models/identities/authGrant';
import Search from './Search.vue';
import { elementBlinkClass } from '../utils/elementBlink';

const props = defineProps<{
    on:AuthGrantOn
    onId:number
}>();

const data = ref<AuthGrantViewModel>();
const touchedOrder = ref<boolean>(false);
async function load(){
    const resp = await api.identites.authGrant.getList(props.on,props.onId);
    if(resp){
        data.value = resp;
    }
    touchedOrder.value = false;
}

const adding = ref<boolean>(false);
const addingTo = ref<AuthGrantTo>(AuthGrantTo.UserGroup);
const addingIsReject = ref<boolean>(false);
async function toSearchDone(_value:string, id:number){
    const resp = await api.identites.authGrant.add({
        Id: 0,
        To: addingTo.value,
        ToId: id,
        On: props.on,
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
    const idx = data.value.Local.findIndex(x=>x.Id==id);
    if(idx>0){
        const ele = document.getElementById(rowId(id));
        if(ele){
            await elementBlinkClass(ele, 'blinking', 1, 100);
        }
        const it = data.value.Local.splice(idx,1);
        data.value.Local.splice(idx-1,0,...it);
        touchedOrder.value = true;
        if(ele){
            setTimeout(()=>{
                elementBlinkClass(ele, 'blinking', 1, 100);
            }, 10);
        }
    }
}
async function saveOrder() {
    if(!data.value){return;}
    const ids = data.value.Local.map(x=>x.Id);
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
    <div class="func">
        <button @click="adding = !adding" :class="{cancel:adding}">{{adding?'取消添加':'添加授权'}}</button>
        <button v-show="touchedOrder" @click="saveOrder" class="ok">保存顺序更改</button>
        <div class="addPanel" v-if="api" v-show="adding">
            <table>
                <tr>
                    <td>对象类型</td>
                    <td>
                        <select v-model="addingTo">
                            <option :value="AuthGrantTo.UserGroup">用户组</option>
                            <option :value="AuthGrantTo.User">单个用户</option>
                            <option :value="AuthGrantTo.EveryOne">所有人</option>
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
                        <Search v-show="addingTo == AuthGrantTo.UserGroup" :source="api.utils.quickSearch.userGroupName" @done="toSearchDone" :compact="true"></Search>
                        <Search v-show="addingTo == AuthGrantTo.User" :source="api.utils.quickSearch.userName" @done="toSearchDone" :compact="true"></Search>
                        <button v-show="addingTo == AuthGrantTo.EveryOne" @click="toSearchDone('',0)">确认</button>
                    </td>
                </tr>
            </table>
        </div>
    </div>
    <table v-if="data">
        <tr v-for="ag in data.BuiltIn">
            <td class="creator">系统</td>
            <td>
                <div v-if="ag.IsReject" class="reject">阻止</div>
                <div v-else class="allow">允许</div>
            </td>
            <td>
                {{ ag.ToName }}
            </td>
            <td><div class="overlapNote">系统内置</div></td>
        </tr>
        <tr v-for="ag in data.Global">
            <td class="creator">{{ ag.CreatorName }}</td>
            <td>
                <div v-if="ag.IsReject" class="reject">阻止</div>
                <div v-else class="allow">允许</div>
            </td>
            <td>
                {{ ag.ToName }}
            </td>
            <td><div class="overlapNote">全局设置</div></td>
        </tr>
        <tr v-for="ag in data.Local" :key="ag.Id" :id="rowId(ag.Id)">
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
        <tr v-if="data.Local.length>1">
            <td colspan="4" class="overlapNote">
                下方的规则会覆盖上方的
            </td>
        </tr>
    </table>
    <div v-if="data?.Global && data.Global.length>0" class="defaultNote">
        "全局设置"请在个人中心调整
    </div>
</div>
</template>

<style scoped>
.defaultNote{
    color:gray;
    font-size: 14px;
    margin: 10px 10px 10px 0px;
    text-align: center;
}
.overlapNote{
    color:gray;
    font-size: 14px;
    background-color: transparent;
}
.tempNone{
    color:gray
}
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
    top:45px;
    left:0px;right: 0px;
    width: 230px;
    margin: auto;
    overflow: visible;
    background-color: white;
    box-shadow: 0 0 10px 0 black;
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
.func{
    margin: 10px 0px 10px 0px;
}
.authGrants{
    min-height: 500px;
    display: flex;
    flex-direction: column;
    align-items: center;
    position: relative;
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