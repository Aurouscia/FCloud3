<script setup lang="ts">
import { ref, computed, watch } from 'vue';
import { injectApi, injectPop } from '@/provides';
import { useAiSelectedInstanceStore } from '@/utils/globalStores/aiSelectedInstance';
import type { AiConversation } from '@/models/ai/aiConversation';
import AiChatConversationList from './AiChatConversationList.vue';
import AiChatMessageArea from './AiChatMessageArea.vue';
import AiInstanceSelector from './AiInstanceSelector.vue';

const props = defineProps<{
    aiInstanceConfigId?: number;
    currentWikiItemId?: number;
}>();

const api = injectApi();
const pop = injectPop();
const store = useAiSelectedInstanceStore();

const currentConversationId = ref<number | undefined>();
const showConvList = ref(true);
const messageAreaRef = ref<InstanceType<typeof AiChatMessageArea>>();
const convListRef = ref<InstanceType<typeof AiChatConversationList>>();
const instanceSelectorRef = ref<InstanceType<typeof AiInstanceSelector>>();

const effectiveInstanceId = computed(() => {
    return props.aiInstanceConfigId ?? store.selectedInstanceId ?? 0;
});

const hasSelectedInstance = computed(() => effectiveInstanceId.value > 0);

watch(effectiveInstanceId, (id) => {
    if (id > 0) {
        currentConversationId.value = undefined;
    }
});

const currentTitle = computed(() => {
    return 'AI 创作助手';
});

function openInstanceSelector() {
    instanceSelectorRef.value?.openSelector();
}

function onSelectConversation(id: number) {
    currentConversationId.value = id;
}

function onCreateConversation(conv: AiConversation) {
    currentConversationId.value = conv.Id;
}

async function onSendMessage(prompt: string) {
    if (!currentConversationId.value) {
        pop.value.show('请先创建或选择一个对话', 'warning');
        return;
    }
    messageAreaRef.value?.appendUserMessage(prompt);
    messageAreaRef.value?.setLoading(true);

    try {
        await fetchStreamSuggestions(prompt);
    } catch (e) {
        pop.value.show('AI 请求失败', 'failed');
    } finally {
        messageAreaRef.value?.setLoading(false);
        convListRef.value?.loadConversations();
    }
}

async function onRename(title: string) {
    if (!currentConversationId.value) return;
    const ok = await api.ai.chat.renameConversation(currentConversationId.value, title);
    if (ok) {
        convListRef.value?.loadConversations();
    }
}

async function fetchStreamSuggestions(userPrompt: string) {
    const url = new URL(`${import.meta.env.VITE_ApiUrlBase}/api/AiChat/GetSuggestions`);
    url.searchParams.set('groupId', effectiveInstanceId.value.toString());
    url.searchParams.set('prompt', userPrompt);
    if (currentConversationId.value) {
        url.searchParams.set('conversationId', currentConversationId.value.toString());
    }
    if (props.currentWikiItemId) {
        url.searchParams.set('currentWikiItemId', props.currentWikiItemId.toString());
    }

    const eventSource = new EventSource(url.toString());
    let result = '';

    return new Promise<void>((resolve, reject) => {
        eventSource.onmessage = (e) => {
            if (e.data) {
                result += e.data;
                messageAreaRef.value?.appendAssistantMessage(e.data);
            }
        };
        eventSource.onerror = (e) => {
            eventSource.close();
            if (result) {
                resolve();
            } else {
                reject(e);
            }
        };
    });
}
</script>

<template>
    <div class="aiChatPanel">
        <AiInstanceSelector
            v-if="!hasSelectedInstance"
            ref="instanceSelectorRef"
            :showSwitchButton="false"
            @selected="currentConversationId = undefined"
        />
        <template v-else>
            <div v-if="showConvList" class="convListSide">
                <div class="instanceBar">
                    <AiInstanceSelector
                        ref="instanceSelectorRef"
                        :showSwitchButton="false"
                        @selected="currentConversationId = undefined"
                    />
                </div>
                <AiChatConversationList
                    ref="convListRef"
                    :aiInstanceConfigId="effectiveInstanceId"
                    :currentConversationId="currentConversationId"
                    @select="onSelectConversation"
                    @create="onCreateConversation"
                />
            </div>
            <div class="chatMain">
                <div class="panelHeader">
                    <button class="minor" @click="showConvList = !showConvList">
                        {{ showConvList ? '◀' : '▶' }}
                    </button>
                    <span class="panelTitle">{{ currentTitle }}</span>
                    <button class="minor switchInstanceBtn" @click="openInstanceSelector">切换实例</button>
                </div>
                <AiChatMessageArea
                    v-if="currentConversationId"
                    ref="messageAreaRef"
                    :conversationId="currentConversationId"
                    @send="onSendMessage"
                    @rename="onRename"
                />
                <div v-else class="noConversation">
                    <p>请从左侧选择一个对话，或新建对话开始聊天</p>
                </div>
            </div>
        </template>
    </div>
</template>

<style lang="scss" scoped>
.aiChatPanel {
    display: flex;
    height: 100%;
    width: 100%;
}
.convListSide {
    width: 260px;
    border-right: 1px solid #ddd;
    flex-shrink: 0;
    display: flex;
    flex-direction: column;
}
.instanceBar {
    border-bottom: 1px solid #eee;
    flex-shrink: 0;
}
.chatMain {
    flex: 1;
    display: flex;
    flex-direction: column;
    min-width: 0;
}
.panelHeader {
    padding: 8px 12px;
    border-bottom: 1px solid #eee;
    display: flex;
    align-items: center;
    gap: 8px;
    .panelTitle {
        font-weight: bold;
        flex: 1;
    }
    .switchInstanceBtn {
        font-size: 12px;
    }
}
.noConversation {
    flex: 1;
    display: flex;
    align-items: center;
    justify-content: center;
    color: #999;
    p {
        text-align: center;
    }
}
</style>
