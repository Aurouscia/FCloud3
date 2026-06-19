<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { injectApi } from '@/provides';
import type { AiConversation } from '@/models/ai/aiConversation';

const props = defineProps<{
    aiInstanceConfigId: number;
    currentConversationId?: number;
}>();

const emit = defineEmits<{
    (e: 'select', conversationId: number): void;
    (e: 'create', conversation: AiConversation): void;
}>();

const api = injectApi();
const conversations = ref<AiConversation[]>([]);
const loading = ref(false);

onMounted(() => {
    loadConversations();
});

async function loadConversations() {
    loading.value = true;
    const list = await api.ai.chat.getConversations(props.aiInstanceConfigId);
    conversations.value = list || [];
    loading.value = false;
}

async function createConversation() {
    const conv = await api.ai.chat.createConversation(
        props.aiInstanceConfigId,
        undefined,
        undefined,
        0
    );
    if (conv) {
        conversations.value.unshift(conv);
        emit('create', conv);
    }
}

async function deleteConversation(id: number) {
    const ok = await api.ai.chat.deleteConversation(id);
    if (ok) {
        conversations.value = conversations.value.filter(c => c.Id !== id);
    }
}

function selectConversation(id: number) {
    emit('select', id);
}

defineExpose({ loadConversations });
</script>

<template>
    <div class="aiChatConversationList">
        <div class="convHeader">
            <button class="minor" @click="createConversation">+ 新建对话</button>
        </div>
        <div v-if="loading" class="loading">加载中...</div>
        <div v-else class="convItems">
            <div v-for="c in conversations" :key="c.Id"
                 :class="['convItem', { active: c.Id === currentConversationId }]"
                 @click="selectConversation(c.Id)">
                <span class="convTitle">{{ c.Title || '未命名对话' }}</span>
                <span class="convCount">({{ c.MessageCount }})</span>
                <button class="delBtn minor" @click.stop="deleteConversation(c.Id)">×</button>
            </div>
            <div v-if="conversations.length === 0" class="empty">暂无对话</div>
        </div>
    </div>
</template>

<style lang="scss" scoped>
.aiChatConversationList {
    display: flex;
    flex-direction: column;
    height: 100%;
}
.convHeader {
    padding: 8px;
    border-bottom: 1px solid #eee;
    button {
        width: 100%;
    }
}
.convItems {
    flex: 1;
    overflow-y: auto;
}
.convItem {
    padding: 8px 12px;
    cursor: pointer;
    display: flex;
    align-items: center;
    gap: 4px;
    &:hover, &.active {
        background: #f0f0f0;
    }
    .convTitle {
        flex: 1;
        overflow: hidden;
        text-overflow: ellipsis;
        white-space: nowrap;
    }
    .convCount {
        color: #999;
        font-size: 12px;
    }
    .delBtn {
        display: none;
        background: none;
        border: none;
        color: #999;
        cursor: pointer;
        padding: 0 4px;
    }
    &:hover .delBtn {
        display: inline;
    }
}
.loading, .empty {
    padding: 16px;
    text-align: center;
    color: #999;
    font-size: 14px;
}
</style>
