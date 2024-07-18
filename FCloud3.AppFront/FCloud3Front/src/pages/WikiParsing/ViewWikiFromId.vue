<script setup lang="ts">
import { ref } from 'vue';
import { injectApi } from '@/provides';
import { useRouter } from 'vue-router';

const router = useRouter()
const props = defineProps<{
    wikiId:string
}>();
const id = parseInt(props.wikiId);
const api = injectApi();
const failed = ref(false);
if(!isNaN(id) && id>0){
    api.wiki.wikiItem.getInfoById(id).then(w=>{
        if(w){
            router.replace(`/w/${w.UrlPathName}`)
        }else{
            failed.value = true
        }
    })
}
else{
    failed.value = true
}
</script>

<template>
    <h1 v-if="!failed">正在跳转</h1>
    <h1 v-else>跳转失败</h1>
</template>