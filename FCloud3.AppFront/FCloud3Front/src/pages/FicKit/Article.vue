<script setup lang="ts">
import { injectHttp } from '@/provides';
import { useIdentityInfoStore } from '@/utils/globalStores/identityInfo';
import { useMainDivDisplayStore } from '@/utils/globalStores/mainDivDisplay';
import { HomeView, useArticleApiStore } from '@fickit/article';
import { storeToRefs } from 'pinia';
import { onMounted, onUnmounted, useTemplateRef } from 'vue';

const { iden } = storeToRefs(useIdentityInfoStore())
const http = injectHttp()
useArticleApiStore().setRequestInitProvider(()=>{
    return {
        headers: {Authorization: `Bearer ${http.jwtToken}`}
    }
})

const articleOuter = useTemplateRef('articleOuter')

const { restrictContentMaxWidth } = storeToRefs(useMainDivDisplayStore())
onMounted(()=>{
    restrictContentMaxWidth.value = false
})
onUnmounted(()=>{
    restrictContentMaxWidth.value = true
})
</script>

<template>
<div class="fickit-article" ref="articleOuter">
    <HomeView :user-id="iden.Id" :user-level="iden.Type" :scroll-container="articleOuter || undefined"></HomeView>
</div>
</template>

<style scoped lang="scss">
@use '../../styles/globalValues';

.fickit-article{
    box-sizing: border-box;
    max-width: 100vw;
    padding: 30px 30px;
    margin: 0px auto;
    height: globalValues.$body-height;
    overflow-y: auto;
}
@media screen and (min-width: 1300px) {
    .fickit-article{
        padding: 30px 200px;
    }
}
:deep(h1){
    border: none;
    padding: unset;
}
</style>