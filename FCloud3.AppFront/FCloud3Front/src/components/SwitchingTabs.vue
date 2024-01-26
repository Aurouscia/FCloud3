<script setup lang="ts">
import { nextTick, onMounted, ref } from 'vue';

const props = defineProps<{texts:string[]}>();
const selectedTab = ref<number>(0);
function switchTo(idx:number){
    selectedTab.value = idx;
    var counter = 0;
    for(var e of tabs){
        if(counter==idx){
            if(e.tagName.toLowerCase()=='table'){
                (e as HTMLElement).style.display='table'
            }else{
                (e as HTMLElement).style.display='block'
            }
        }else{
            (e as HTMLElement).style.display='none'
        }
        counter++;
    }
    emit('switch',idx);
}
const emit = defineEmits<{
    (e:'switch',idx:number):void
}>();
var tabs:HTMLCollection;
const tabsContainer = ref<HTMLDivElement>();
onMounted(async ()=>{
    //可能子代组件没挂载好导致获取不到，必须等到下一个tick再执行
    await nextTick();
    if(tabsContainer.value){
        tabs = tabsContainer.value?.children;
    }
    switchTo(0);
})
</script>

<template>
    <div class="switchingTabs">
        <div class="controls">
            <div v-for="t,idx in props.texts" @click="switchTo(idx)" :class="{selected:idx==selectedTab}">
                {{ t }}
            </div>
        </div>
        <div class="tabsContainer" ref="tabsContainer">
            <slot></slot>
        </div>
    </div>
</template>

<style scoped>

.tabsContainer{
    position: relative;
}
.switchingTabs{
    display: flex;
    flex-direction: column;
    background-color: #fff;
    gap:5px
}
.controls>div.selected{
    background-color: #ccc;
}
.controls>div{
    background-color: #eee;
    flex-grow: 1;
    padding: 10px 3px 10px 3px;
    color:black;
    text-align: center;
    cursor: pointer;
    flex-basis: 10px;
    border-radius: 5px 5px 0px 0px;
    transition: 0.5s;
}
.controls{
    display: flex;
    flex-direction: row;
    justify-content: center;
    align-items: center;
    user-select: none;
    gap:5px;
    border-bottom: 10px solid #ccc;
}
</style>