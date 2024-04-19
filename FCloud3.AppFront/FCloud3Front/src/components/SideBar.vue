<script setup lang="ts">
import { CSSProperties, ref } from 'vue';
import { SwipeListener } from '../utils/swipeListener';

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

let swl: SwipeListener|undefined
let timer:number
function extend(){
    window.clearTimeout(timer)
    showing.value = true;
    coverStyle.value = {
        display:'block',
        opacity:0
    }
    barStyle.value = {
        right:'0px',
        width,
        boxShadow: '0px 0px 10px 0px black'
    }
    setTimeout(()=>{
        coverStyle.value={
            display:'block',
            opacity:0.3
        }
    })
    emit('extend');
    swl = new SwipeListener((n) => {
        if (n == "right") {
            fold()
        }
    }, "hor", 100)
    swl.startListen()
}
function fold(){
    coverStyle.value = {
        display:'block',
        opacity:0
    }
    barStyle.value = {
        right: foldedRight,
        width,
        boxShadow:'none'
    }
    timer = window.setTimeout(()=>{
        coverStyle.value = {}
        showing.value = false;
    },500)
    emit('fold');
    swl?.stopListen()
    swl = undefined
}
defineExpose({extend,fold})
const emit = defineEmits<{
    (e:'extend'):void
    (e:'fold'):void
}>()
</script>

<template>
<div class="sidebarOuter">
    <div class="cover" :style="coverStyle" @click="fold"></div>
    <div class="sideBar" :style="barStyle">
        <div class="body">
            <slot v-if="showing"></slot>
        </div>
    </div>
</div>
</template>

<style scoped>
.sideBar button{
    margin-bottom: 10px;
}
.body{
    padding: 10px;
    overflow-y: auto;
    height: calc(100vh - 50px);
    position: relative;
}
.sideBar{
    position: fixed;
    top:0px;
    bottom: 0px;
    display: flex;
    flex-direction: column;
    transition: 0.5s;
    background-color: white;
    box-shadow: none;
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