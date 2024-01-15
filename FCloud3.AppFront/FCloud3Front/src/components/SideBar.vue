<script setup lang="ts">
import { CSSProperties, ref } from 'vue';

const coverStyle = ref<CSSProperties>();
const barStyle = ref<CSSProperties>();
const showing = ref<boolean>(false);
const props = defineProps<{
    width?:number
}>()
const width = props.width? props.width+'px' : '300px';
const foldedRight = '-'+width;

barStyle.value = {
    width:width,
    right:foldedRight
}

function extend(){
    showing.value = true;
    coverStyle.value = {
        display:'block',
        opacity:0
    }
    barStyle.value = {
        right:'0px',
        width
    }
    setTimeout(()=>{
        coverStyle.value={
            display:'block',
            opacity:0.3
        }
    })
}
function fold(){
    coverStyle.value = {
        display:'block',
        opacity:0
    }
    barStyle.value = {
        right: foldedRight,
        width
    }
    window.setTimeout(()=>{
        coverStyle.value = {}
        showing.value = false;
    },500)
}
defineExpose({extend,fold})
</script>

<template>
<div class="sidebarOuter">
    <div class="cover" :style="coverStyle"></div>
    <div class="sideBar" :style="barStyle">
        <div>
            <div><button class="off" @click="fold">关闭</button></div>
            <slot v-if="showing"></slot>
        </div>
    </div>
</div>
</template>

<style scoped>
.sideBar>div{
    padding: 20px;
}
.sideBar{
    position: fixed;
    top:0px;
    bottom: 0px;
    display: flex;
    flex-direction: column;
    transition: 0.5s;
    background-color: white;
    z-index: 1001;
}
.cover{
    opacity: 0;
    background-color: black;
    position: fixed;
    top: 0px;bottom:0px;
    left: 0px;right:0px;
    display: none;
    transition: 0.5s;
    z-index: 1000;
}
</style>