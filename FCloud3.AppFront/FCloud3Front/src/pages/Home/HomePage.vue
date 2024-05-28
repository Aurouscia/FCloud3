<script setup lang="ts">
import LatestWork from '@/components/LatestWork.vue';
import OpRecord from '@/components/Messages/OpRecord.vue';
import SwitchingTabs from '@/components/SwitchingTabs.vue';
import Help from './Help.vue';
</script>

<template>
    <h1>
        欢迎
    </h1>
    <div class="welcome">
        欢迎来到FicCloud，这里是用于存储和展示架空设定作品的平台。<br/>
        目前是半成品状态，对操作时遇到的不满意情况请及时反馈
    </div>
    <SwitchingTabs :texts="['作品', '动态' ,'指南']">
        <div>
            <LatestWork>
            </LatestWork>
        </div>
        <OpRecord>
        </OpRecord>
        <Help></Help>
    </SwitchingTabs>
</template>

<style scoped>
.welcome{
    text-align: center;
    margin: 15px;
}
</style>