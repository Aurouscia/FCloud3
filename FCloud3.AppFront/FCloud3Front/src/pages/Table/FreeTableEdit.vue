<script setup lang="ts">
import { Editor, TableData } from '@aurouscia/au-table-editor'
import '/node_modules/@aurouscia/au-table-editor/dist/style.css'
import Loading from '../../components/Loading.vue';
import { onMounted, ref } from 'vue';
import { injectApi, injectHideTopbar } from '../../provides';
import { Api } from '../../utils/api';
import { FreeTable } from '../../models/table/freeTable';

const props = defineProps<{
    id:string
}>();

const tableInfo = ref<FreeTable>(); 
const tableData = ref<TableData>();

async function load(){
    const id = parseInt(props.id);
    if(!id){console.error("路由格式有误"); return;}
    tableInfo.value = await api.table.freeTable.load(id);
    if(tableInfo.value && tableInfo.value.Data){
        tableData.value = JSON.parse(tableInfo.value.Data);
    }
}
async function editName() {
    if(!tableInfo.value?.Name){return;}
    await api.table.freeTable.saveInfo(parseInt(props.id),tableInfo.value.Name);
}
async function editContent(val:TableData, callBack:(success:boolean,msg:string)=>void) {
    const data = JSON.stringify(val);
    const resp = await api.table.freeTable.saveContent(parseInt(props.id),data);
    if(resp.success){
        callBack(true,"成功保存");
    }else{
        callBack(false,resp.errmsg);
    }
}

var api:Api;
const loadComplete = ref<boolean>(false);
onMounted(async()=>{
    api = injectApi();
    var hideTopbar = injectHideTopbar();
    hideTopbar();
    await load();
    loadComplete.value = true;
})
</script>

<template>
<div v-if="tableInfo" class="freeTableEdit">
    <Editor v-if="loadComplete" :table-data="tableData" @save="editContent">
    </Editor>
    <Loading v-else></Loading>
    <input class="name" v-model="tableInfo.Name" @blur="editName" placeholder="表格名称(必填)"/>
</div>
</template>

<style scoped>
.name{
    position: absolute;
    top:2px;
    right: 45px;
    width: 100px;
    z-index: 10000;
    border-radius: 5px;
}
</style>
<style>
    .tableEditor .author{
        padding-right: 10px;
    }
</style>