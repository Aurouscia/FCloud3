<script setup lang="ts">
import {ref,onMounted,onBeforeUnmount} from 'vue'

const props = defineProps<{reached:()=>void}>()

const progress = ref<number>(0);
const pressed = ref<boolean>(false);
const refreshInterval = 10;
const offsetEveryRefresh = 1;
const resting = ref<boolean>(false);

function press(){
    pressed.value = true;
    resting.value = false;
}
function release(){
    pressed.value = false;
}
function refresh(){
    if(pressed.value && !resting.value){
        if(progress.value<100){
            progress.value+=offsetEveryRefresh;
            if(progress.value>=100){
                props.reached();
                resting.value = true;
            }
        }
    }
    else if(progress.value>0)
    {
        progress.value-=offsetEveryRefresh*3;
        if(progress.value<0){
            progress.value = 0;
        }
    }
}
var interval:number;
onMounted(()=>{
    interval = window.setInterval(refresh,refreshInterval);
})
onBeforeUnmount(()=>{
    clearInterval(interval);
})
</script>

<template>
    <div class="outer">
        <div class="progress">
            <div class="progressInner" :class="{resting:resting}" :style="{width:`${progress}%`}">
            </div>
        </div>
        <button class="btn" @touchstart="press" @mousedown="press" @touchend="release" @mouseup="release" @mouseleave="release">
            <slot></slot>
        </button>
        <div class="progress">
            <div class="progressInner" :class="{resting:resting}" :style="{width:`${progress}%`}">
            </div>
        </div>
    </div>
</template>

<style scoped>
.outer{
    display: inline-flex;
    flex-direction: column;
    padding: 0px;
    text-align: left;
}
.outer *{
    transition: none !important;
}
.btn{
    margin: 0px;
    padding: 0px 2px 0px 2px;
    color: #666;
    border: none;
    user-select: none;
    background-color: transparent;
}
.progress{
    margin: 0px;
    height: 5px;
    width:100%;
}
.progressInner{
    margin: auto;
    padding: 0px;
    height: 5px;
    background-color: orange;
}
.resting{
    background-color: yellowgreen;
}
</style>