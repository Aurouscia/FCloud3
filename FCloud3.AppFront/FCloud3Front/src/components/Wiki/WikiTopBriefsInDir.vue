<script setup lang="ts">
import { WikiTopBriefOfDirResponse } from '@/models/etc/wikiTopBriefsOfDir';
import { useWikiParsingRoutesJump } from '@/pages/WikiParsing/routes/routesJump';
import { injectApi } from '@/provides';
import { onMounted, ref } from 'vue';
import Loading from '../Loading.vue';

const props = defineProps<{
    dirId:number,
    takeBriefAt?:number,
    takeKvPairAt?:number
}>()

const api = injectApi()
const data = ref<WikiTopBriefOfDirResponse>()
const { jumpToViewWikiRoute } = useWikiParsingRoutesJump()
async function load(){
    const resp = await api.etc.wikiTopBriefsOfDir.get({
        DirId: props.dirId,
        Skip: 0,
        Take: 100,
        TakeBriefAt: props.takeBriefAt??0,
        TakeKvPairAt: props.takeKvPairAt??0
    })
    if(resp){
        data.value = resp
    }
}
onMounted(async()=>{
    await load()
})
</script>

<template>
<div v-if="data" class="wikiTopBriefsInDir">
    <div v-for="item in data?.Items" :key="item.Id" class="item">
        <div class="topBriefTitle">
            <div class="time">{{ item.Time }}</div>
            <RouterLink :to="jumpToViewWikiRoute(item.PathName)">{{item.Title}}</RouterLink>
        </div>
        <div class="topBriefContent">{{item.Brief}}</div>
    </div>
</div>
<Loading v-else></Loading>
</template>

<style lang="scss" scoped>
.wikiTopBriefsInDir>div{
    padding: 10px;
    border: 0px solid #aaa;
    border-width: 0px 0px 1px 0px;
    margin: 10px;
}
.topBriefTitle{
    display: flex;
    flex-direction: row;
    align-items: flex-end;
    gap: 4px;
    font-size: 18px;
    font-weight: bold;
    margin-bottom: 4px;
    .time{
        font-size: 24px;
    }
}
.topBriefContent{
    font-size: 16px;
    text-indent: 2em;
    margin-left: 10px;
}
</style>