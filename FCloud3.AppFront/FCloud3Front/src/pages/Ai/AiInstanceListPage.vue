<script setup lang="ts">
import { onMounted, onUnmounted, ref } from 'vue';
import { injectApi, injectPop } from '@/provides';
import Loading from '@/components/Loading.vue';
import type { AiInstanceConfigSummary } from '@/models/ai/aiInstanceConfig';
import { setTitleTo, recoverTitle } from '@/utils/titleSetter';
import { useAiRoutesJump } from './routes/routesJump';

const props = defineProps<{
    groupId: string
}>();

const api = injectApi();
const pop = injectPop();
const { jumpToAiInstanceEdit } = useAiRoutesJump();

const loadComplete = ref(false);
const instances = ref<AiInstanceConfigSummary[]>([]);

async function load() {
    const groupId = parseInt(props.groupId, 10);
    if (!groupId) {
        pop.value.show('团体 ID 无效', 'failed');
        return;
    }
    instances.value = await api.ai.instanceConfig.getList(groupId);
    loadComplete.value = true;
}

function truncate(text: string | null, maxLen: number) {
    if (!text) return '-';
    return text.length > maxLen ? text.slice(0, maxLen) + '...' : text;
}

onMounted(() => {
    setTitleTo('AI 实例管理');
    load();
});
onUnmounted(() => {
    recoverTitle();
});
</script>

<template>
    <div v-if="loadComplete" class="aiInstanceListPage">
        <h1>AI 实例管理</h1>
        <div class="toolbar">
            <button @click="jumpToAiInstanceEdit(undefined, parseInt(groupId))">+ 新增实例</button>
        </div>
        <table><tbody>
            <tr>
                <th>ID</th>
                <th>实例名称</th>
                <th>默认模型名</th>
                <th>系统提示词</th>
                <th>启用</th>
                <th>操作</th>
            </tr>
            <tr v-for="item in instances" :key="item.Id">
                <td>{{ item.Id }}</td>
                <td>{{ item.InstanceName || '-' }}</td>
                <td>{{ item.DefaultModelName || '-' }}</td>
                <td :title="item.SystemPrompt || ''">{{ truncate(item.SystemPrompt, 40) }}</td>
                <td>{{ item.Enabled ? '是' : '否' }}</td>
                <td>
                    <button class="lite" @click="jumpToAiInstanceEdit(item.Id)">编辑</button>
                </td>
            </tr>
            <tr v-if="instances.length === 0">
                <td colspan="6" class="empty">暂无 AI 实例</td>
            </tr>

        </tbody></table>
    </div>
    <Loading v-else></Loading>
</template>

<style lang="scss" scoped>
.aiInstanceListPage {
    max-width: 900px;
    margin: auto;
}
.toolbar {
    margin-bottom: 12px;
}
table {
    width: 100%;
}
.empty {
    text-align: center;
    color: #999;
}
</style>
