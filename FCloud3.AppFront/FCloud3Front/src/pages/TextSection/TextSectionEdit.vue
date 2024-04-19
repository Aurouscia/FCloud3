<script setup lang="ts">
import {ref, Ref, onMounted, computed, onUnmounted, nextTick} from 'vue'
import Pop from '../../components/Pop.vue';
import WikiTitleContain from '../../components/Wiki/WikiTitleContain.vue';
import { TextSection } from '../../models/textSection/textSection'
import { Api } from '../../utils/api';
import { updateScript } from '../../utils/wikiView/dynamicScriptUpdate';
import { LineAndHash,split } from '../../utils/wikiView/textSecSplitLine';
import { md5 } from 'js-md5'
import { SetTopbarFunc, injectApi, injectPop, injectSetTopbar } from '../../provides';
import { clone } from 'lodash';
import { useFootNoteJump } from '../../utils/wikiView/footNoteJump';
import { WikiTitleContainType } from '../../models/wiki/wikiTitleContain';
import SideBar from '../../components/SideBar.vue';
import { usePreventLeavingUnsaved } from '../../utils/preventLeavingUnsaved';
import UnsavedLeavingWarning from '../../components/UnsavedLeavingWarning.vue';
import { ShortcutListener } from '@aurouscia/keyboard-shortcut';

const locatorHash:(str:string)=>string = (str)=>{
    return md5(str)
}

const props = defineProps<{id:string}>()
var textSecId:number = Number(props.id);
const previewOn = ref<boolean>(true);
var api:Api
var pop:Ref<InstanceType<typeof Pop>>;

const preScriptsDiv = ref<HTMLDivElement>();
const postScriptsDiv = ref<HTMLDivElement>();
const stylesContent = ref<string>();
const styles = computed(()=>`<style>${stylesContent.value}</style>`)

const loadComplete = ref<boolean>(false);
const data = ref<TextSection>({
    Id:textSecId,
    Content:"",
    Title:""
});
const previewContent = ref<string>();
const lines:Array<LineAndHash>=[];
const writeArea = ref<HTMLTextAreaElement>();
const previewArea = ref<HTMLDivElement>();
const writeAreaLineHeight = 25;

async function togglePreview(){
    if(previewOn.value){
        previewOn.value=false;
    }else{
        previewOn.value=true;
        contentInput();
    }
}
let timer:number=0;
const refreshThrs=750;//这么多毫秒后还没有新的输入，则发送preview请求
//const saveThrs=50;//输入这么多次后自动保存
//let inputCounter:number = 0;
async function contentInput(){
    if(!previewContent.value){
        previewContent.value="加载中..."
    }
    preventLeaving();
    window.clearTimeout(timer);
    timer = window.setTimeout(async()=>{
        var refreshProm = refreshPreview()
        calculateLines();
        await refreshProm;
    },refreshThrs);

    // inputCounter+=1;
    // if(inputCounter>saveThrs){
    //     replaceContent();
    //     inputCounter=0;
    // }
}
async function refreshPreview() {
    if(!previewOn.value){
        return;
    }
    const res = await api.textSection.preview(textSecId,data.value.Content||"");
    if(res && preScriptsDiv.value){
        stylesContent.value = res.Styles;
        updateScript(preScriptsDiv.value, res.PreScripts);
        previewContent.value = res.HtmlSource;
        setTimeout(()=>
        {
            if(postScriptsDiv.value){
                updateScript(postScriptsDiv.value, res.PostScripts, "module")
            }
        },10);
    }
}
function calculateLines(){
    if(!writeArea.value){
        return;
    }
    const content = data.value.Content||"";
    const res = split(content,false,false);
    var searchStart = 0
    var same = true;
    for(var i=0;i<res.length;i++){
        const newLine = res[i];
        const index = content.indexOf(newLine,searchStart);
        const indexEnd = index+newLine.length;
        searchStart = indexEnd;
        if(!lines[i] || newLine!=lines[i].text){
            same = false;
        }
        if(!same){
            lines[i]={
                text:newLine,
                hash:locatorHash(res[i]),
                indexStart:index,
                indextEnd:indexEnd
            }
        }
    }
}

async function replaceTitle() {
    const val = data.value.Title;
    if(!val || val==""){
        pop.value.show("段落标题不可为空","failed");
        return;
    }
    const send = clone(data.value);
    send.Content = null;//不更新正文，只更新标题
    api.textSection.editExe(send)
}
async function replaceContent() {
    const resp = await api.textSection.editExe(data.value);
    if(resp){
        releasePreventLeaving()
    }
}

async function init(){
    const resp = await api.textSection.edit(textSecId);
    if(resp){
        data.value = resp;
        loadComplete.value = true;
        await contentInput();
        releasePreventLeaving()
    }
}

let setTopbar:SetTopbarFunc|undefined;
const { footNoteJumpCallBack, listenFootNoteJump, disposeFootNoteJump } = useFootNoteJump();
let saveShortcut: ShortcutListener|undefined;
onMounted(async()=>{
    pop = injectPop();
    api = injectApi();
    setTopbar = injectSetTopbar();
    setTopbar(false);
    footNoteJumpCallBack.value = (top)=>{
        console.log("滚到",top)
        previewArea.value?.scrollTo({top: top, behavior: 'smooth'})
    };
    listenFootNoteJump();
    saveShortcut = new ShortcutListener(replaceContent, "s", true);
    saveShortcut.startListen();
    await init();
    await nextTick();
})
onUnmounted(()=>{
    if(setTopbar)
        setTopbar(true);
    disposeFootNoteJump();
    releasePreventLeaving();
    saveShortcut?.dispose();
})

