<script setup lang="ts">
import { onMounted, onUnmounted, ref } from 'vue';
import { injectApi } from '../../provides';
import { Api } from '../../utils/api';
import { WikiParsingResult } from '../../models/wikiParsing/wikiParsingResult';
import { TitleClickFold } from '../../utils/wikiView/titleClickFold';
import { useFootNoteJump } from '../../utils/wikiView/footNoteJump';

const props = defineProps<{
    wikiPathName: string;
}>()

const data = ref<WikiParsingResult>();
async function load(){
    data.value = await api.wikiParsing.wikiParsing.getParsedWiki(props.wikiPathName);
}

let api:Api;
let clickFold:TitleClickFold;
const {listenFootNoteJump,disposeFootNoteJump,footNoteJumpCallBack} = useFootNoteJump();
const wikiViewArea = ref<HTMLDivElement>();
onMounted(async()=>{
    api = injectApi();
    await load();
    clickFold = new TitleClickFold();
    clickFold.listen();
    listenFootNoteJump();
    footNoteJumpCallBack.value = (top)=>{
        wikiViewArea.value?.scrollTo({top: top, behavior: 'smooth'})
    };
})
onUnmounted(()=>{
    clickFold.dispose();
    disposeFootNoteJump();
})
</script>

<template>
<div>
    <div v-if="data" class="wikiView" ref="wikiViewArea">
        <div class="masterTitle">
            {{data.Title}}
        </div>
        <div v-for="p in data.Paras">
            <div v-html="p.Content">
            </div>
        </div>
        <div v-html="data.FootNotes"></div>
    </div>
</div>
</template>

<style scoped>
.wikiView{
    position: fixed;
    top:var(--top-bar-height);
    bottom: 0px;
    left:10px;
    right: 10px;
    overflow-y: scroll;
    overflow-x: hidden;
}
</style>