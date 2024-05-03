<script setup lang="ts">
import OpRecord from '../../components/Messages/OpRecord.vue';
</script>

<template>
    <h1>
        欢迎
    </h1>
    <div class="welcome">
        欢迎来到FicCloud，这里是用于存储和展示架空设定作品的平台。<br/>
        开发仍在进行中，对操作时遇到的不满意请及时反馈
    </div>
    <OpRecord>
    </OpRecord>
</template>

<style scoped>
.welcome{
    text-align: center;
    margin: 15px;
}
</style>