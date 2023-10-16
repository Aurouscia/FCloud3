<script setup lang="ts">
import {ref} from 'vue';
const show = ref<boolean>(false);
const opaque = ref<boolean>(false);
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
    <div class="functions">
        <img @click="toggleShow" class="menu" :class="{showing:opaque}" src="../assets/menu.svg"/>
        <div class="buttons" v-if="show" :class="{showing:opaque}">
            <slot></slot>
        </div>
    </div>
</template>

<style scoped>
    .functions{
        position: relative;
    }
    img.menu{
        width: 40px;
        height: 30px;
        object-fit: contain;
        cursor: pointer;
        border:2px solid transparent;
        border-radius:5px;
        transition: 0.2s;
    }
    img.showing{
        border:2px solid #666 !important
    }
    .buttons{
        position: absolute;
        right: 0px;
        z-index: 1000;
        width: 150px;
        display: flex;
        flex-direction: column;
        background-color: white;
        border:2px solid #666;
        border-radius: 5px;
        opacity: 0;
        transition: 0.2s;
    }
    .buttons.showing{
        opacity: 1;
    }
</style>