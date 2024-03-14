<script setup lang="ts">
import { computed, onMounted, onUnmounted, ref } from 'vue';
import { Api } from '../../utils/api';
import { injectApi, injectSetTopbar } from '../../provides';
import { WikiTemplate } from '../../models/wikiParsing/wikiTemplate';
import SwitchingTabs from '../../components/SwitchingTabs.vue';
import { updateScript } from '../../utils/wikiView/dynamicScriptUpdate';

const props = defineProps<{
    id:string
}>();
const idNum = parseInt(props.id);

const data = ref<WikiTemplate>();
async function Load(){
    const resp = await api.wikiParsing.wikiTemplate.edit(idNum);
    if(resp){
        data.value = resp;
    }
}
async function save() {
    if(data.value){
        await api.wikiParsing.wikiTemplate.editExe(data.value)
    }
}

const previewOn = ref<boolean>(true);
const stylesContent = ref<string>();
const styles = computed(()=>`<style>${stylesContent.value}</style>`)
const previewContent = ref<string>();
const preScriptsDiv = ref<HTMLDivElement>();
const postScriptsDiv = ref<HTMLDivElement>();
const writeAreaLineHeight = 25;

async function refreshPreview() {
    if(!previewOn.value || !data.value){
        return;
    }
    const res = await api.wikiParsing.wikiTemplate.preview(data.value);
    if(res && preScriptsDiv.value){
        stylesContent.value = res.Styles;
        updateScript(preScriptsDiv.value, res.PreScripts);
        previewContent.value = "";
        //清除旧的事件侦听器
        setTimeout(()=>{
            previewContent.value = res.HtmlSource;
        },5);
        setTimeout(()=>
        {
            if(postScriptsDiv.value){
                updateScript(postScriptsDiv.value, res.PostScripts)
            }
        },10);
    }
}

let inputCounter = 0;
const saveThrs = 50;
let timer = 0;
const refreshThrs = 1000;
async function contentInput(){
    if(!previewContent.value){
        previewContent.value="加载中..."
    }
    window.clearTimeout(timer);
    timer = window.setTimeout(async()=>{
        var refreshProm = refreshPreview()
        await refreshProm;
    },refreshThrs);
    inputCounter+=1;
    if(inputCounter>saveThrs){
        save();
        inputCounter=0;
    }
}

let api:Api;
onMounted(async()=>{
    api = injectApi();
    injectSetTopbar()(false);
    await Load();
    await contentInput();
})
onUnmounted(()=>{
    injectSetTopbar()(true);
})
</script>

<template>
    <div class="topbar">
    <div>
        <button @click="previewOn = !previewOn" :class="{off:!previewOn}">
            预览
        </button>
        <input v-if="data" v-model="data.Name" placeholder="请输入模板名称"/>
    </div>
    <div>
        <button @click="save">
            保存
        </button>
    </div>
</div>
<div v-if="data" class="background">
    <div class="invisible" v-html="styles"></div>
    <div ref="preScriptsDiv" class="invisible"></div>
    <div v-show="previewOn"
        ref="previewArea" class="preview wikiView" v-html="previewContent">
    </div>
    <div ref="postScriptsDiv" class="invisible"></div>

    <SwitchingTabs class="write" :class="{writeNoPreview:!previewOn}" 
        :texts="['html','js-A','js-B','css','demo']">
        <textarea v-model="data.Source"
            placeholder="请输入内容" @input="contentInput"
            :style="{lineHeight:writeAreaLineHeight+'px'}" spellcheck="false">
        </textarea>
        <textarea v-model="data.PostScripts"
            placeholder="请输入内容" @input="contentInput"
            :style="{lineHeight:writeAreaLineHeight+'px'}" spellcheck="false">
        </textarea>
        <textarea v-model="data.PreScripts"
            placeholder="请输入内容" @input="contentInput"
            :style="{lineHeight:writeAreaLineHeight+'px'}" spellcheck="false">
        </textarea>
        <textarea v-model="data.Styles"
            placeholder="请输入内容" @input="contentInput"
            :style="{lineHeight:writeAreaLineHeight+'px'}" spellcheck="false">
        </textarea>
        <textarea v-model="data.Demo"
            placeholder="请输入内容" @input="contentInput"
            :style="{lineHeight:writeAreaLineHeight+'px'}" spellcheck="false">
        </textarea>
    </SwitchingTabs>
</div>
</template>

<style scoped>
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
        margin: 10px;
    }
    .writeNoPreview{
        width: calc(100vw - 20px);
    }
    .write textarea{
        width: calc(50vw - 40px);
        height: calc(100vh - 150px);
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
    .writeNoPreview textarea{
        width: calc(100vw - 40px);
    }
    @media screen and (max-width:800px) {
        .preview{
            width: calc(100vw - 20px);
            height: calc(50vh - 20px - 25px);
        }
        .write{
            width: calc(100vw - 20px);
            height: calc(60vh - 20px);
        }
        .write textarea{
            width: calc(100vw - 40px);
            height: calc(60vh - 110px);
        }
        .writeNoPreview{
            height: 100%;
        }
        .writeNoPreview textarea{
            height: calc(100vh - 145px);
        }
        .background{
            flex-direction: column;
        }
    }
    .topbar{
        position: fixed;
        top:0px;
        left:0px;
        width: 100vw;
        height: 50px;
        background-color: #ccc;

        display: flex;
        align-items: center;
        justify-content: space-between;
    }
    .topbar > div{
        display: flex;
        padding: 0px 10px 0px 10px;
    }
    .invisible{
        display: none;
    }
</style>