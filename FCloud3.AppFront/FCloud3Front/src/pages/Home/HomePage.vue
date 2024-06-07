<script setup lang="ts">
import WikiCenteredHomePage from './WikiCenteredHomePage.vue';
</script>

<template>
    <h1>欢迎</h1>
    <div class="welcome">
        这里是我们的架空世界地图库和WIKI<br/>
        请随意浏览或者上传自己的作品
    </div>
    <WikiCenteredHomePage>
    </WikiCenteredHomePage>
</template>

<style scoped>
.welcome{
    text-align: center;
    margin: 10px;
    color: #666;
}
h1{
    text-align: center;
    border: none;
    padding: 0px;
    font-size: 22px;
}
</style>