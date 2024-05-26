<script setup lang="ts">
import { onMounted, ref, watch } from 'vue';
import { injectApi } from '../../provides';
import { Api } from '../../utils/com/api';
import SideBar from '../../components/SideBar.vue';
import { wikiParaDefaultFoldMark } from '../../models/wiki/wikiPara';

const props = defineProps<{
    paraId:number,
    currentNameOverride:string|null
}>()
const emits = defineEmits<{
    (e:'close'):void,
    (e:'needReload'):void
}>()
defineExpose({comeout})

const nameOverride = ref<string|null>("");
const defaultFold = ref<boolean>(false);

async function save(){
    let newName = nameOverride.value?.trim();
    if(defaultFold.value && newName){
        newName = wikiParaDefaultFoldMark+newName;
    }
    const resp = await api.wiki.para.setInfo(props.paraId, newName || null)
    if(resp){
        emits('needReload')
        hide();
    }
}
function comeout(){
    nameOverride.value = props.currentNameOverride;
    if(nameOverride.value?.startsWith(wikiParaDefaultFoldMark)){
        defaultFold.value = true;
        nameOverride.value = nameOverride.value.substring(1);
    }
    else{
        defaultFold.value = false;
    }
    sidebar.value?.extend()
}
function hide(){
    sidebar.value?.fold();
}
watch(nameOverride,(newVal)=>{
    if(!newVal?.trim()){
        defaultFold.value = false;
    }
})

const sidebar = ref<InstanceType<typeof SideBar>>()
let api:Api;
onMounted(()=>{
    api = injectApi();
})
</script>

<template>
<SideBar ref="sidebar" @fold="emits('close')">
    <h1>段落信息</h1>
    <table>
        <tr>
            <td>
                名称
            </td>
            <td>
                <input v-model="nameOverride" placeholder="选填"/>
            </td>
        </tr>
        <tr>
            <td>
                默认<br/>折起
            </td>
            <td>
                <input type="checkbox" v-model="defaultFold" :disabled="!nameOverride"/>
            </td>
        </tr>
        <tr>
            <td class="note" colspan="2">
                段落名称将覆盖段落内容本身(文本段/表格/文件名)的名称作为词条中的次级标题显示
            </td>
        </tr>
        <tr>
            <td class="note" colspan="2">
                “默认折起”对文件段落无效
            </td>
        </tr>
        <tr>
            <td colspan="2">
                <button @click="save">保存</button>
            </td>
        </tr>
    </table>
</SideBar>
</template>

<style scoped>
input{
    width: 160px;
}
td{
    min-width: 30px;
}
.note{
    color:#666;
    font-size: 12px;
}
</style>