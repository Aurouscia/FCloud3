<script setup lang="ts">
import { injectHttp } from '@/provides';
import { useIdentityInfoStore } from '@/utils/globalStores/identityInfo';
import { CurrencyManager, useCurrencyApiStore } from '@fickit/currency';
import { storeToRefs } from 'pinia';

const { iden } = storeToRefs(useIdentityInfoStore())
const http = injectHttp()
useCurrencyApiStore().setRequestInitProvider(()=>{
    return {
        headers: {Authorization: `Bearer ${http.jwtToken}`}
    }
})
</script>

<template>
<CurrencyManager :user-id="iden.Id" :user-level="iden.Type"></CurrencyManager>
</template>