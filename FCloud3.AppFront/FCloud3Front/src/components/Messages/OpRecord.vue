<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { OpRecordViewModel, OpTypeReadable, OpTypeColor, TargetTypeReadable } from '../../models/messages/opRecord';
import { injectApi, injectPop } from '../../provides';
import Loading from '../Loading.vue';

const props = defineProps<{
    user?: number
}>()
const api = injectApi();
const records = ref<OpRecordViewModel[]>([])
const loaded = ref(false)
const pop = injectPop()

async function load(active:boolean){
    const resp = await api.messages.opRecord.get(records.value.length, props.user)
    if(resp){
        records.value.push(...resp);
        if(resp.length === 0 && active)
            pop.value.show("没有更多了", "warning")
    }
}
onMounted(async()=>{
    await load(false);
    loaded.value = true
})
</script>

<template>
<div class="opRecords">
    <div v-for="r in records" :key="r.Id" class="record">
        <div class="meta">
            <div class="un">{{ r.UserName }}</div>
            <div class="opType" :style="{color: OpTypeColor(r.OpType)}">
                {{ OpTypeReadable(r.OpType) }}
            </div>
            <div class="tarType">{{ TargetTypeReadable(r.TargetType) }}</div>
        </div>
        <div class="c">{{ r.Content }}</div>
        <div class="t">{{ r.Time }}</div>
    </div>
    <div class="more" @click="load(true)">加载更多</div>
    <Loading v-if="!loaded"></Loading>
</div>

</template>

<style scoped lang="scss">
.record{
    display: flex;
    flex-direction: column;
    align-items: flex-start;
    background-color: #eee;
    margin-bottom: 10px;
    padding: 5px;
}
.meta{
    display: flex;
    gap: 5px
}
.un{
    font-weight: bold;
}
.t{
    color: #666
}
.c{
    color: black;
    background-color: #ddd;
    margin: 5px 0px 5px 0px;
    padding: 5px;
}
.more{
    text-align: center;
    color:#666;
    cursor: pointer;
    margin-bottom: 50px;
    &:hover{
        color: black;
        text-decoration: underline;
    }
}
</style>