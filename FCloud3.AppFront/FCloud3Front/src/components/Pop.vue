<script setup lang="ts">
import { onMounted, onBeforeUnmount, ref } from 'vue';

const rightDefault = -200;
const height = 70;
export type boxType = "success"|"failed"|"warning"|"info"

interface msgBox {
    right: number,
    msg: string,
    created:number,
    type:boxType
}
const boxes = ref<msgBox[]>([])

function show(msg: string,type: boxType) {
    const newBox:msgBox = {
        right: rightDefault,
        msg: msg,
        created:Number(new Date()),
        type
    };
    boxes.value.push(newBox)
}
function refresh() {
    if(boxes.value.length==0){
        return;
    }
    const now = Number(new Date());
    boxes.value.filter(x=>{
        return now - x.created < 2000 && now-x.created>100
    }).forEach((b)=>{
        b.right = 0
    });
    boxes.value.filter(x=>{
        return now - x.created >= 2000
    }).forEach((b)=>{
        b.right = rightDefault
    });
    boxes.value = boxes.value.filter(x=>{
        return now-x.created <= 2500
    });
}
var interval:number; 
onMounted(()=>{
    interval = setInterval(refresh,20)
})
onBeforeUnmount(()=>{
    clearInterval(interval);
})

defineExpose({ show }) 

</script>

<template>
    <div v-for="box,index in boxes" :key="box.created" class="box" :style="{ 
        right: box.right + 'px',
        width: (-rightDefault) + 'px',
        top: height*index + 100 + 'px'
         }" :class="box.type">
        {{ box.msg }}
    </div>
</template>

<style scoped>
.box {
    position: fixed;
    top: 100px;
    right:-200px;
    height: 60px;
    display: flex;
    align-items: center;
    justify-content: center;
    transition: 0.5s;
    margin: 0px;
    color:white;
    word-break: break-all;
    z-index: 10000;
}
.success{
    background-color: #339933;
}
.failed{
    background-color: #cc2222;
}
.info{
    background-color: #cccccc;
}
.warning{
    background-color: #ffff00;
    color:black;
}
</style>