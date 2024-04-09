<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { injectApi } from '../../provides';
import { Api } from '../../utils/api';
import { DiffContentType } from '../../models/diff/DiffContentType';
import { DiffContentHistoryResult } from '../../models/diff/DiffContentHistory';

const props = defineProps<{
    type: string;
    objId: string;
}>();
const type = parseInt(props.type) as DiffContentType
const objId = parseInt(props.objId)

const history = ref<DiffContentHistoryResult>()

let api:Api;
onMounted(async()=>{
    api = injectApi();
    history.value = await api.diffContent.history(type, objId)
    await api.diffContent.detail(type, objId)
})
</script>

<template>
<div class="diffContentHistory">
    <table class="historyList" v-if="history">
        <tr>
            <th>时间</th>
            <th>操作者</th>
            <th>变动</th>
        </tr>
        <tr v-for="i in history.Items">
            <td>
                {{ i.T }}
            </td>
            <td>
                {{ i.UName }}
            </td>
            <td class="ar">
                <span class="a">{{ i.A ? '+'+i.A : ''}}</span>
                <span class="r">{{ i.R ? '-'+i.R : ''}}</span>
            </td>
        </tr>
    </table>
    <div class="from">

    </div>
    <div class="to">

    </div>
</div>
</template>

<style scoped lang="scss">
.historyList
.ar{
    display: flex;
    justify-content: space-between;
    align-items: center;
    gap: 10px
}
.a{
    color:green
}
.r{
    color:red
}
</style>