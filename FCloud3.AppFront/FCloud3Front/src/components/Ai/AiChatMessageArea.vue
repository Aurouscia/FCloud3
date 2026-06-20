<script setup lang="ts">
import { ref, watch, nextTick } from 'vue';
import { injectApi } from '@/provides';
import type { AiMessage, AiMessageRole } from '@/models/ai/aiConversation';

const props = defineProps<{
    conversationId: number;
}>();

const emit = defineEmits<{
    (e: 'send', prompt: string): void;
    (e: 'rename', title: string): void;
}>();

const api = injectApi();

const messages = ref<AiMessage[]>([]);
const prompt = ref('');
const loading = ref(false);
const title = ref('对话');
const messagesEndRef = ref<HTMLDivElement>();

watch(() => props.conversationId, async (id) => {
    if (id) {
        await loadMessages(id);
    } else {
        messages.value = [];
        title.value = '新对话';
    }
}, { immediate: true });

watch(messages, () => {
    nextTick(() => {
        messagesEndRef.value?.scrollIntoView({ behavior: 'smooth' });
    });
}, { deep: true });

async function loadMessages(conversationId: number) {
    loading.value = true;
    const list = await api.ai.chat.getMessages(conversationId);
    messages.value = list || [];
    loading.value = false;
}

async function send() {
    if (!prompt.value.trim()) return;
    const userPrompt = prompt.value;
    prompt.value = '';
    emit('send', userPrompt);
}

function handleKeydown(e: KeyboardEvent) {
    if (e.key === 'Enter' && !e.shiftKey) {
        e.preventDefault();
        send();
    }
}

async function rename() {
    const newTitle = window.prompt('请输入新标题:', title.value);
    if (newTitle && newTitle !== title.value) {
        emit('rename', newTitle);
    }
}

function roleClass(role: AiMessageRole): string {
    switch (role) {
        case 1: return 'user';      // User
        case 2: return 'assistant'; // Assistant
        case 3: return 'tool';      // Tool
        default: return 'system';
    }
}

function roleLabel(role: AiMessageRole): string {
    switch (role) {
        case 1: return '我';
        case 2: return 'AI';
        case 3: return '工具';
        default: return '系统';
    }
}

defineExpose({ loadMessages, appendUserMessage, appendAssistantMessage, setLoading });

function appendUserMessage(content: string) {
    messages.value.push({
        Id: 0,
        ConversationId: props.conversationId,
        Role: 1,
        Content: content,
        ToolCalls: null,
        Order: messages.value.length + 1,
        InputTokenCount: 0,
        OutputTokenCount: 0,
        ModelName: null
    });
}

function appendAssistantMessage(content: string) {
    const last = messages.value[messages.value.length - 1];
    if (last && last.Role === 2) {
        last.Content = (last.Content || '') + content;
    } else {
        messages.value.push({
            Id: 0,
            ConversationId: props.conversationId,
            Role: 2,
            Content: content,
            ToolCalls: null,
            Order: messages.value.length + 1,
            InputTokenCount: 0,
            OutputTokenCount: 0,
            ModelName: null
        });
    }
}

function setLoading(value: boolean) {
    loading.value = value;
}
</script>

<template>
    <div class="aiChatMessageArea">
        <div class="chatHeader">
            <span class="title" @click="rename">{{ title }}</span>
            <span v-if="loading" class="loadingIndicator">思考中...</span>
        </div>
        <div class="messages">
            <div v-for="m in messages" :key="m.Order" :class="['msg', roleClass(m.Role)]">
                <div class="msgLabel">{{ roleLabel(m.Role) }}</div>
                <div class="msgContent">{{ m.Content }}</div>
            </div>
            <div ref="messagesEndRef"></div>
        </div>
        <div class="inputArea">
            <textarea
                v-model="prompt"
                placeholder="向 AI 询问创作建议..."
                @keydown="handleKeydown"
                :disabled="loading"
            />
            <button class="ok" @click="send" :disabled="loading || !prompt.trim()">发送</button>
        </div>
    </div>
</template>

<style lang="scss" scoped>
.aiChatMessageArea {
    display: flex;
    flex-direction: column;
    height: 100%;
}
.chatHeader {
    padding: 8px 12px;
    border-bottom: 1px solid #eee;
    display: flex;
    align-items: center;
    gap: 8px;
    .title {
        font-weight: bold;
        cursor: pointer;
        &:hover {
            text-decoration: underline;
        }
    }
    .loadingIndicator {
        color: #999;
        font-size: 12px;
    }
}
.messages {
    flex: 1;
    overflow-y: auto;
    padding: 8px;
}
.msg {
    padding: 8px;
    margin: 4px 0;
    border-radius: 4px;
    .msgLabel {
        font-size: 12px;
        color: #666;
        margin-bottom: 4px;
    }
    .msgContent {
        white-space: pre-wrap;
        word-break: break-word;
    }
}
.msg.user {
    background: #e3f2fd;
    margin-left: 20%;
}
.msg.assistant {
    background: #f5f5f5;
    margin-right: 20%;
}
.msg.tool {
    background: #fff3e0;
    margin-right: 20%;
    font-size: 12px;
}
.msg.system {
    background: #e8f5e9;
    margin-right: 20%;
    font-size: 12px;
}
.inputArea {
    display: flex;
    gap: 8px;
    padding: 8px;
    border-top: 1px solid #eee;
    textarea {
        flex: 1;
        resize: none;
        height: 60px;
        padding: 8px;
    }
}
</style>
