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
}
function release(){
    pressed.value = false;
    resting.value = false;
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
    border-left: 3px solid #ccc;
    border-right: 3px solid #ccc;
    text-align: left;
}
.btn{
    margin: 0px;
    border: none;
    user-select: none;
}
.progress{
    margin: 0px;
    height: 10px;
    width:100%;
}
.progressInner{
    margin: auto;
    padding: 0px;
    height: 10px;
    background-color: orange;
}
.resting{
    background-color: yellowgreen;
}
</style>