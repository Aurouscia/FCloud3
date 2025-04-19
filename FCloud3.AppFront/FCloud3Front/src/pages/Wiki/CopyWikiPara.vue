<script setup lang="ts">
import Notice from '@/components/Notice.vue';
import Search from '@/components/Search.vue';
import SideBar from '@/components/SideBar.vue';
import { injectApi } from '@/provides';
import { computed, ref, watch } from 'vue';

const props = defineProps<{
    copyingType?:'textSection'|'freeTable'
}>()
watch(()=>props.copyingType, ()=>{
    if(!props.copyingType)
        sidebar.value?.fold()
    else
        sidebar.value?.extend()
})
const copyingTypeText = computed(()=>{
    switch(props.copyingType){
        case 'textSection':
            return '文本';
        case 'freeTable':
            return '表格';
    }
})

const copyingTitle = ref<string>()
const copyingId = ref<number>()
function searchDone(value:string, id:number){
    copyingTitle.value = value;
    copyingId.value = id;
}
function clear(){
    copyingTitle.value = undefined
    copyingId.value = 0
}

const sidebar = ref<InstanceType<typeof SideBar>>();
const api = injectApi();
function confirmed(){
    if(!copyingId.value) return;
    emit('done', copyingId.value)
    copyingId.value = 0
    copyingTitle.value = undefined
    sidebar.value?.fold()
}
const emit = defineEmits<{
    (e:'done', id:number):void
    (e:'open'):void
    (e:'close'):void
}>()
</script>

<template>
<SideBar ref="sidebar" @extend="emit('open')" @fold="clear();emit('close')">
    <h1>复制段落（{{copyingTypeText}}）</h1>
    <Search v-if="copyingType==='textSection'" :source="api.etc.quickSearch.copyableTextSection" :done-when-click-cand="true"
        @done="searchDone" :placeholder="'段落名称'"></Search>
    <Search v-if="copyingType==='freeTable'" :source="api.etc.quickSearch.copyableFreeTable" :done-when-click-cand="true"
        @done="searchDone" :placeholder="'段落名称'"></Search>
    <div v-if="copyingTitle && copyingId" class="copyConfirm">
        <b>{{copyingTitle}}</b>
        <button @click="confirmed">复制到本词条</button>
    </div>
    <Notice :type="'info'">
        为避免盗用，只有名称中含有<br/>
        “<b>（可复制）</b>”（括号为中文括号）<br/>
        的段落可以被复制<br/><br/>
        建议把这样的“复制源”段落单独放置<br/>
        用于快速创建格式相同的段落
    </Notice>
</SideBar>
</template>

<style lang="scss" scoped>
.copyConfirm{
    display: flex;
    flex-direction: column;
    gap: 10px;
    padding: 10px;
    align-items: center;
    b{
        font-size: 20px;
    }
}
</style>