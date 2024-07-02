<script setup lang="ts">
import { WikiItem } from '@/models/wiki/wikiItem';
import { WikiParaDisplay, wikiParaDefaultFoldMark, wikiParaDisplayPlaceholder } from '@/models/wiki/wikiPara';
import { injectApi, injectPop, injectSetTopbar } from '@/provides';
import { onMounted, ref, nextTick, onUnmounted } from 'vue';
import Loading from '@/components/Loading.vue';
import { AuTableEditor,  AuTableData } from '@aurouscia/au-table-editor';
import { WikiParaType } from '@/models/wiki/wikiParaType';
import WikiParaInfo from './WikiParaInfo.vue';
import FileParaListItem from './ParaListItem/FileParaListItem.vue';
import ImageFocusView from '@/components/ImageFocusView.vue';
import WikiFileParaEdit from './WikiFileParaEdit.vue';
import SideBar from '@/components/SideBar.vue';
import UnsavedLeavingWarning from '@/components/UnsavedLeavingWarning.vue'
import { ImageClickJump } from '@/utils/wikiView/imgClickJump';
import { usePreventLeavingUnsaved } from '@/utils/eventListeners/preventLeavingUnsaved';
import { useRouter } from 'vue-router';
import { ShortcutListener } from '@aurouscia/keyboard-shortcut'
import { HeartbeatSenderForWholeWiki } from '@/models/etc/heartbeat';

const props = defineProps<{
    urlPathName: string
}>()

interface WikiParaDisplayEdit extends WikiParaDisplay{
    changed?: boolean,
    height?: number,
    tableData?:AuTableData,
    save?:()=>Promise<boolean>
}

