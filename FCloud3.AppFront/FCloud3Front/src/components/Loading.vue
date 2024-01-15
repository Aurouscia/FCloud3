<script setup lang="ts">
import { CSSProperties, onMounted, onUnmounted, ref } from 'vue';

const contentStyle = ref<CSSProperties>();
function cycle(){
    contentStyle.value = {
        left:'-300px',
        transition:'0s'
    }
    window.setTimeout(()=>{
        contentStyle.value = {
            left:'calc(100% + 80px)',
            transition:'1s'
        };
    },10)
}
var timer:number=0;
onMounted(()=>{
    cycle();
    timer = window.setInterval(cycle,2000);
})
onUnmounted(()=>{
    window.clearInterval(timer);
})
</script>

<template>
    <div class="loading">
        <div :style="contentStyle" class="loadingContent" ref="content"></div>
    </div>
    <div class="loading">
        <div :style="contentStyle" class="loadingContent" ref="content"></div>
    </div>
</template>

<style scoped>
    .loading{
        position: relative;
        background-color: #ccc;
        border:2px solid #ccc;
        height:1em;
        overflow: hidden;
        margin-top: 15px;
    }
    .loadingContent{
        position: absolute;
        top: 0px;
        bottom: 0px;
        width: 100px;
        background-color: white;
        box-shadow: 0px 0px 40px 40px white;
    }
</style>