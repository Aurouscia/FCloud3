<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { guideInfo } from '@/utils/guideInfo';

const show = ref<boolean>(false);
const way = ref<string|undefined>();
function setShow(s:boolean){
    show.value = s;
}
defineExpose({setShow})

onMounted(()=>{
    way.value = guideInfo.applyMember
})
</script>

<template>
    <div v-if="show" class="fixFillPanelOuter fixFill">
        <div class="fixFillPanelBg fixFill"></div>
        <div class="fixFillPanel">
            <h2>无身份</h2>
            <div class="mainInfo">您需要申请正式成员身份<br/>才能进行编辑性质操作</div>
            <div>{{ way }}</div>
            <button @click="setShow(false)">知道了</button>
        </div>
    </div>
</template>

<style scoped>
.mainInfo{
    color: #999
}
</style>