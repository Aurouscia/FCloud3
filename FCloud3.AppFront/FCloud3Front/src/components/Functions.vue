<script setup lang="ts">
import menuIconSrc from '../assets/menu.svg';
import {CSSProperties, ref} from 'vue';
const show = ref<boolean>(false);
const opaque = ref<boolean>(false);
const props = defineProps<{
    xAlign?:"left"|"center"|"right",
    yAlign?:"up"|"down"
    imgSrc?:string,
    entrySize?:number,
    leaveKeep?:boolean|undefined
}>();
const entrySizeDefault = 20;
const entrySize = props.entrySize||entrySizeDefault;

var btnsStyle:CSSProperties;
if(props.xAlign=="left"){btnsStyle={left:"0px"}}
else if(props.xAlign=="right"){btnsStyle={right:"0px"}}
else{btnsStyle={right:(-46+entrySize/2)+'px'}}
if(props.yAlign=="up"){
    btnsStyle.top='unset'
    btnsStyle.bottom=(entrySize)+'px'
}
else{
    btnsStyle.top=entrySize+'px'
}

var outerStyle:CSSProperties = {
    width:(entrySize)+'px',
    height:(entrySize)+'px'
}
var entryStyle:CSSProperties = {
    width:(entrySize)-2+'px',
    height:(entrySize)-2+'px'
}

function toggleShow(){
    if(show.value){
        opaque.value=false;
        setTimeout(() => {
            show.value=false;
        },200)
    }else{
        show.value=true;
        setTimeout(() => {
            opaque.value=true;
        }, 10);
    }
}

var closeTimer = 0;
function leave(){
    if(!props.leaveKeep && show.value){
        closeTimer = window.setTimeout(()=>{
            if(show.value){
                toggleShow();
            }
        },500)
    }
}
function enter(){
    window.clearInterval(closeTimer);
}
</script>

<template>
    <div class="functions" @click="toggleShow" @mouseleave="leave" @mouseenter="enter" :style="outerStyle">
        <img class="menu" :class="{showing:opaque}" :src="imgSrc||menuIconSrc" :style="entryStyle"/>
        <div class="buttons" v-if="show" :class="{showing:opaque}" :style="btnsStyle">
            <slot></slot>
        </div>
    </div>
</template>

<style scoped>
    .functions{
        position: relative;
    }
    img.menu{
        position: absolute;
        top:0px;left:0px;right:0px;bottom:0px;
        object-fit: contain;
        cursor: pointer;
        border:2px solid transparent;
        border-radius:30px;
        transition: 0.2s;
    }
    .buttons{
        position: absolute;
        top:0px;
        z-index: 1000;
        width: 80px;
        display: flex;
        flex-direction: column;
        background-color: white;
        border-radius: 5px;
        opacity: 0;
        transition: 0.2s;
        box-shadow: 0px 0px 10px black;
        padding: 4px;
    }
    .buttons.showing{
        opacity: 1;
    }
</style>