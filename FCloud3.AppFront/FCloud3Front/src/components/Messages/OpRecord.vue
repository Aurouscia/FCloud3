<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { OpRecordViewModel, OpTypeReadable, OpTypeColor, TargetTypeReadable, OpRecordTargetType } from '@/models/messages/opRecord';
import { injectApi, injectPop } from '@/provides';
import Loading from '../Loading.vue';
import { useFilesRoutesJump } from '@/pages/Files/routes/routesJump';
import { useWikiParsingRoutesJump } from '@/pages/WikiParsing/routes/routesJump';
import { useIdentityRoutesJump } from '@/pages/Identities/routes/routesJump';

const props = defineProps<{
    wikiId?: number,
    user?: number
}>()
const { jumpToDirFromIdRoute, jumpToViewFileItemRoute } = useFilesRoutesJump()
const { jumpToViewWikiFromIdRoute } = useWikiParsingRoutesJump()
const { jumpToUserCenterFromIdRoute } = useIdentityRoutesJump();
const api = injectApi();
const records = ref<OpRecordViewModel[]>([])
const loaded = ref(false)
const pop = injectPop()

async function load(active:boolean){
    if(props.wikiId){
        const resp = await api.messages.opRecord.getRecordsOfWiki(props.wikiId)
        if(resp){
            records.value = resp
        }
    } else {
        const resp = await api.messages.opRecord.get(records.value.length, props.user)
        if(resp){
            records.value.push(...resp);
            if(resp.length === 0 && active)
                pop.value.show("没有更多了", "warning")
        }
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
        <div class="c">
            <RouterLink v-if="r.TargetType==OpRecordTargetType.WikiItem" 
                :to="jumpToViewWikiFromIdRoute(r.TargetObjId)" target="_blank">{{ r.Content }}</RouterLink>
            <RouterLink v-else-if="r.TargetType==OpRecordTargetType.FileDir" 
                :to="jumpToDirFromIdRoute(r.TargetObjId)" target="_blank">{{ r.Content }}</RouterLink>
            <RouterLink v-else-if="r.TargetType==OpRecordTargetType.User"
                :to="jumpToUserCenterFromIdRoute(r.TargetObjId)" target="_blank">{{ r.Content }}</RouterLink>
            <RouterLink v-else-if="r.TargetType==OpRecordTargetType.FileItem"
                :to="jumpToViewFileItemRoute(r.TargetObjId)" target="_blank">{{ r.Content }}</RouterLink>
            <div v-else>{{ r.Content }}</div>
        </div>
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
    a{
        color:black
    }
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