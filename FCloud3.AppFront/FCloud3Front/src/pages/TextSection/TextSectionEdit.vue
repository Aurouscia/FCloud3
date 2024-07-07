<script setup lang="ts">
import {ref, Ref, onMounted, computed, onUnmounted, nextTick} from 'vue'
import Pop from '@/components/Pop.vue';
import WikiTitleContain from '@/components/Wiki/WikiTitleContain.vue';
import Loading from '@/components/Loading.vue';
import { TextSection } from '@/models/textSection/textSection'
import { Api } from '@/utils/com/api';
import { updateScript } from '@/utils/wikiView/dynamicScriptUpdate';
import { LineAndHash,split } from '@/utils/wikiSource/textSecSplitLine';
import { WikiSourceHighlighter } from '@/utils/wikiSource/wikiSourceHighlight';
import md5 from 'md5';
import { SetTopbarFunc, injectApi, injectPop, injectSetTopbar } from '@/provides';
import { useFootNoteJump } from '@/utils/wikiView/footNoteJump';
import { WikiTitleContainType } from '@/models/wiki/wikiTitleContain';
import SideBar from '@/components/SideBar.vue';
import { usePreventLeavingUnsaved } from '@/utils/eventListeners/preventLeavingUnsaved';
import UnsavedLeavingWarning from '@/components/UnsavedLeavingWarning.vue';
import { ShortcutListener } from '@aurouscia/keyboard-shortcut';
import { sleep } from '@/utils/sleep';
import { HeartbeatObjType, HeartbeatSender } from '@/models/etc/heartbeat';
import { recoverTitle, setTitleTo } from '@/utils/titleSetter';
import { useRouter } from 'vue-router';
import { TextSectionLocalConfig, readLocalConfig, saveLocalConfig, textSectionConfigDefault } from '@/utils/localConfig';

const locatorHash:(str:string)=>string = (str)=>{
    return md5(str.trim())
}

const props = defineProps<{id:string}>()
var textSecId:number = Number(props.id);
const previewOn = ref<boolean>(false);
var api:Api
var pop:Ref<InstanceType<typeof Pop>>;
const router = useRouter();

const preScriptsDiv = ref<HTMLDivElement>();
const postScriptsDiv = ref<HTMLDivElement>();
const stylesContent = ref<string>();
const styles = computed(()=>`<style>${stylesContent.value}</style>`)

const wikiSourceHighlighter = new WikiSourceHighlighter()

const loadComplete = ref<boolean>(false);
const data = ref<TextSection>({
    Id:textSecId,
    Content:"",
    Title:""
});
const previewContent = ref<string>();
const lines:Array<LineAndHash>=[];
const writeArea = ref<HTMLDivElement>();
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
    if(!writeArea.value){return;}
    data.value.Content = textNode()?.textContent || "";
    //有时候会莫名其妙出现多余的子节点，检查一下并删掉即可
    const childs = writeArea.value.childNodes
    const needRemove:ChildNode[] = []
    let removed = false;
    for(let i = 0; i<childs.length;i++){
        if(i>0){
            needRemove.push(childs[i])
            removed = true;
        }
    }
    const converted = needRemove.filter(x=>x.textContent && x.textContent.length>0).map(x=>x.textContent).join('\n');
    needRemove.forEach(n=>n.remove());
    if(removed){
        textNode().textContent += (converted+"\n")
    }
    
    if(!previewContent.value){
        previewContent.value="加载中..."
    }
    if(data.value.Content === initialContent){
        releasePreventLeaving();
    }
    else{
        preventLeaving();
    }
    window.clearTimeout(timer);
    timer = window.setTimeout(async()=>{
        var refreshProm = refreshPreview()
        calculateLines();
        await refreshProm;
    },refreshThrs);

    if(writeArea.value){
        if(writeArea.value.scrollHeight - writeArea.value.scrollTop - 30 < writeArea.value.offsetHeight){
            writeArea.value.scrollTop = 1000000
        }
    }

    // inputCounter+=1;
    // if(inputCounter>saveThrs){
    //     replaceContent();
    //     inputCounter=0;
    // }
    wikiSourceHighlighter.run(textNode())
}
async function refreshPreview() {
    if(!previewOn.value){
        return;
    }
    const res = await api.textSection.textSection.preview(textSecId,data.value.Content||"");
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
    const val = data.value.Title || "";
    const send:TextSection = {
        Id : data.value.Id,
        Title : val,
        Content : null//不更新正文，只更新标题
    }
    api.textSection.textSection.editExe(send)
}
async function replaceContent() {
    if(!loadComplete.value){
        return;
    }
    const resp = await api.textSection.textSection.editExe(data.value);
    if(resp){
        releasePreventLeaving()
        initialContent = data.value.Content || "";
    }
}
function leave(){
    heartbeatSender?.stop();
    api.etc.heartbeat.release({objType: HeartbeatObjType.TextSection, objId: textSecId});
    router.back();
}

