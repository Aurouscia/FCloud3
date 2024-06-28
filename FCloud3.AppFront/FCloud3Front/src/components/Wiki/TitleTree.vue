<script setup lang="ts">
import { onMounted } from 'vue';
import { ParserTitleTreeNode } from '@/models/wikiParsing/wikiParsingResult';
import { isDefaultFolded } from '@/utils/wikiView/titleClickFold';
import { cmtTitleId } from '@/models/messages/comment';

const props = defineProps<{
    titleTree: ParserTitleTreeNode[];
    isMaster?: boolean|undefined;
}>();

const emit = defineEmits<{
    (e:'clickTitle',titleId:number):void;
}>();

function elementId(id:number):string{
    return 'ct_'+id;
}
let currentHighlight:HTMLElement|undefined;
const highlightClassName = "currentTitleInCatalog";
function highlight(id:number):number|undefined{
    const eleId = elementId(id);
    if(currentHighlight && currentHighlight.id==eleId){return;}
    const ele = document.getElementById(eleId);
    if(!ele){return;}
    currentHighlight?.classList.toggle(highlightClassName,false);
    currentHighlight = ele;
    ele.classList.toggle(highlightClassName,true);
    return ele.offsetTop;
}
onMounted(()=>{
    if(props.isMaster){
        props.titleTree.push({
            Text: "评论区",
            Id: cmtTitleId,
            Level: 0,
            Subs:[]
        })
    }
})
defineExpose({
    highlight
})
</script>

<template>
<div class="titleTreeNodes" :class="{master:props.isMaster}">
    <div v-for="t in props.titleTree" :key="t.Id">
        <div v-if="t.Text && !isDefaultFolded(t.Text)" class="titleTreeNodeText" :class="{master:props.isMaster}" 
            @click="emit('clickTitle',t.Id)" :id="elementId(t.Id)">
            <div class="currentMark"></div>
            <span v-html="t.Text"></span>
        </div>
        <TitleTree v-if="t.Subs" :titleTree="t.Subs" @clickTitle="id=>emit('clickTitle',id)" ref="subs"></TitleTree>
    </div>
</div>
</template>

<style scoped>
.currentTitleInCatalog .currentMark{
    background-color: cornflowerblue;
    position: absolute;
    left: 0px;
    height: 20px;
    width: 5px;
}
.titleTreeNodes div.currentTitleInCatalog{
    color:cornflowerblue;
    font-weight: bold;
}
.titleTreeNodes{
    display: flex;
    flex-direction: column;
    padding-left: 12px;
}
.titleTreeNodes.master{
    border-left: 0.5px solid #ccc;
    padding-left: 18px;
}
.titleTreeNodeText{
    color: #666;
    margin: 10px 0px 10px 0px;
    min-height: 22px;
    font-size: 14px;
    display: flex;
    align-items: center;
}
.titleTreeNodeText:hover{
    color: black;
    cursor: pointer;
}
.titleTreeNodeText.master{
    font-weight: 600;
    font-size: 18px;
    color: #333;
}
</style>