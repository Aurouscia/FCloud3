<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { injectApi, injectPop } from '@/provides';
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
const pop = injectPop();
const conversations = ref<AiConversation[]>([]);
const loading = ref(false);
const showArchived = ref(false);
const activeMenuId = ref<number | null>(null);

onMounted(() => {
    loadConversations();
});

async function loadConversations() {
    loading.value = true;
    const list = await api.ai.chat.getConversations(props.aiInstanceConfigId, showArchived.value);
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
    activeMenuId.value = null;
}

async function togglePin(conv: AiConversation) {
    const ok = await api.ai.chat.setConversationPinned(conv.Id, !conv.IsPinned);
    if (ok) {
        conv.IsPinned = !conv.IsPinned;
        sortConversations();
    }
    activeMenuId.value = null;
}

async function toggleArchive(conv: AiConversation) {
    const ok = await api.ai.chat.setConversationArchived(conv.Id, !conv.IsArchived);
    if (ok) {
        pop.value.show(conv.IsArchived ? '已取消归档' : '已归档', 'success');
        if (!showArchived.value && !conv.IsArchived) {
            // 正在归档且未显示归档列表 → 从列表移除
            conversations.value = conversations.value.filter(c => c.Id !== conv.Id);
        } else {
            conv.IsArchived = !conv.IsArchived;
            sortConversations();
        }
    }
    activeMenuId.value = null;
}

function sortConversations() {
    conversations.value.sort((a, b) => {
        if (a.IsPinned !== b.IsPinned) return a.IsPinned ? -1 : 1;
        return new Date(b.Updated).getTime() - new Date(a.Updated).getTime();
    });
}

function selectConversation(id: number) {
    emit('select', id);
}

function toggleMenu(id: number, e: Event) {
    e.stopPropagation();
    activeMenuId.value = activeMenuId.value === id ? null : id;
}

function closeMenu() {
    activeMenuId.value = null;
}

const displayConversations = computed(() => {
    if (showArchived.value) return conversations.value;
    return conversations.value.filter(c => !c.IsArchived);
});

defineExpose({ loadConversations });
</script>

<template>
    <div class="aiChatConversationList" @click="closeMenu">
        <div class="convHeader">
            <button class="minor" @click="createConversation">+ 新建对话</button>
            <label class="showArchived">
                <input type="checkbox" v-model="showArchived" @change="loadConversations" />
                显示归档
            </label>
        </div>
        <div v-if="loading" class="loading">加载中...</div>
        <div v-else class="convItems">
            <div v-for="c in displayConversations" :key="c.Id"
                 :class="['convItem', { active: c.Id === currentConversationId, pinned: c.IsPinned, archived: c.IsArchived }]"
                 @click="selectConversation(c.Id)">
                <span class="pinIndicator" v-if="c.IsPinned">📌</span>
                <span class="convTitle">{{ c.Title || '未命名对话' }}</span>
                <span class="convCount">({{ c.MessageCount }})</span>
                <button class="menuBtn" @click.stop="toggleMenu(c.Id, $event)">
                    ⋮
                </button>
                <div v-if="activeMenuId === c.Id" class="dropdownMenu" @click.stop>
                    <button class="menuItem" @click="togglePin(c)">
                        {{ c.IsPinned ? '↓ 取消顶置' : '↑ 顶置' }}
                    </button>
                    <button class="menuItem" @click="toggleArchive(c)">
                        {{ c.IsArchived ? '▣ 取消归档' : '▢ 归档' }}
                    </button>
                    <button class="menuItem del" @click="deleteConversation(c.Id)">
                        × 删除
                    </button>
                </div>
            </div>
            <div v-if="displayConversations.length === 0" class="empty">
                {{ showArchived ? '暂无对话' : '暂无未归档对话' }}
            </div>
        </div>
    </div>
</template>

<style lang="scss" scoped>
.aiChatConversationList {
    display: flex;
    flex-direction: column;
    height: 100%;
    position: relative;
}
.convHeader {
    padding: 8px;
    border-bottom: 1px solid #eee;
    display: flex;
    flex-direction: column;
    gap: 8px;
    button {
        width: 100%;
    }
    .showArchived {
        font-size: 12px;
        color: #666;
        display: flex;
        align-items: center;
        gap: 4px;
        cursor: pointer;
        input {
            margin: 0;
        }
    }
}
.convItems {
    flex: 1;
    overflow-y: auto;
}
.convItem {
    padding: 10px 12px;
    cursor: pointer;
    display: flex;
    align-items: center;
    gap: 4px;
    position: relative;
    min-height: 44px;
    &:hover, &.active {
        background: #f0f0f0;
    }
    &.pinned {
        background: #fff8e1;
        .convTitle {
            font-weight: bold;
        }
    }
    &.archived {
        opacity: 0.6;
    }
    .pinIndicator {
        font-size: 12px;
        flex-shrink: 0;
    }
    .convTitle {
        flex: 1;
        overflow: hidden;
        text-overflow: ellipsis;
        white-space: nowrap;
        min-width: 0;
    }
    .convCount {
        color: #999;
        font-size: 12px;
        flex-shrink: 0;
    }
    .menuBtn {
        background: none;
        border: none;
        color: #999;
        cursor: pointer;
        padding: 4px 8px;
        font-size: 16px;
        line-height: 1;
        flex-shrink: 0;
        min-width: 32px;
        min-height: 32px;
        display: flex;
        align-items: center;
        justify-content: center;
        border-radius: 4px;
        &:hover, &:active {
            background: #e0e0e0;
            color: #333;
        }
    }
    .dropdownMenu {
        position: absolute;
        right: 8px;
        top: 36px;
        background: white;
        border: 1px solid #ddd;
        border-radius: 6px;
        box-shadow: 0 2px 8px rgba(0,0,0,0.15);
        z-index: 100;
        min-width: 120px;
        display: flex;
        flex-direction: column;
        padding: 4px 0;
    }
    .menuItem {
        background: none;
        border: none;
        padding: 8px 12px;
        text-align: left;
        cursor: pointer;
        font-size: 14px;
        color: #333;
        white-space: nowrap;
        &:hover, &:active {
            background: #f0f0f0;
        }
        &.del {
            color: #c00;
            &:hover, &:active {
                background: #ffebee;
            }
        }
    }
}
.loading, .empty {
    padding: 16px;
    text-align: center;
    color: #999;
    font-size: 14px;
}

/* 移动端优化 */
@media (max-width: 768px) {
    .convItem {
        padding: 12px;
        .menuBtn {
            padding: 6px 10px;
            font-size: 18px;
        }
        .dropdownMenu {
            right: 4px;
            top: 40px;
            min-width: 140px;
        }
        .menuItem {
            padding: 10px 14px;
            font-size: 15px;
        }
    }
}
</style>
