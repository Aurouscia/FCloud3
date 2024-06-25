<script setup lang="ts">
import { WikiItem } from '@/models/wiki/wikiItem';
import { WikiParaDisplay, wikiParaDefaultFoldMark, wikiParaDisplayPlaceholder } from '@/models/wiki/wikiPara';
import { injectApi } from '@/provides';
import { onMounted, ref, nextTick, onUnmounted } from 'vue';
import Loading from '@/components/Loading.vue';
import { AuTableEditor,  AuTableData } from '@aurouscia/au-table-editor';
import { WikiParaType } from '@/models/wiki/wikiParaType';
import WikiParaInfo from './WikiParaInfo.vue';
import FileParaListItem from './ParaListItem/FileParaListItem.vue';
import ImageFocusView from '@/components/ImageFocusView.vue';
import WikiFileParaEdit from './WikiFileParaEdit.vue';
import SideBar from '@/components/SideBar.vue';
import { ImageClickJump } from '@/utils/wikiView/imgClickJump';

const props = defineProps<{
    urlPathName: string
}>()

interface WikiParaDisplayEdit extends WikiParaDisplay{
    changed?: boolean
}

const api = injectApi();
const info = ref<WikiItem>();
const paras = ref<WikiParaDisplayEdit[]>();
const ready = ref(false);
const tableDatas:{tableId: number, data: AuTableData}[] = [];
async function load(){
    info.value = await api.wiki.wikiItem.edit(props.urlPathName);
    if(info.value){
        paras.value = await api.wiki.wikiItem.loadFull(info.value.Id)
        if(paras.value){
            paras.value.forEach(t=>{
                if(t.Type == WikiParaType.Table)
                    tableDatas.push({tableId:t.UnderlyingId, data:JSON.parse(t.Content)})
            });
            ready.value = true
        }
    }
}

const wikiParaInfo = ref<InstanceType<typeof WikiParaInfo>>(); 
const editingPara = ref<WikiParaDisplay>(wikiParaDisplayPlaceholder);
const wikiFileParaEdit = ref<InstanceType<typeof SideBar>>();
async function startEditingInfo(p:WikiParaDisplay){
    editingPara.value = p;
    await nextTick();
    wikiParaInfo.value?.comeout();
}
async function startEditingFilePara(p:WikiParaDisplay) {
    editingPara.value = p;
    await nextTick();
    wikiFileParaEdit.value?.extend();
}
async function moveUp(idx:number) {
    if(idx == 0 || !info.value?.Id){return}
    const ids = paras.value?.map(x=>x.ParaId);
    if(!ids){return;}
    const targetId = ids.splice(idx,1)[0];
    ids.splice(idx-1,0, targetId);
    const s = await api.wiki.wikiItem.setParaOrders({id: info.value.Id, orderedParaIds:ids})
    if(s){
        const temp = paras.value || [];
        paras.value = [];
        ids.forEach(id=>{
            const p = temp?.find(x=>x.ParaId == id)
            if(p)
                paras.value?.push(p)
        })
    }
}

function defaultParaTitle(t:WikiParaType){
    if(t == WikiParaType.Text){
        return "文本段落"
    }else if(t == WikiParaType.Table){
        return "表格段落"
    }else if(t == WikiParaType.File){
        return "文件"
    }
}
function removeDefaultFoldMark(t:string|null){
    if(!t)
        return null;
    if(t.startsWith(wikiParaDefaultFoldMark)){
        return t.slice(1)
    }
    return t;
}

function tableChanged(p:WikiParaDisplayEdit){
    p.changed = true;
}
async function tableSave(data:AuTableData, cb:(s:boolean, msg:string)=>void, p:WikiParaDisplayEdit){
    const resp = await api.table.freeTable.saveContent(p.UnderlyingId, JSON.stringify(data));
    if(resp.success){
        cb(true,"成功保存");
    }else{
        cb(false,resp.errmsg);
    }
}