let initialContent:string = "";
async function init(){
    const resp = await api.textSection.textSection.edit(textSecId);
    if(resp){
        data.value = resp;
        initialContent = data.value.Content || "";
        loadComplete.value = true;
        await nextTick();
        if(!writeArea.value){return}
        textNode().textContent = data.value.Content || "\n";
        writeArea.value.addEventListener("keydown", enterKeyHandler);
        await contentInput();
        releasePreventLeaving()

        if(!heartbeatSender){
            heartbeatSender = new HeartbeatSender(api, HeartbeatObjType.TextSection, textSecId);
            heartbeatSender.start();
        }
    }
}

function textNode(){return writeArea.value?.childNodes[0] as Text}
async function enterKeyHandler(e: KeyboardEvent) {
    //contenteditable的div中按下回车键时，如果光标在最后，会自动在最后插入一个<br>，导致保存后空行莫名其妙变多，这里要阻止这个行为
    //自定义实现一个换行功能
    if (e.key == "Enter") {
        e.preventDefault()
        let sel = window.getSelection();
        const node = textNode();
        if (writeArea.value && sel && sel.isCollapsed && data.value.Content && node && sel.focusNode == node) {
            const range = sel.getRangeAt(0);
            const offset = range.startOffset;
            const before = data.value.Content.substring(0, offset);
            let after = data.value.Content.substring(offset);
            if (!after) {
                after = "\n";
            }
            const newContent = before + "\n" + after;
            data.value.Content = newContent;
            node.textContent = newContent;
            await sleep(5);
            const newRange = document.createRange()
            newRange.setStart(node, offset + 1);
            newRange.setEnd(node, offset + 1);
            window.getSelection()?.removeAllRanges();
            window.getSelection()?.addRange(newRange);
            contentInput();
        }
    }
}

let setTopbar:SetTopbarFunc|undefined;
const { footNoteJumpCallBack, listenFootNoteJump, disposeFootNoteJump } = useFootNoteJump();
let saveShortcut: ShortcutListener|undefined;
let heartbeatSender:HeartbeatSender|undefined;
const localConfig = ref<TextSectionLocalConfig>(textSectionConfigDefault);
onMounted(async()=>{
    localConfig.value = (readLocalConfig("textSection") || textSectionConfigDefault) as TextSectionLocalConfig;
    setTitleTo('文本段编辑器')
    pop = injectPop();
    api = injectApi();
    setTopbar = injectSetTopbar();
    setTopbar(false);
    footNoteJumpCallBack.value = (top)=>{
        previewArea.value?.scrollTo({top: top, behavior: 'smooth'})
    };
    listenFootNoteJump();
    saveShortcut = new ShortcutListener(replaceContent, "s", true);
    saveShortcut.startListen();
    await init();
    await nextTick();
})
onUnmounted(()=>{
    recoverTitle()
    if(setTopbar)
        setTopbar(true);
    disposeFootNoteJump();
    releasePreventLeaving();
    saveShortcut?.dispose();
    heartbeatSender?.stop();
})
function saveLocalConfigClick(){
    saveLocalConfig(localConfig.value);
    pop.value.show("保存成功","success")
}

