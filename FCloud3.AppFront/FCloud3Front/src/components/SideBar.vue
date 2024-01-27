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
    window.setTimeout(()=>{
        coverStyle.value = {}
        showing.value = false;
    },500)
    emit('fold');
}
defineExpose({extend,fold})
const emit = defineEmits<{
    (e:'extend'):void
    (e:'fold'):void
}>()
</script>

<template>
<div class="sidebarOuter">
    <div class="cover" :style="coverStyle"></div>
    <div class="sideBar" :style="barStyle">
        <div class="offBtn"><button class="cancel" @click="fold">关闭</button></div>
        <div class="body">
            <slot v-if="showing"></slot>
        </div>
    </div>
</div>
</template>

<style scoped>
.offBtn{
    padding: 5px;
    height: 50px;
    overflow: hidden;
    flex-grow: 0;
    flex-shrink: 0;
}
.sideBar button{
    margin-bottom: 10px;
}
.body{
    padding: 10px;
    overflow-y: auto;
    height: calc(100vh - 50px);
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