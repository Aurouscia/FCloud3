<script setup lang="ts">
import { Editor, TableData } from '@aurouscia/au-table-editor'
import '/node_modules/@aurouscia/au-table-editor/dist/style.css'
import Loading from '../../components/Loading.vue';
import { onMounted, ref } from 'vue';
import { injectApi } from '../../provides';
import { Api } from '../../utils/api';
import { FreeTable } from '../../models/table/freeTable';

const props = defineProps<{
    id:string
}>();

const tableInfo = ref<FreeTable>(); 
const tableData = ref<TableData>();

async function load(){
    tableInfo.value = await api.table.freeTable.load(parseInt(props.id));
    console.log(tableInfo.value);
    if(tableInfo.value && tableInfo.value.Data){
        tableData.value = JSON.parse(tableInfo.value.Data);
        console.log(tableData.value);
    }
}

var api:Api;
const loadComplete = ref<boolean>(false);
onMounted(async()=>{
    api = injectApi();
    await load();
    loadComplete.value = true;
})
</script>

<template>
<Editor v-if="loadComplete" :table-data="tableData">
</Editor>
<Loading v-else></Loading>
</template>

<style scoped>
</style>