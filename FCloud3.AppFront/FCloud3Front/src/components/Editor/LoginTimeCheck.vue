<script lang="ts" setup>
import { injectIdentityInfoProvider, injectPop } from '@/provides';
import { onMounted } from 'vue';

const idenProvider = injectIdentityInfoProvider()
const pop = injectPop();
onMounted(async()=>{
    //这种读法会查看缓存是否过期
    const info = await idenProvider.getIdentityInfo()
    if(info.Id>0 && info.LeftHours<=1){
        pop.value.show("登录即将到期，建议重新登录", 'warning')
    }
})
</script>

<template>
</template>