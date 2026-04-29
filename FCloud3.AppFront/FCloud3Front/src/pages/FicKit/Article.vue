<script setup lang="ts">
import { injectHttp } from '@/provides';
import { useIdentityInfoStore } from '@/utils/globalStores/identityInfo';
import { HomeView, useArticleApiStore } from '@fickit/article';
import { storeToRefs } from 'pinia';

const { iden } = storeToRefs(useIdentityInfoStore())
const http = injectHttp()
useArticleApiStore().setRequestInitProvider(()=>{
    return {
        headers: {Authorization: `Bearer ${http.jwtToken}`}
    }
})
</script>

<template>
<div class="fickit-article">
<HomeView :user-id="iden.Id" :user-level="iden.Type"></HomeView>
</div>
</template>

<style scoped lang="scss">
.fickit-article{
    max-width: 960px;
    margin: 20px auto;
}
:deep(h1){
    border: none;
    padding: unset;
}
</style>