const api = injectApi();
const router = useRouter();
const info = ref<WikiItem>();
const paras = ref<WikiParaDisplayEdit[]>();
const selectedPara = ref(0);
const ready = ref(false);
const paraMode = ref(false);
async function load(){
    info.value = await api.wiki.wikiItem.edit(props.urlPathName);
    if(info.value){
        paras.value = await api.wiki.wikiItem.loadFull(info.value.Id)
        if(paras.value){
            paras.value.forEach(t=>{
                if(t.Type == WikiParaType.Table){
                    //迫使表格编辑器组件消失并重新生成，才能正常保存
                    t.tableData = undefined;
                    window.setTimeout(()=>{
                        t.tableData = JSON.parse(t.Content) as AuTableData
                    },1)
                }
            });
            ready.value = true
            if(!heartbeat){
                heartbeat = new HeartbeatSenderForWholeWiki(api, info.value.Id);
                heartbeat.start();
            }
        }
    }
}
function registerActiveSave(fn:()=>Promise<boolean>, p:WikiParaDisplayEdit){
    p.save = fn;
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
    const saveSuccess = await saveAll();
    if(!saveSuccess)
        return
    if(idx == 0 || !info.value?.Id){return}
    const ids = paras.value?.map(x=>x.ParaId);
    if(!ids){return;}
    const targetId = ids.splice(idx,1)[0];
    ids.splice(idx-1,0, targetId);
    const s = await api.wiki.wikiItem.setParaOrders({id: info.value.Id, orderedParaIds:ids})
    if(s){
        await load();
    }
}
async function removePara(idx:number) {
    const saveSuccess = await saveAll();
    if(!saveSuccess)
        return
    const target = paras.value?.at(idx);
    if(target){
        if(!window.confirm(`确定要删除段落<${displayTitle(target)}>?`)){
            return
        }
        const s = await api.wiki.wikiItem.removePara({
            id:info.value?.Id||0,
            paraId:target.ParaId
        })
        if(s)
            await load();
    }
}
async function insertPara(p:WikiParaDisplay, position:"before"|"after", type:WikiParaType) {
    const saveSuccess = await saveAll();
    if(!saveSuccess)
        return
    const afterOrder = position == "after" ? p.Order : p.Order-1;
    const data = await api.wiki.wikiItem.insertParaAndGetId({
        id:info.value?.Id || 0,
        afterOrder: afterOrder,
        type
    })
    if(data){
        selectedPara.value = data.newlyCreatedParaId;
        if(type == WikiParaType.Text){
            const res = await api.textSection.textSection.createForPara({paraId: data.newlyCreatedParaId});
            if(res){
                await load();
            }
        }else if(type == WikiParaType.Table){
            const res = await api.table.freeTable.createForPara(data.newlyCreatedParaId);
            if(res){
                await load();
            }
        }
        else if(type == WikiParaType.File){
            await load();
        }
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
function displayTitle(p:WikiParaDisplayEdit){
    return removeDefaultFoldMark(p.NameOverride) || p.Title || defaultParaTitle(p.Type);
}
function removeDefaultFoldMark(t:string|null){
    if(!t)
        return null;
    if(t.startsWith(wikiParaDefaultFoldMark)){
        return t.slice(1)
    }
    return t;
}

function refreshUnsaveStatus(){
    console.log(paras.value?.map(x=>x.changed));
    const anyChanged = paras.value?.some(p=>p.changed) || false;
    if(anyChanged){
        preventLeaving();
    }else{
        releasePreventLeaving();
    }
}
function paraChanged(p:WikiParaDisplayEdit){
    p.changed = true;
    preventLeaving();
}
async function tableSave(data:AuTableData, cb:(s:boolean, msg:string)=>void, p:WikiParaDisplayEdit){
    const resp = await api.table.freeTable.saveContent(p.UnderlyingId, JSON.stringify(data));
    if(resp.success){
        cb(true,"成功保存");
        p.changed = false;
        refreshUnsaveStatus();
    }else{
        cb(false,resp.errmsg);
    }
}

async function saveAll():Promise<boolean> {
    const changedParas = paras.value?.filter(x=>x.changed) || [];
    if(changedParas.length == 0){
        return true;
    }
    let successCount = 0;
    for(const p of changedParas){
        if(p.Type == WikiParaType.Text){
            const s = await api.textSection.textSection.editExe({
                Id: p.UnderlyingId,
                Title: null,
                Content: p.Content
            })
            if(s){
                p.changed = false;
                successCount += 1;
            }
        }else if(p.Type == WikiParaType.Table){
            let s = false;
            if(p.save){
                s = await p.save();
            }
            if(s){
                p.changed = false;
                successCount += 1;
            }
        }
    };
    refreshUnsaveStatus();
    if(successCount)
        pop.value.show(`成功保存${successCount}个段落的更改`, "success")
    const changedParasAfter = paras.value?.filter(x=>x.changed) || [];
    return changedParasAfter.length == 0
}
function leave(){
    router.back();
}

let imgClickJump:ImageClickJump;
const focusImg = ref<string>();
const parasDiv = ref<HTMLDivElement>();
const setTopbar = injectSetTopbar();
const pop = injectPop();
const { preventLeaving, releasePreventLeaving, preventingLeaving , showUnsavedWarning } = usePreventLeavingUnsaved()
const ctrlZ = new ShortcutListener(()=>{
    pop.value.show("撤销功能暂未做", "failed")
}, "z", true, false);
const ctrlShiftZ = new ShortcutListener(()=>{}, "z", true, true);
const ctrlS = new ShortcutListener(saveAll, "s", true, false);
ctrlZ.startListen();
ctrlShiftZ.startListen();
ctrlS.startListen();    
let heartbeat:HeartbeatSenderForWholeWiki|undefined = undefined;
onMounted(async()=>{
    setTopbar(false);
    await load();
    await nextTick();
    imgClickJump = new ImageClickJump(src=>{
        focusImg.value = src;
    }, true);
    imgClickJump.listen(parasDiv.value);
})
onUnmounted(()=>{
    ctrlZ?.dispose();
    ctrlShiftZ?.dispose();
    ctrlS?.dispose();
    imgClickJump.dispose();
    heartbeat?.stop();
    setTopbar(true)
})
</script>

<template>
<div v-if="info && ready">
    <div class="topbar" @click="selectedPara = 0">
        <div class="wikiTitle">{{ info.Title }}</div>
        <div class="btns">
            <button class="off" @click="paraMode = !paraMode" :class="{paraMode}">增删</button>
            <button v-if="preventingLeaving" @click="saveAll">保存</button>
            <button v-else @click="leave" class="ok">离开</button>
        </div>
        <div class="preventingLeaving" v-show="preventingLeaving"></div>
    </div>
    <div v-if="paras" class="paras" ref="parasDiv">
        <div v-for="p,idx in paras" :key="p.ParaId" class="para"
            :class="{selected:p.ParaId == selectedPara}" @click="selectedPara = p.ParaId">
            <div class="paraTop">
                <h2 :class="{changed:p.changed}">
                    {{ displayTitle(p) }}
                    <span class="defaultFold">{{ p.NameOverride?.startsWith('^') ? '(默认折起)':''}}</span>
                </h2>
                <div class="ops">
                    <button v-if="p.Type == WikiParaType.File" class="lite" @click="startEditingFilePara(p)">编辑</button>
                    <button v-show="!paraMode" class="lite" @click="startEditingInfo(p)">设置</button>
                    <button v-show="paraMode" class="lite rmPara" @click="removePara(idx)">移除</button>
                </div>
            </div>
            <div v-if="p.Type == WikiParaType.Text" class="text">
                {{ p.Content }}
                <div v-if="!p.Content || p.Content.endsWith('\n')" style="height: 20px;">.</div>
                <textarea v-model="p.Content" @input="paraChanged(p)" spellcheck="false"></textarea>
            </div>
            <div v-else-if="p.Type == WikiParaType.Table" class="table" :style="{height:p.height}">
                <AuTableEditor v-if="p.tableData"
                    :table-data="p.tableData" 
                    :no-shortcut="true"
                    @changed="()=>paraChanged(p)"
                    @save="(val, cb)=>tableSave(val, cb, p)"
                    :size-change-callback="(s)=>{p.height = s.height}"
                    :external-save-callback-provide="(s)=>registerActiveSave(s,p)"></AuTableEditor>
            </div>
            <div v-else-if="p.Type == WikiParaType.File" class="file">
                <FileParaListItem :w="p" :no-title="true"></FileParaListItem>
            </div>
            <div v-if="paraMode && idx==0" class="paraInsert top">
                <button class="minor" @click="insertPara(p, 'before', WikiParaType.Text)">+文本</button>
                <button class="minor" @click="insertPara(p, 'before', WikiParaType.Table)">+表格</button>
                <button class="minor" @click="insertPara(p, 'before', WikiParaType.File)">+文件</button>
            </div>
            <div v-if="paraMode" class="paraInsert bottom">
                <button v-if="idx<paras.length-1" class="minor" @click="moveUp(idx+1)">⇅交换</button>
                <button class="minor" @click="insertPara(p, 'after', WikiParaType.Text)">+文本</button>
                <button class="minor" @click="insertPara(p, 'after', WikiParaType.Table)">+表格</button>
                <button class="minor" @click="insertPara(p, 'after', WikiParaType.File)">+文件</button>
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
<UnsavedLeavingWarning v-if="showUnsavedWarning" :release="releasePreventLeaving" @ok="showUnsavedWarning=false"></UnsavedLeavingWarning>
</template>

<style scoped lang="scss">
@import '@/styles/globalValues';

.topbar{
    position: fixed;
    top:0px;left: 0px;right: 0px;
    height: $topbar-height;
    box-shadow: 0px 0px 5px 0px black;
    z-index: 1000;
    display: flex;
    padding: 0px 10px 0px 10px;
    align-items: center;
    justify-content: space-between;
    background-color: white;
    .wikiTitle{
        font-size: 18px;
        max-width: 180px;
        overflow: hidden;
        white-space: nowrap;
        text-overflow: ellipsis;
    }
    .preventingLeaving{
        position: fixed;
        right: 8px;
        top: 5px;
        width: 10px;
        height: 10px;
        background-color: red;
        border-radius: 50%;
    }
    .btns{
        button.off{
            background-color: #999;
            &:hover{
                background-color: #666;
            }
            transition: 0.2s;
        }
        button.paraMode{
            background-color: cadetblue;
            &:hover{
                background-color: rgb(56, 137, 140)
            }
        }
    }
}
.file{
    height: 110px;
    position: relative;
}
.table{
    position: relative;
    height: 40vh;
    transition: 0s;
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
    gap: 10px;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 4px;
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
        color: white;
        background-color: cornflowerblue;
        padding: 3px;
        border-radius: 3px;
        word-break: break-all;
    }
    h2.changed{
        background-color: palevioletred;
    }
}
.paras{
    display: flex;
    flex-direction: column;
    gap:15px;
    padding-top: 30px;
    padding-bottom: 100px;
    min-width: 270px;
}
.para{
    background-color: #e8e8e8;
    padding: 5px;
    position: relative;
    button.rmPara{
        font-weight: bold;
    }
    .paraInsert{
        display: flex;
        justify-content: center;
        align-items: center;
        position: absolute;
        left: 0px;
        right: 0px;
        height: 24px;
        z-index: 10;
        button{
            height: unset;
            padding: 2px;
            border: none;
            box-shadow: 0px 0px 2px 0px black;
        }
    }
    .paraInsert.top{
        top: -20px;
    }
    .paraInsert.after{
        bottom: -20px;
    }
}
.para.selected{
    background-color: #ccc;
}
*{
    transition: 0.5s;
}
</style>