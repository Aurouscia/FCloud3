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
const config = ref<AiInstanceConfigEditModel>({
    Id: 0,
    GroupId: 0,
    ApiBaseUrl: '',
    ApiKey: '',
    ModelName: '',
    SystemPrompt: '',
    Enabled: false,
    DefaultDirId: 0,
    MaxContextMessages: 10,
    DailyTokenLimit: 0,
    MonthlyTokenLimit: 0
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
            ApiBaseUrl: existing.ApiBaseUrl || '',
            ApiKey: existing.ApiKey || '',
            ModelName: existing.ModelName || '',
            SystemPrompt: existing.SystemPrompt || '',
            Enabled: existing.Enabled,
            DefaultDirId: existing.DefaultDirId,
            MaxContextMessages: existing.MaxContextMessages,
            DailyTokenLimit: existing.DailyTokenLimit,
            MonthlyTokenLimit: existing.MonthlyTokenLimit
        };
    }
    loadComplete.value = true;
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
                <td>API 地址</td>
                <td><input v-model="config.ApiBaseUrl" placeholder="https://api.example.com/v1"/></td>
            </tr>
            <tr>
                <td>API Key</td>
                <td><input v-model="config.ApiKey" type="password" placeholder="sk-..."/></td>
            </tr>
            <tr>
                <td>模型名</td>
                <td><input v-model="config.ModelName" placeholder="gpt-4o"/></td>
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
                <td>
                    <input type="checkbox" v-model="config.Enabled"/>
                </td>
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
    max-width: 700px;
    margin: auto;
}
table {
    width: 100%;
}
input, textarea {
    width: 200px;
    padding: 3px;
    border-radius: 5px;
    box-sizing: border-box;
    resize: none;
}
button {
    width: 100%;
}
</style>
