<script setup lang="ts">
import AiChatPanel from '@/components/Ai/AiChatPanel.vue';

const props = defineProps<{
    groupId?: string;
}>();
</script>

<template>
    <div class="aiChatPage">
        <AiChatPanel :aiInstanceConfigId="groupId ? parseInt(groupId, 10) : undefined" />
    </div>
</template>

<style lang="scss" scoped>
.aiChatPage {
    height: calc(100vh - 60px);
    padding: 8px;
}
</style>
