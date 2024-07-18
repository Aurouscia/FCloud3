<script setup lang="ts">
import { ref } from 'vue';
import { injectApi } from '@/provides';
import { useRouter } from 'vue-router';

const router = useRouter()
const props = defineProps<{
    id:string
}>();
const id = parseInt(props.id);
const api = injectApi();
const failed = ref(false);
if(!isNaN(id) && id>0){
    api.files.fileDir.getPathById(id).then(p=>{
        if(p){
            router.replace(`/d/${p.join('/')}`)
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
    <div v-if="failed">可能原因：操作记录在旧版记录，无法自动跳转</div>
</template>