<script setup lang="ts">
import {ref, Ref, onMounted, inject, computed} from 'vue'
import Pop from '../../components/Pop.vue';
import {TextSection} from '../../models/textSection/textSection'
import { Api } from '../../utils/api';
import { updateScript } from '../../utils/dynamicScriptUpdate';

const props = defineProps<{id:string}>()
var textSecId:number = Number(props.id);
const previewOn = ref<boolean>(true);
var api:Api
var pop:Ref<InstanceType<typeof Pop>>;

const preScriptsDiv = ref<HTMLDivElement>();
const postScriptsDiv = ref<HTMLDivElement>();
const stylesContent = ref<string>();
const styles = computed(()=>`<style>${stylesContent.value}</style>`)

const data = ref<TextSection>({
    Id:textSecId,
    Content:"",
    Title:""
});
const previewContent = ref<string>();

async function togglePreview(){
    if(previewOn.value){
        previewOn.value=false;
    }else{
        previewOn.value=true;
        contentInput();
    }
}
var timer:number=0;
//var lastInputTime:number=0;
const refreshThrs=750;
async function contentInput(){
    if(!previewContent.value){
        previewContent.value="加载中..."
    }
    window.clearTimeout(timer);
    timer = window.setTimeout(refreshPreview,refreshThrs);
}
async function refreshPreview() {
    if(!previewOn.value){
        return;
    }
    const res = await api.textSection.preview(textSecId,data.value.Content,pop.value.show);
    if(res && preScriptsDiv.value){
        stylesContent.value = res.Styles;
        updateScript(preScriptsDiv.value, res.PreScripts);
        previewContent.value = res.HtmlSource;
        setTimeout(()=>
        {
            if(postScriptsDiv.value){
                updateScript(postScriptsDiv.value, res.PostScripts)
            }
        },10);
    }
}

async function replaceTitle() {
    const val = data.value.Title;
    if(!val || val==""){
        pop.value.show("段落标题不可为空","failed");
        return;
    }
    api.textSection.editExe(data.value,pop.value.show)
}

async function init(){
    const resp = await api.textSection.edit(textSecId,pop.value.show);
    if(resp){
        data.value = resp;}
}
onMounted(async()=>{
    pop = inject('pop') as Ref<InstanceType<typeof Pop>>;
    api = inject('api') as Api;
    const hideTopbar = inject('hideTopbar') as ()=>void;
    hideTopbar();

    await init();
})
</script>

<template>
<div class="topbar">
    <div>
        <button @click="togglePreview">
            {{ previewOn?"预览开":"预览关" }}
        </button>
        <input v-model="data.Title" placeholder="请输入段落标题" @blur="replaceTitle"/>
    </div>
    <div>
        <button @click="()=>{}">
            保存
        </button>
    </div>
</div>
<div v-if="data" class="background">
    <div class="invisible" v-html="styles"></div>
    <div ref="preScriptsDiv" class="invisible"></div>
    <div class="preview" v-show="previewOn" v-html="previewContent">
    </div>
    <div ref="postScriptsDiv" class="invisible"></div>

    <textarea v-model="data.Content" placeholder="请输入内容" @input="contentInput" class="write" :class="{writeNoPreview:!previewOn}">
    </textarea>
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
        flex-wrap: wrap;
        justify-content:space-between;
        align-items: center;
        overflow: hidden;
    }
    .preview{
        width: calc(50vw - 20px);
        height: 100%;
        font-size: 16px;
        overflow-y: scroll;
        word-break: break-all;
        padding: 10px;
    }
    .write{
        width: calc(50vw - 20px);
        height: calc(100% - 16px);
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
    .write:focus{
        border:none !important
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