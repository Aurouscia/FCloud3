<script setup lang="ts">
import { useIdentityInfoStore } from '@/utils/globalStores/identityInfo';
import { CurrencyManager } from '@fickit/currency';
import { storeToRefs } from 'pinia';

const { iden } = storeToRefs(useIdentityInfoStore())
</script>

<template>
<CurrencyManager :user-id="iden.Id" :user-level="iden.Type"></CurrencyManager>
</template>