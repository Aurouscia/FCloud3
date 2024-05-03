<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { LatestWorkViewItem, LatestWorkType } from '../models/etc/latestWork';
import { injectApi } from '../provides';
import { getFileIconStyle, getFileExt } from '../utils/fileUtils';
import { jumpToViewWiki } from '../pages/WikiParsing/routes';
import { jumpToUserCenter } from '../pages/Identities/routes';

const props = defineProps<{
    uid?: number,
    noWrap?: boolean
}>()
const api = injectApi();
const list = ref<LatestWorkViewItem[]>([])
const loaded = ref(false);

async function load(){
    const resp = await api.utils.latestWork(props.uid || -1)
    if(resp){
        loaded.value = true;
        list.value = resp;
    }
}
function jumpTo(w:LatestWorkViewItem){
    if(w.Type == LatestWorkType.Wiki){
        jumpToViewWiki(w.JumpParam);
    }else if(w.Type == LatestWorkType.File){
        location.href = w.JumpParam
    }
}

onMounted(async()=>{
    await load();
})
</script>

<template>
<div class="latests" :class="{canWrap:!noWrap}">
    <div v-for="i in list" class="latest">
        <div class="t" @click="jumpTo(i)">
            <div v-if="i.Type==LatestWorkType.Wiki" class="wikiIcon">W</div>
            <div v-else-if="i.Type==LatestWorkType.File" class="icon" :style="getFileIconStyle(i.Title)">{{ getFileExt(i.Title) }}</div>
            <div class="tt">{{ i.Title }}</div>
        </div>
        <div class="right">
            <div class="u" @click="jumpToUserCenter(i.UserName)">{{ i.UserName }}</div>
            <div class="time">{{ i.Time }}</div>
        </div>
    </div>
</div>
</template>

<style scoped lang="scss">
.right{
    display: flex;
    gap: 10px;
}
.u{
    font-weight: bold;
    cursor: pointer;
    &:hover{
        text-decoration: underline;
    }
}
.t{
    display: flex;
    align-items: center;
    gap: 5px;
}
.tt{
    cursor: pointer;
    &:hover{
        text-decoration: underline;
    }
}
.time{
    color: #888;
}
.latest{
    background-color: #eee;
    margin-bottom: 10px;
    padding: 5px;
    display: flex;
    flex-direction: row;
    justify-content: space-between;
    align-items: center;
    gap: 10px;
    &:hover{
        background-color: #d8d8d8;
    }
}
@media screen and (max-width: 500px) {
    .latest{
        flex-direction: column;
        align-items: flex-start;
    }
}
.latests{
    display: flex;
    flex-direction: column;
}
@media screen and (min-width: 1000px) {
    .canWrap{
        flex-direction: row;
        flex-wrap: wrap;
        justify-content: space-between;
        &>div{
            width: 48%;
        }
    } 
}
</style>