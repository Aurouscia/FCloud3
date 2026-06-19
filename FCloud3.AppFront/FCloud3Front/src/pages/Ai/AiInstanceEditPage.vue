<script setup lang="ts">
import { onMounted, onUnmounted, ref, computed } from 'vue';
import { injectApi, injectPop } from '@/provides';
import { useRouter, useRoute } from 'vue-router';
import Loading from '@/components/Loading.vue';
import type { AiInstanceConfigEditModel } from '@/models/ai/aiInstanceConfig';
import { setTitleTo, recoverTitle } from '@/utils/titleSetter';

const props = defineProps<{
    instanceId?: string
}>();

const api = injectApi();
const pop = injectPop();
const router = useRouter();
const route = useRoute();

const isCreate = computed(() => !props.instanceId || props.instanceId === '0');
const pageTitle = computed(() => isCreate.value ? '新建 AI 实例' : '编辑 AI 实例');

const loadComplete = ref(false);
const availableModels = ref<string[]>([]);
const loadingModels = ref(false);

const config = ref<AiInstanceConfigEditModel>({
    Id: 0,
    GroupId: 0,
    InstanceName: '',
    ApiBaseUrl: '',
    ApiKey: '',
    DefaultModelName: '',
    SystemPrompt: '',
    Enabled: false,
    DefaultDirId: 0,
    MaxContextMessages: 10,
    DailyTokenLimit: 0,
    MonthlyTokenLimit: 0
});

const modelOptions = computed(() => {
    const opts = new Set(availableModels.value);
    if (config.value.DefaultModelName) {
        opts.add(config.value.DefaultModelName);
    }
    return Array.from(opts);
});

async function load() {
    const queryGroupId = parseInt(route.query.groupId as string, 10);
    if (isCreate.value) {
        if (!queryGroupId) {
            pop.value.show('缺少团体 ID，无法新建实例', 'failed');
            return;
        }
        config.value.GroupId = queryGroupId;
        loadComplete.value = true;
        return;
    }

    const instanceId = parseInt(props.instanceId!, 10);
    if (!instanceId) {
        pop.value.show('实例 ID 无效', 'failed');
        return;
    }

    const existing = await api.ai.instanceConfig.get(instanceId);
    if (existing) {
        config.value = {
            ...config.value,
            Id: existing.Id,
            GroupId: existing.GroupId,
            InstanceName: existing.InstanceName || '',
            ApiBaseUrl: existing.ApiBaseUrl || '',
            ApiKey: existing.ApiKey || '',
            DefaultModelName: existing.DefaultModelName || '',
            SystemPrompt: existing.SystemPrompt || '',
            Enabled: existing.Enabled,
            DefaultDirId: existing.DefaultDirId,
            MaxContextMessages: existing.MaxContextMessages,
            DailyTokenLimit: existing.DailyTokenLimit,
            MonthlyTokenLimit: existing.MonthlyTokenLimit
        };
        await loadModels();
    }
    loadComplete.value = true;
}

async function loadModels() {
    if (!config.value.ApiBaseUrl || !config.value.ApiKey) {
        pop.value.show('请先填写 API 地址和 Key', 'warning');
        return;
    }
    loadingModels.value = true;
    const res = await api.ai.instanceConfig.getAvailableModels(
        config.value.ApiBaseUrl,
        config.value.ApiKey
    );
    loadingModels.value = false;
    if (res) {
        availableModels.value = res.Models;
        if (availableModels.value.length === 0) {
            pop.value.show('未获取到可用模型', 'warning');
        }
    } else {
        pop.value.show('获取模型列表失败', 'failed');
    }
}

async function save() {
    const ok = await api.ai.instanceConfig.set(config.value);
    if (ok) {
        router.back();
    }
}

onMounted(() => {
    setTitleTo(pageTitle.value);
    load();
});
onUnmounted(() => {
    recoverTitle();
});
</script>

<template>
    <div v-if="loadComplete" class="aiInstanceEditPage">
        <h1>{{ pageTitle }}</h1>
        <table><tbody>
            <tr>
                <td>实例名称</td>
                <td><input v-model="config.InstanceName" placeholder="如：GPT-4o 生产环境"/></td>
            </tr>
            <tr>
                <td>API 地址</td>
                <td><input v-model="config.ApiBaseUrl" placeholder="https://api.example.com/v1"/></td>
            </tr>
            <tr>
                <td>API Key</td>
                <td><input v-model="config.ApiKey" type="password" placeholder="sk-..."/></td>
            </tr>
            <tr>
                <td>默认模型名</td>
                <td>
                    <div class="modelSelectRow">
                        <select v-model="config.DefaultModelName">
                            <option value="" disabled>请选择模型</option>
                            <option v-for="m in modelOptions" :key="m" :value="m">{{ m }}</option>
                        </select>
                        <button class="minor" :disabled="loadingModels" @click="loadModels">
                            {{ loadingModels ? '获取中...' : '获取模型列表' }}
                        </button>
                    </div>
                </td>
            </tr>
            <tr>
                <td>系统提示词</td>
                <td><textarea v-model="config.SystemPrompt" rows="6" placeholder="向 AI 描述它的角色与回答要求..."></textarea></td>
            </tr>
            <tr>
                <td>默认目录 ID</td>
                <td><input v-model.number="config.DefaultDirId" type="number"/></td>
            </tr>
            <tr>
                <td>最大上下文消息数</td>
                <td><input v-model.number="config.MaxContextMessages" type="number"/></td>
            </tr>
            <tr>
                <td>每日 Token 限额</td>
                <td><input v-model.number="config.DailyTokenLimit" type="number"/></td>
            </tr>
            <tr>
                <td>每月 Token 限额</td>
                <td><input v-model.number="config.MonthlyTokenLimit" type="number"/></td>
            </tr>
            <tr>
                <td>启用</td>
                <td><input type="checkbox" v-model="config.Enabled"/> 启用后组员可在 AI 助手中使用该实例</td>
            </tr>
            <tr>
                <td colspan="2">
                    <button @click="save">保存</button>
                </td>
            </tr>
        </tbody></table>
    </div>
    <Loading v-else></Loading>
</template>

<style lang="scss" scoped>
.aiInstanceEditPage {
    padding: 12px;
    max-width: 700px;
}
h1 {
    font-size: 20px;
    margin-bottom: 12px;
}
table {
    width: 100%;
    border-collapse: collapse;
}
td {
    padding: 8px;
    border: 1px solid #ddd;
    vertical-align: top;
}
td:first-child {
    width: 140px;
    white-space: nowrap;
}
input, textarea, select {
    width: 100%;
    box-sizing: border-box;
}
input[type="checkbox"] {
    width: auto;
    margin-right: 6px;
}
.modelSelectRow {
    display: flex;
    gap: 8px;
    select {
        flex: 1;
    }
    button {
        width: auto;
        white-space: nowrap;
    }
}
button {
    width: 100%;
}
</style>
