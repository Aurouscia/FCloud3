<script setup lang="ts">
import { provide, ref } from 'vue';
import { ParserTitleTreeNode } from '../../models/wikiParsing/wikiParsingResult';

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
defineExpose({
    highlight
})
</script>

<template>
<div class="titleTreeNodes" :class="{master:props.isMaster}">
    <div v-for="t in props.titleTree">
        <div class="titleTreeNodeText" :class="{master:props.isMaster}" 
            @click="emit('clickTitle',t.Id)" :id="elementId(t.Id)">
            {{ t.Text }}
        </div>
        <TitleTree v-if="t.Subs" :titleTree="t.Subs" @clickTitle="id=>emit('clickTitle',id)" ref="subs"></TitleTree>
    </div>
</div>
</template>

<style scoped>
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
}
.titleTreeNodeText{
    color: #666;
    margin: 10px 0px 10px 0px
}
.titleTreeNodeText:hover{
    color: #333;
    text-decoration: underline;
    cursor: pointer;
}
.titleTreeNodeText.master{
    font-weight: 600;
    font-size: 1.1em;
    color: #333;
}
</style>