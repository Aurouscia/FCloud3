<script setup lang="ts">
import SwitchingTabs from '@/components/SwitchingTabs.vue';
import AuthGrants from '@/components/AuthGrants.vue';
import { AuthGrantOn, authGrantOnIdForAll } from '@/models/identities/authGrant';

</script>

<template>
    <div>
        <h1>全局授权设置</h1>
        <SwitchingTabs :texts="['词条', '目录']">
            <div>
                <AuthGrants :on="AuthGrantOn.WikiItem" :on-id="authGrantOnIdForAll"></AuthGrants>
            </div>
            <div>
                <AuthGrants :on="AuthGrantOn.Dir" :on-id="authGrantOnIdForAll"></AuthGrants>
            </div>
        </SwitchingTabs>
    </div>
</template>

<style scoped>

</style>