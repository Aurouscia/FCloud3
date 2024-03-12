<script setup lang="ts">
import { nextTick, onMounted, onUnmounted, ref } from 'vue';
import { injectApi } from '../../provides';
import { Api } from '../../utils/api';
import { WikiParsingResult } from '../../models/wikiParsing/wikiParsingResult';
import { TitleClickFold } from '../../utils/wikiView/titleClickFold';
import { useFootNoteJump } from '../../utils/wikiView/footNoteJump';
import Loading from '../../components/Loading.vue';
import TitleTree from '../../components/Wiki/TitleTree.vue';

const props = defineProps<{
    wikiPathName: string;
}>()

const data = ref<WikiParsingResult>();
async function load(){
    data.value = await api.wikiParsing.wikiParsing.getParsedWiki(props.wikiPathName);
}

const titles = ref<InstanceType<typeof TitleTree>>();
function titleElementId(id:number):string|undefined{
    if(id>0)
        return 't_'+id;
}
function getIdFromElementId(ele:HTMLElement):number{
    return parseInt(ele.id.substring(2));
}
function moveToTitle(titleId:number){
    const title = document.getElementById(titleElementId(titleId)||"??");
    if(title){
        wikiViewArea.value?.scrollTo({top: title.offsetTop, behavior: 'smooth'})
    }
}
let lastScrollTime = 0;
function viewAreaScrollHandler(){
    if(Date.now() - lastScrollTime < 50){return;}
    lastScrollTime = Date.now();
    let currentTitleIdx = titlesInContent.findIndex(t=>
        t.offsetTop > wikiViewArea.value!.scrollTop - 20);
    // if(currentTitleIdx<=0){
    //     currentTitleIdx = 0
    // }else if(currentTitleIdx >= titlesInContent.length-1){
    //     currentTitleIdx = titlesInContent.length-1;
    // }else{
    //     currentTitleIdx--;
    // }
    let currentTitle = titlesInContent[currentTitleIdx];
    titles.value?.highlight(getIdFromElementId(currentTitle));
}

let api:Api;
let clickFold:TitleClickFold;
const {listenFootNoteJump,disposeFootNoteJump,footNoteJumpCallBack} = useFootNoteJump();
const wikiViewArea = ref<HTMLDivElement>();
let titlesInContent:HTMLElement[] 
onMounted(async()=>{
    api = injectApi();
    await load();

    listenFootNoteJump();
    footNoteJumpCallBack.value = (top)=>{
        wikiViewArea.value?.scrollTo({top: top, behavior: 'smooth'})
    };

    await nextTick();
    clickFold = new TitleClickFold();
    titlesInContent = clickFold.listen(wikiViewArea.value);

    wikiViewArea.value?.addEventListener('scroll',viewAreaScrollHandler);
})
onUnmounted(()=>{
    clickFold.dispose();
    disposeFootNoteJump();
})
</script>

<template>
<div class="wikiViewFrame">
    <div v-if="data" class="wikiView" ref="wikiViewArea">
        <div class="masterTitle">
            {{data.Title}}
        </div>
        <div v-for="p in data.Paras">
            <h1 :id="titleElementId(p.TitleId)">
                {{ p.Title }}
                <div class="h1Sep"></div>
                <div class="editBtn">编辑</div>
            </h1>
            <div class="indent" v-html="p.Content">
            </div>
        </div>
        <div class="footNotes">
            <div v-for="f in data.FootNotes" v-html="f" class="footNote">
            </div>
        </div>
    </div>
    <div class="wikiView" v-else>
        <Loading></Loading>
    </div>

    <div class="subTitles">
        <TitleTree v-if="data" :title-tree="data?.SubTitles" 
        :isMaster="true" @click-title="moveToTitle" ref="titles"></TitleTree>
        <Loading v-else></Loading>
    </div>
</div>
</template>

<style scoped>
.wikiViewFrame{
    height:calc(96vh - var(--top-bar-height));
    display: flex;
    gap:20px;
}
.subTitles{
    width: 160px;
    height:100%;
    overflow-y: auto;
    overflow-x: hidden;
    flex-shrink: 0;
}
.wikiView{
    max-width: 800px;
    position: relative;
    height:100%;
    flex-grow: 1;
    overflow-y: scroll;
    overflow-x: hidden;
}

@media screen and (max-width: 700px){
    .subTitles{
        display: none;
    }
}
</style>