function leftToRight(e:MouseEvent){
    if(!writeArea.value){return;}
    const target = e.target as HTMLElement;
    var lookingAt:HTMLElement|null = target;
    var attr:Attr|null = null;
    while(!attr){
        attr = lookingAt.attributes.getNamedItem("loc");
        if(!attr){
            lookingAt = lookingAt.parentElement
        }
        if(!lookingAt || lookingAt.classList.contains('preview')){
            break;
        }
    }
    if(!attr){return;}
    const line = lines.find(x=>x.hash==attr?.value)
    if(!line){return;}
    writeArea.value.setSelectionRange(line.indexStart,line.indexStart);
    writeArea.value.focus();
    writeArea.value.setSelectionRange(line.indexStart,line.indextEnd);
}
function rightToLeft(e:MouseEvent){
    const write = writeArea.value;
    if(!write || e.target!=writeArea.value){
        return;
    }
    const sele = write.selectionStart;
    const targetLine = lines.find(line=>sele>=line.indexStart&&sele<=line.indextEnd);
    if(!targetLine){
        return;
    }
    const targetHash = targetLine.hash;
    clearTimeout(lastRightToLeftHashFadeTimer);
    if(targetHash==lastRightToLeftHash){
        return;
    }
    lastRightToLeftHash = targetHash;
    lastRightToLeftHashFadeTimer = setTimeout(()=>{lastRightToLeftHash = ""}, 10000)
    const target = document.querySelector(`[loc="${targetHash}"]`) as HTMLElement;
    if(!target){return;}
    if(previewArea.value){
        previewArea.value.scrollTo({top: target.offsetTop, behavior: 'smooth'})
    }
    target.style.backgroundColor="yellow";
    window.setTimeout(()=>{
        target.style.backgroundColor="";
    },1000)
}
let lastRightToLeftHash:string = "";
let lastRightToLeftHashFadeTimer = 0;

const { preventLeaving, releasePreventLeaving, preventingLeaving, showUnsavedWarning } = usePreventLeavingUnsaved();
const wikiTitleContainSidebar = ref<InstanceType<typeof SideBar>>()
</script>

<template>
<div class="topbar">
    <div>
        <button @click="togglePreview" :class="{off:!previewOn}">
            预览
        </button>
        <input v-model="data.Title" placeholder="请输入段落标题" @blur="replaceTitle" class="paraTitle"/>
    </div>
    <div>
        <div>
            <button class="minor" @click="wikiTitleContainSidebar?.extend">
                链接
            </button>
        </div>
        <button @click="replaceContent">
            保存
        </button>
    </div>
    <div class="preventingLeaving" v-show="preventingLeaving"></div>
</div>
<SideBar ref="wikiTitleContainSidebar">
    <WikiTitleContain :type="WikiTitleContainType.TextSection" :object-id="textSecId" :get-content="()=>data.Content" @changed="refreshPreview">
    </WikiTitleContain>
</SideBar>
<div v-if="loadComplete" class="background">
    <div class="invisible" v-html="styles"></div>
    <div ref="preScriptsDiv" class="invisible"></div>
    <div @click="leftToRight" v-show="previewOn"
        ref="previewArea" class="preview wikiView" v-html="previewContent">
    </div>
    <div ref="postScriptsDiv" class="invisible"></div>

    <textarea v-model="data.Content"
        ref="writeArea" placeholder="请输入内容"
        @input="contentInput" @click="rightToLeft"
        class="write" :class="{writeNoPreview:!previewOn}"
        :style="{lineHeight:writeAreaLineHeight+'px'}" spellcheck="false">
    </textarea>
</div>
<UnsavedLeavingWarning v-if="showUnsavedWarning" :release="releasePreventLeaving" @ok="showUnsavedWarning=false"></UnsavedLeavingWarning>
</template>

<style scoped>
    .preventingLeaving{
        position: fixed;
        right: 10px;
        top: 7px;
        width: 10px;
        height: 10px;
        background-color: red;
        border-radius: 50%;
    }
    .background{
        position: fixed;
        width: 100vw;
        top: 50px;
        left: 0px;
        bottom: 0px;

        display: flex;
        flex-direction:row;
        justify-content:space-between;
        align-items: center;
        overflow: hidden;
    }
    .preview{
        width: calc(50vw - 20px);
        height: calc(100% - 20px);
        font-size: 16px;
        overflow-y: scroll;
        word-break: break-all;
        padding: 10px;
    }
    .write{
        width: calc(50vw - 20px);
        height: calc(100% - 20px);
        resize:none;
        border: none !important;
        font-size: 16px;
        font-family:unset;
        overflow-y: scroll;
        word-break: break-all;
        padding: 10px;
        line-height: 25px;
        background-color: #222;
        color:white;
    }
    .writeNoPreview{
        width: calc(100vw - 20px);
    }
    @media screen and (max-width:800px) {
        .preview{
            width: calc(100vw - 20px);
            height: calc(50vh - 20px - 25px);
        }
        .write{
            width: calc(100vw - 20px);
            height: calc(50vh - 20px);
        }
        .writeNoPreview{
            height: 100%;
        }
        .background{
            flex-direction: column;
        }
    }
    .topbar{
        position: fixed;
        top:0px;
        left:0px;
        right: 0px;
        height: 50px;
        background-color: #ccc;
        padding: 0px 10px 0px 10px;

        display: flex;
        align-items: center;
        justify-content: space-between;
    }
    .paraTitle{
        width: 160px;
    }
    .topbar > div{
        display: flex;
        align-items: center;
    }
    .topbar button{
        white-space: nowrap;
    }
</style>

<style>
.preview *{
    transition: 0.3s;
}
</style>