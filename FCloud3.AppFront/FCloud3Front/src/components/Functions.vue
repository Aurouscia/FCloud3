<script setup lang="ts">
import menuIconSrc from '../assets/menu.svg';
import {computed, CSSProperties, ref} from 'vue';
const show = ref<boolean>(false);
const opaque = ref<boolean>(false);
const props = defineProps<{
    xAlign?:"left"|"center"|"right",
    yAlign?:"up"|"down"
    imgSrc?:string,
    entrySize?:number,
    entrySizeY?:number,
    leaveKeep?:boolean|undefined,
    button?:{
        text?:string,
        class?:string
    }
}>();
const entrySizeDefault = 20;
const entrySizeX = computed(() => props.entrySize || entrySizeDefault);
const entrySizeY = computed(() => props.entrySizeY || props.entrySize || entrySizeDefault)

const btnsStyle = computed<CSSProperties>(()=>{
    const res:CSSProperties = {}
    if(props.xAlign == 'left')
        res.left = '0px' 
    else if(props.xAlign == 'right')
        res.right = '0px'
    else
        res.right = (-46 + entrySizeX.value / 2)+'px'
    if(props.yAlign=="up"){
        res.top = 'unset'
        res.bottom = entrySizeY.value+'px'
    }
    else{
        res.top = entrySizeY.value+'px'
    }
    return res
})

const outerStyle = computed<CSSProperties>(()=>({
    width: entrySizeX.value+'px',
    height: entrySizeY.value+'px'
}))
const entryStyle = computed<CSSProperties>(()=>({
    width:(entrySizeX.value)-2+'px',
    height:(entrySizeY.value)-2+'px'
}))

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
        <button v-if="button" :class="button.class">{{ button.text }}</button>
        <img v-else class="menu" :class="{showing:opaque}" :src="imgSrc||menuIconSrc" :style="entryStyle"/>
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