let imgClickJump:ImageClickJump;
const focusImg = ref<string>();
const parasDiv = ref<HTMLDivElement>();
onMounted(async()=>{
    await load();
    await nextTick();
    imgClickJump = new ImageClickJump(src=>{
        focusImg.value = src;
    });
    imgClickJump.listen(parasDiv.value);
})
onUnmounted(()=>{
    imgClickJump.dispose();
})
</script>

<template>
<div v-if="info && ready">
    <h1>{{ info.Title }}</h1>
    <div class="paras" ref="parasDiv">
        <div v-for="p,idx in paras" :key="p.ParaId" class="para">
            <div class="paraTop">
                <h2>
                    {{ removeDefaultFoldMark(p.NameOverride) || p.Title || defaultParaTitle(p.Type) }}
                    <span class="defaultFold">{{ p.NameOverride?.startsWith('^') ? '(默认折起)':''}}</span>
                </h2>
                <div class="ops">
                    <button v-if="p.Type == WikiParaType.File" class="lite" @click="startEditingFilePara(p)">编辑</button>
                    <button class="lite" @click="moveUp(idx)">上移</button>
                    <button class="lite" @click="startEditingInfo(p)">设置</button>
                </div>
            </div>
            <div v-if="p.Type == WikiParaType.Text" class="text">
                {{ p.Content }}
                <div v-if="!p.Content || p.Content.endsWith('\n')" style="height: 20px;">.</div>
                <textarea v-model="p.Content"></textarea>
            </div>
            <div v-else-if="p.Type == WikiParaType.Table" class="table">
                <AuTableEditor 
                    :table-data="tableDatas.find(x=>x.tableId == p.UnderlyingId)?.data" 
                    :no-shortcut="true"
                    :on-changed="()=>tableChanged(p)"
                    :on-save="(val, cb)=>tableSave(val, cb, p)"></AuTableEditor>
            </div>
            <div v-else-if="p.Type == WikiParaType.File" class="file">
                <FileParaListItem :w="p" :no-title="true"></FileParaListItem>
            </div>
        </div>
    </div>
</div>
<Loading v-else></Loading>
<WikiParaInfo :para="editingPara"  ref="wikiParaInfo"></WikiParaInfo>
<ImageFocusView v-if="focusImg" :img-src="focusImg" :close="()=>{focusImg=undefined}" ref="imgFocusViewElement">
</ImageFocusView>
<SideBar ref="wikiFileParaEdit">
    <WikiFileParaEdit :para-id="editingPara.ParaId" :file-id="editingPara.UnderlyingId" @file-id-set="load"></WikiFileParaEdit>
</SideBar>
</template>

<style scoped lang="scss">
.file{
    height: 110px;
    position: relative;
}
.table{
    position: relative;
    height: 50vh;
}
.text{
    position: relative;
    font-size: 16px;
    line-height: 20px;
    white-space: pre-line;
    padding: 5px;
    word-break: break-all;
    textarea{
        font-family: unset;
        font-size: 16px;
        line-height: 20px;
        position: absolute;
        top: 0px;bottom: 0px;
        left: 0px;right: 0px;
        padding: 3px;
    }
}
.paraTop{
    display: flex;
    justify-content: space-between;
    align-items: center;
    img{
        width: 20px;
        height: 20px;
    }
    .defaultFold{
        font-size: 14px;
        color:#666
    }
    .ops{
        display: flex;
        gap: 10px;
        white-space: nowrap;
    }
    h2{
        font-size: 18px;
        margin-bottom: 4px;
        color: white;
        background-color: cornflowerblue;
        padding: 3px;
        border-radius: 3px;
        flex-shrink: 1;
        white-space: nowrap;
        text-overflow: ellipsis;
    }
}
.paras{
    display: flex;
    flex-direction: column;
    gap:10px;
}
.para{
    background-color: #e8e8e8;
    padding: 5px;
}
</style>