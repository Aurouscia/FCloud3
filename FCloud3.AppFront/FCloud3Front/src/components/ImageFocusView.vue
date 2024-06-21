<script setup lang="ts">
defineProps<{
    imgSrc:string,
    close:()=>void
}>()
</script>

<template>
<div class="bg fixFill"></div>
<div class="cont fixFill" @click="close">
    <img :src="imgSrc"/>
</div>
</template>

<style scoped>
.bg{
    z-index: 100000;
    background-color: black;
    opacity: 0.8;
}
.cont{
    z-index: 100001;
    display: flex;
    flex-direction: column;
    justify-content: center;
    align-items: center;
}
img{
    max-width: 90vw;
    max-height: 90vh;
}
</style>