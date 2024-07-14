<script setup lang="ts">
import { computed, onMounted, onUnmounted } from 'vue';
import { useRouter } from 'vue-router';

const router = useRouter();

const props = defineProps<{
    imgSrc:string,
    desc?:string,
    close:()=>void
}>()
const howToSave = computed<string>(()=>{
    const w = window.innerWidth;
    if(w < 1300){
        return "长按保存"
    }else{
        return "右键保存"
    }
})

let removeRouteGuard:()=>void
onMounted(()=>{
    removeRouteGuard = router.beforeEach(()=>{
        removeRouteGuard()
        props.close();
        return false;
    });
})
onUnmounted(()=>{
    removeRouteGuard()
})
</script>

<template>
<div class="bg fixFill"></div>
<div class="cont fixFill" @click="close">
    <img :src="imgSrc"/>
    <div class="desc">{{ desc || imgSrc }}</div>
    <div class="leaveNotice">{{ howToSave }} | 点击任意处离开</div>
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
    gap:10px
}
img{
    max-width: 90vw;
    max-height: calc(100vh - 80px);
}
.desc{
    color: white;
}
.leaveNotice{
    color: #ccc;
    font-size: small;
}
</style>