async function leftToRight(e:MouseEvent){
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

    //将选中部分及以后的部分删掉，让编辑框缩短得到那时的高度，再恢复，可以使编辑框滚动到指定位置
    textNode().textContent = data.value.Content?.substring(0, line.indexStart) || "";
    writeArea.value.style.color = "transparent";
    await sleep(30)
    const height = writeArea.value.offsetHeight;
    const scrollHeight = writeArea.value.scrollHeight;
    let top = 0;
    if(scrollHeight > height){
        top = scrollHeight - 30
    }
    textNode().textContent = data.value.Content || "";
    await sleep(30)
    writeArea.value.style.color = "";
    writeArea.value.scrollTo({top: top, behavior:'instant'})

    const range = document.createRange()
    range.setStart(textNode(), line.indexStart)
    range.setEnd(textNode(), line.indextEnd)
    window.getSelection()?.removeAllRanges()
    window.getSelection()?.addRange(range)
    wikiSourceHighlighter.run(textNode())
}
function rightToLeft(e:MouseEvent){
    const write = writeArea.value;
    if(!write || e.target!=writeArea.value){
        return;
    }
    const sele = window.getSelection()?.getRangeAt(0).startOffset || -1;
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
    lastRightToLeftHashFadeTimer = window.setTimeout(()=>{lastRightToLeftHash = ""}, 10000)
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
const localConfigSidebar = ref<InstanceType<typeof SideBar>>()
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
        <div class="ops">
            <button class="minor" @click="localConfigSidebar?.extend">
                设置
            </button>
            <button class="minor" @click="wikiTitleContainSidebar?.extend">
                链接
            </button>
        </div>
        <button v-if="preventingLeaving" @click="replaceContent">
            保存
        </button>
        <button v-else @click="leave" class="ok">
            退出
        </button>
    </div>
    <div class="preventingLeaving" v-show="preventingLeaving"></div>
</div>
<SideBar ref="wikiTitleContainSidebar">
    <WikiTitleContain :type="WikiTitleContainType.TextSection" :object-id="textSecId" :get-content="()=>data.Content" @changed="refreshPreview">
    </WikiTitleContain>
</SideBar>
<SideBar ref="localConfigSidebar">
    <h1>编辑器设置</h1>
    <table>
        <tr>
            <td>黑色背景</td>
            <td><input type="checkbox" v-model="localConfig.blackBg"/></td>
        </tr>
        <tr>
            <td>字体大小</td>
            <td><input type="range" min="14" max="20" step="1" v-model="localConfig.fontSize"/></td>
        </tr>
        <tr>
            <td class="noBg" colspan="2"><button @click="saveLocalConfigClick">保存</button><br/></td>
        </tr>
    </table>
    <div style="font-size: 14px; color: #aaa">仅会保存在本浏览器内<br/>后续更新可能需重新设置</div>
</SideBar>
<div v-if="loadComplete" class="background">
    <div class="invisible" v-html="styles"></div>
    <div ref="preScriptsDiv" class="invisible"></div>
    <div @click="leftToRight" v-show="previewOn"
        ref="previewArea" class="preview wikiView" v-html="previewContent">
    </div>
    <div ref="postScriptsDiv" class="invisible"></div>

    <div contenteditable="plaintext-only"
        ref="writeArea" placeholder="请输入内容"
        @input="contentInput" @click="rightToLeft"
        class="write" 
        :class="{
            writeNoPreview:!previewOn,
            writeWhiteBg:!localConfig.blackBg}"
        :style="{
            lineHeight:writeAreaLineHeight+'px',
            fontSize:localConfig.fontSize+'px',
            'line-height':localConfig.fontSize/16*25+'px'}" 
        spellcheck="false">&nbsp;</div>
</div>
<Loading v-else></Loading>
<UnsavedLeavingWarning v-if="showUnsavedWarning" :release="releasePreventLeaving" @ok="showUnsavedWarning=false"></UnsavedLeavingWarning>
</template>

<style scoped lang="scss">
    @import "@/utils/wikiSource/wikiSourceHighlight";
    .preventingLeaving{
        position: fixed;
        right: 5px;
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
        white-space: pre-wrap;
    }
    .writeNoPreview{
        width: calc(100vw - 20px);
    }
    .writeWhiteBg{
        background-color: white;
        color: #222;
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
        padding: 0px 5px 0px 5px;

        display: flex;
        align-items: center;
        justify-content: space-between;
        .ops{
            display: flex;
            flex-wrap: nowrap;
            overflow-x: auto;
        }
    }
    .paraTitle{
        width: 130px;
    }
    .topbar > div{
        display: flex;
        align-items: center;
    }
    .topbar button{
        white-space: nowrap;
        margin: 1px;
    }
</style>

<style>
.preview *{
    transition: 0.3s;
}
</style>