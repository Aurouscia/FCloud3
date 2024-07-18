<script setup lang="ts">
import { ref } from 'vue';
import { injectApi } from '@/provides';
import { useRouter } from 'vue-router';

const router = useRouter()
const props = defineProps<{
    uid:string
}>();
const id = parseInt(props.uid);
const api = injectApi();
const failed = ref(false);
if(!isNaN(id) && id>0){
    api.identites.user.getInfo(id).then(u=>{
        if(u){
            router.replace(`/u/${u.Name}`)
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