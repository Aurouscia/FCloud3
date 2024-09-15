<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue';
import { injectApi, injectPop } from '@/provides';
import { Api } from '@/utils/com/api';
import SideBar from '@/components/SideBar.vue';
import { WikiParaDisplay, wikiParaDefaultFoldMark } from '@/models/wiki/wikiPara';
import { WikiParaType } from '@/models/wiki/wikiParaType';
import { getFileExt } from '@/utils/fileUtils';

const props = defineProps<{
    para:WikiParaDisplay
}>()
const emits = defineEmits<{
    (e:'close'):void,
    (e:'needReload'):void
}>()
defineExpose({comeout})

const name = ref<string>()
let originalExt = '';
let originalName = '';
const nameOverride = ref<string|null>("");
const defaultFold = ref<boolean>(false);
const nameChanged = ref(false);
const nameOverrideChanged = ref(false);

async function save() {
    let success = true;
    if(nameChanged.value){
        const res = await saveName();
        success = res && success;
    }
    if(nameOverrideChanged.value){
        const res = await saveNameOverride();
        success = res && success;
    }
    if(success){
        nameChanged.value = false;
        nameOverrideChanged.value = false;
        hide();
    }
}
async function saveNameOverride():Promise<boolean> {
    let newName = nameOverride.value?.trim();
    if(defaultFold.value && newName){
        newName = wikiParaDefaultFoldMark+newName;
    }
    const s = await api.wiki.wikiPara.setInfo(props.para.ParaId, newName || null)
    if(s){
        props.para.NameOverride = newName || null;
    }
    return s;
}
async function saveName():Promise<boolean> {
    let s = false;
    if(props.para.Type == WikiParaType.Text){
        s = await api.textSection.textSection.editExe({
            Title: name.value || "",
            Content: null,
            Id: props.para.UnderlyingId
        })
    }
    else if(props.para.Type == WikiParaType.Table){
        s = await api.table.freeTable.saveInfo(
            props.para.UnderlyingId,
            name.value || ""
        )
    }
    else if(props.para.Type == WikiParaType.File){
        s = await api.files.fileItem.editInfo(
            props.para.UnderlyingId,
            name.value || ""
        )
    }
    else{
        pop.value.show("页面参数异常","failed");
        return false;
    }
    if(s){
        props.para.Title = name.value||"";
    }
    return s
}
function comeout(){
    nameOverride.value = props.para.NameOverride;
    if(nameOverride.value?.startsWith(wikiParaDefaultFoldMark)){
        defaultFold.value = true;
        nameOverride.value = nameOverride.value.substring(1);
    }
    else{
        defaultFold.value = false;
    }
    initName();
    sidebar.value?.extend()
}
function hide(){
    sidebar.value?.fold();
}

async function initName(){
    if(!props.para.UnderlyingId)
        return;
    let n:string|undefined;
    if(props.para.Type == WikiParaType.Text){
        n = (await api.textSection.textSection.getMeta(props.para.UnderlyingId))?.Title || undefined
    }else if(props.para.Type == WikiParaType.Table){
        n = (await api.table.freeTable.getMeta(props.para.UnderlyingId))?.Name || undefined
    }else if(props.para.Type == WikiParaType.File){
        n = (await api.files.fileItem.getInfo(props.para.UnderlyingId))?.DisplayName
    }else{
        pop.value.show("页面参数异常", "failed")
    }
    name.value = n;
    originalName = n || '';
    originalExt = getFileExt(n || '', false, false);
}

const extChanged = computed<boolean>(()=>{
    if(props.para.Type !== WikiParaType.File)
        return false;
    const ext = getFileExt(name.value || '', false, false)
    return originalExt!==ext
})

watch(nameOverride,(newVal)=>{
    if(!newVal?.trim()){
        defaultFold.value = false;
    }
})

const sidebar = ref<InstanceType<typeof SideBar>>()
let api:Api = injectApi();
let pop = injectPop();
onMounted(async()=>{
    await initName();
})
</script>

<template>
<SideBar ref="sidebar" @fold="emits('close')">
    <h1>段落信息</h1>
    <table>
        <tr>
            <td>
                内容名称
            </td>
            <td v-if="para.UnderlyingId>0">
                <input v-model="name" placeholder="必填" @input="nameChanged = true"/>
                <div v-if="extChanged" class="extWarn">
                    文件后缀名变更<br/>会造成下载后无法打开<br/>
                    需要词条中不显示后缀<br/>请填写下方的框<br/>
                    <button class="lite" @click="name = originalName">点击恢复后缀名</button>
                </div>
            </td>
            <td v-else>
                请先点击"编辑"<br/>向段落写入内容
            </td>
        </tr>
        <tr>
            <td>
                词条内<br/>显示名称
            </td>
            <td>
                <input v-model="nameOverride" placeholder="选填" @input="nameOverrideChanged = true"/>
            </td>
        </tr>
        <tr>
            <td>
                默认<br/>折起
            </td>
            <td>
                <input type="checkbox" v-model="defaultFold" :disabled="!nameOverride" @change="nameOverrideChanged = true"/>
            </td>
        </tr>
        <tr>
            <td class="note" colspan="2">
                太长不看：只填第一个即可。<br/><br/>
                <b>词条内显示名称</b>将覆盖段落内容本身(文本段/表格/文件名)的名称作为词条中的次级标题显示。
                例如“内容名称”可能为<i>上海市轨道交通线路</i>，其“词条内显示名称”可为<i>本市轨道交通线路</i>
            </td>
        </tr>
        <tr v-if="para.Type===WikiParaType.File">
            <td class="note" colspan="2">
                “默认折起”对文件段落无效<br/><br/>
                注意更改“内容名称”时不要改坏文件的后缀名
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

<style scoped lang="scss">
input{
    width: 160px;
}
td{
    min-width: 60px;
}
.note{
    color:#666;
    font-size: 12px;
    i{
        margin: 0px 2px 0px 2px;
    }
}
.extWarn{
    color: red;
    font-size: 12px;
}
</style>