<script setup lang="ts">
import { ref } from 'vue';

const show = ref<boolean>(false);
function setShow(s: boolean) {
    show.value = s;
}
defineExpose({ setShow })
</script>

<template>
    <div v-if="show" class="historyCorruptedNotice fixFill">
        <div class="background fixFill"></div>
        <div class="panel">
            <h2>历史记录损坏</h2>
            <div class="mainInfo">
                非常抱歉，由于一次严重运营事故<br/>
                如果你的文本段落在 2026.6.8 做过改动<br/>
                在那之前的改动内容可能已经损坏，无法正常加载
            </div>
            <button @click="setShow(false)">知道了</button>
        </div>
    </div>
</template>

<style scoped>
.mainInfo {
    color: #999
}

.historyCorruptedNotice {
    z-index: 30000;
    display: flex;
    flex-direction: column;
    justify-content: center;
    align-items: center;
}

.background {
    background-color: black;
    opacity: 0.2;
}

.panel {
    border-radius: 10px;
    box-shadow: 0 0 10px 0 black;
    height: 200px;
    width: 300px;
    background-color: white;
    z-index: 30001;
    display: flex;
    flex-direction: column;
    justify-content: space-around;
    align-items: center;
    text-align: center;
}
</style>
