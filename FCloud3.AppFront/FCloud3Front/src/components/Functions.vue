<script setup lang="ts">
import menuIconSrc from '../assets/menu.png';
import {ref} from 'vue';
const show = ref<boolean>(false);
const opaque = ref<boolean>(false);
const props = defineProps<{
    xAlign?:"left"|"center"|"right",
    imgSrc?:string
}>();

var btnsXStyle:any;
if(props.xAlign=="left"){btnsXStyle={left:"0px"}}
else if(props.xAlign=="right"){btnsXStyle={right:"0px"}}
else{btnsXStyle={right:"-55px"}}

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

</script>

<template>
    <div class="functions" @click="toggleShow">
        <img class="menu" :class="{showing:opaque}" :src="imgSrc||menuIconSrc"/>
        <div class="buttons" v-if="show" :class="{showing:opaque}" :style="btnsXStyle">
            <slot></slot>
        </div>
    </div>
</template>

<style scoped>
    .functions{
        position: relative;
        height: 44px;
    }
    img.menu{
        width: 40px;
        height: 40px;
        object-fit: contain;
        cursor: pointer;
        border:2px solid transparent;
        border-radius:30px;
        transition: 0.2s;
    }
    img.showing{
        border:2px solid #666 !important;
        box-shadow: 0px 0px 5px black;
    }
    .buttons{
        position: absolute;
        z-index: 1000;
        width: 150px;
        display: flex;
        flex-direction: column;
        background-color: white;
        border:2px solid #666;
        border-radius: 5px;
        opacity: 0;
        transition: 0.2s;
        box-shadow: 0px 0px 5px black;
    }
    .buttons.showing{
        opacity: 1;
    }
</style>