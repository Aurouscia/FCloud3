<script setup lang="ts">
import { AuTableEditor, AuTableData } from '@aurouscia/au-table-editor'
import '@aurouscia/au-table-editor/style.css'
import Loading from '@/components/Loading.vue';
import { onMounted, onUnmounted, ref } from 'vue';
import { SetTopbarFunc, injectApi, injectSetTopbar } from '@/provides';
import { Api } from '@/utils/com/api';
import { FreeTable } from '@/models/table/freeTable';
import WikiTitleContain from '@/components/Wiki/WikiTitleContain.vue';
import { WikiTitleContainType } from '@/models/wiki/wikiTitleContain';
import { join } from 'lodash';
import SideBar from '@/components/SideBar.vue';
import UnsavedLeavingWarning from '@/components/UnsavedLeavingWarning.vue';
import { usePreventLeavingUnsaved } from '@/utils/eventListeners/preventLeavingUnsaved';
import { HeartbeatObjType, HeartbeatSender } from '@/models/etc/heartbeat';
import { recoverTitle, setTitleTo } from '@/utils/titleSetter';
import leaveImg from '@/assets/leave.svg';
import { useRouter } from 'vue-router';

const props = defineProps<{
    id:string
}>();
const router = useRouter();

const tableInfo = ref<FreeTable>(); 
const tableData = ref<AuTableData>();

async function load(){
    const id = parseInt(props.id);
    if(!id){console.error("路由格式有误"); return;}
    tableInfo.value = await api.table.freeTable.load(id);
    if(tableInfo.value && tableInfo.value.Data){
        tableData.value = JSON.parse(tableInfo.value.Data);
        heartbeatSender = new HeartbeatSender(api, HeartbeatObjType.FreeTable, id);
        heartbeatSender.start();
    }
}
async function editName() {
    if(!tableInfo.value)return;
    await api.table.freeTable.saveInfo(parseInt(props.id),tableInfo.value.Name || "");
}
async function editContent(val:AuTableData, callBack:(success:boolean,msg:string)=>void) {
    const data = JSON.stringify(val);
    const resp = await api.table.freeTable.saveContent(parseInt(props.id),data);
    if(resp.success){
        callBack(true,"成功保存");
        releasePreventLeaving();
    }else{
        callBack(false,resp.errmsg);
    }
}
function getContent() {
    if(!tableData.value){return;}
    return join(tableData.value.cells.map(row=>join(row, ',')), ',')
}
function leave(){
    router.back();
}

let api:Api;
let setTopBar: SetTopbarFunc|undefined;
const loadComplete = ref<boolean>(false);
const titleContainSidebar = ref<InstanceType<typeof SideBar>>();
let heartbeatSender:HeartbeatSender|undefined;
onMounted(async()=>{
    setTitleTo('表格编辑器')
    api = injectApi();
    setTopBar = injectSetTopbar();
    setTopBar(false);
    await load();
    loadComplete.value = true;
})
onUnmounted(()=>{
    recoverTitle()
    if(setTopBar)
        setTopBar(true);
    heartbeatSender?.stop();
})

const { preventLeaving, releasePreventLeaving, preventingLeaving , showUnsavedWarning } = usePreventLeavingUnsaved()
</script>

<template>
<div v-if="tableInfo && loadComplete" class="freeTableEdit">
    <AuTableEditor v-if="tableData" :table-data="tableData" @save="editContent" @changed="preventLeaving">
    </AuTableEditor>
    <img class="leaveBtn" v-if="!preventingLeaving" :src="leaveImg" @click="leave"/>
    <div class="preventingLeaving" v-show="preventingLeaving"></div>
    <input class="name" v-model="tableInfo.Name" @blur="editName" placeholder="表格名称(必填)"/>
    <button class="titleContainBtn minor" @click="titleContainSidebar?.extend">链接</button>
    <SideBar ref="titleContainSidebar">
        <WikiTitleContain :type="WikiTitleContainType.FreeTable" :object-id="tableInfo.Id" :get-content="getContent">
        </WikiTitleContain>
    </SideBar>
    <UnsavedLeavingWarning v-if="showUnsavedWarning" :release="releasePreventLeaving" @ok="showUnsavedWarning = false"></UnsavedLeavingWarning>
</div>
<Loading v-else></Loading>
</template>

<style scoped lang="scss">
.leaveBtn{
    width: 30px;
    height: 30px;
    background-color: olivedrab;
    border-radius: 5px;
    position: absolute;
    top:4px;
    right: 3px;
    z-index: 19900;
    cursor: pointer;
    transition: 0.5s;
}
.leaveBtn:hover{
    background-color: green;
}
.preventingLeaving{
    width: 10px;
    height: 10px;
    background-color: red;
    border-radius: 50%;
    position: absolute;
    top:5px;
    right: 5px;
    z-index: 19900;
}
.titleContainBtn{
    position: absolute;
    top: 1px;
    padding: 3px;
    right: 140px;
    z-index: 900;
}
.name{
    position: absolute;
    top:1px;
    right: 30px;
    width: 100px;
    z-index: 900;
    border-radius: 5px;
}
</style>
<style>
    .tableEditor .control{
        height: 38px;
        box-sizing: border-box;
    }
</style>