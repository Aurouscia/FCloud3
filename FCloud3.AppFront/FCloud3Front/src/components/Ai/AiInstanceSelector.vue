<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { injectApi } from '@/provides';
import { useAiSelectedInstanceStore } from '@/utils/globalStores/aiSelectedInstance';
import type { AiInstanceConfigSummary } from '@/models/ai/aiInstanceConfig';

const props = defineProps<{
    showSwitchButton?: boolean;
}>();

const emit = defineEmits<{
    (e: 'selected', instanceId: number): void;
}>();

const api = injectApi();
const store = useAiSelectedInstanceStore();

const instances = ref<AiInstanceConfigSummary[]>([]);
const loading = ref(false);
const showSelector = ref(false);

const selectedInstance = computed(() => {
    return instances.value.find(x => x.Id === store.selectedInstanceId)
});

onMounted(() => {
    loadInstances();
});

async function loadInstances() {
    loading.value = true;
    const list = await api.ai.instanceConfig.getMyAvailableInstances();
    instances.value = list || [];
    loading.value = false;

    // 如果持久化的 instance 不在可用列表中，清除选择
    if (store.selectedInstanceId && !instances.value.some(x => x.Id === store.selectedInstanceId)) {
        store.clearSelectedInstanceId();
    }

    // 如果只有一个可用实例，自动选中
    if (instances.value.length === 1 && !store.selectedInstanceId) {
        selectInstance(instances.value[0].Id);
    }
}

function selectInstance(id: number) {
    store.setSelectedInstanceId(id);
    showSelector.value = false;
    emit('selected', id);
}

function openSelector() {
    loadInstances();
    showSelector.value = true;
}

function closeSelector() {
    showSelector.value = false;
}

defineExpose({ openSelector, loadInstances });
</script>

<template>
    <div class="aiInstanceSelector">
        <div v-if="selectedInstance" class="selectedBar">
            <div class="selectedInfo">
                <span class="groupName">{{ selectedInstance.InstanceName || selectedInstance.GroupName || '未命名实例' }}</span>
                <span class="modelName">{{ selectedInstance.DefaultModelName || '未设置模型' }}</span>
            </div>
            <button v-if="showSwitchButton !== false" class="minor" @click="openSelector">切换</button>
        </div>
        <div v-else class="noSelected">
            <p>未选择 AI 实例</p>
            <button class="ok" @click="openSelector">选择 AI 实例</button>
        </div>

        <div v-if="showSelector" class="selectorModal" @click="closeSelector">
            <div class="selectorContent" @click.stop>
                <div class="selectorHeader">
                    <h3>选择 AI 实例</h3>
                    <button class="minor" @click="closeSelector">关闭</button>
                </div>
                <div v-if="loading" class="loading">加载中...</div>
                <div v-else class="instanceList">
                    <div v-if="instances.length === 0" class="empty">
                        你暂无可用的 AI 实例
                    </div>
                    <div v-for="inst in instances" :key="inst.Id"
                         :class="['instanceItem', { active: inst.Id === store.selectedInstanceId }]"
                         @click="selectInstance(inst.Id)">
                        <div class="instanceName">{{ inst.InstanceName || inst.GroupName || '未命名实例' }}</div>
                        <div class="instanceMeta">
                            <span>默认模型：{{ inst.DefaultModelName || '未设置' }}</span>
                        </div>
                        <div v-if="inst.SystemPrompt" class="systemPrompt">
                            {{ inst.SystemPrompt }}
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<style lang="scss" scoped>
.aiInstanceSelector {
    padding: 12px;
}
.selectedBar {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 12px;
    padding: 10px 12px;
    background: #f5f5f5;
    border-radius: 4px;
    .selectedInfo {
        display: flex;
        flex-direction: column;
        gap: 4px;
        .groupName {
            font-weight: bold;
        }
        .modelName {
            font-size: 12px;
            color: #666;
        }
    }
}
.noSelected {
    text-align: center;
    padding: 24px;
    color: #999;
    p {
        margin-bottom: 12px;
    }
}
.selectorModal {
    position: fixed;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background: rgba(0, 0, 0, 0.3);
    display: flex;
    align-items: center;
    justify-content: center;
    z-index: 2000;
}
.selectorContent {
    background: white;
    border-radius: 8px;
    width: 90%;
    max-width: 500px;
    max-height: 80vh;
    display: flex;
    flex-direction: column;
}
.selectorHeader {
    padding: 16px;
    border-bottom: 1px solid #eee;
    display: flex;
    justify-content: space-between;
    align-items: center;
    h3 {
        margin: 0;
    }
}
.instanceList {
    overflow-y: auto;
    padding: 8px;
}
.instanceItem {
    padding: 12px;
    border: 1px solid #eee;
    border-radius: 4px;
    margin-bottom: 8px;
    cursor: pointer;
    &:hover, &.active {
        background: #f0f8ff;
        border-color: #4a90e2;
    }
    .instanceName {
        font-weight: bold;
        margin-bottom: 4px;
    }
    .instanceMeta {
        font-size: 12px;
        color: #666;
    }
    .systemPrompt {
        margin-top: 8px;
        font-size: 12px;
        color: #999;
        overflow: hidden;
        text-overflow: ellipsis;
        display: -webkit-box;
        -webkit-line-clamp: 2;
        -webkit-box-orient: vertical;
    }
}
.loading, .empty {
    padding: 24px;
    text-align: center;
    color: #999;
}
</style>
