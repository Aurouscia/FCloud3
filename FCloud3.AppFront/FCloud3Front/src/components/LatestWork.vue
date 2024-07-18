<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { LatestWorkViewItem, LatestWorkType } from '../models/etc/latestWork';
import { injectApi } from '../provides';
import { getFileIconStyle, getFileExt } from '../utils/fileUtils';
import { useWikiParsingRoutesJump } from '../pages/WikiParsing/routes/routesJump';
import { useIdentityRoutesJump } from '@/pages/Identities/routes/routesJump';
import { useFilesRoutesJump } from '@/pages/Files/routes/routesJump';

const props = defineProps<{
    uid?: number,
    noWrap?: boolean
}>()
const api = injectApi();
const list = ref<LatestWorkViewItem[]>([])
const loaded = ref(false);
const { jumpToViewWikiRoute } = useWikiParsingRoutesJump();
const { jumpToUserCenterRoute } = useIdentityRoutesJump();
const { jumpToViewFileItemRoute } = useFilesRoutesJump();

async function load(){
    const resp = await api.etc.latestWork.get(props.uid || -1)
    if(resp){
        loaded.value = true;
        list.value = resp;
    }
}

onMounted(async()=>{
    await load();
})
</script>

<template>
<div class="latests" :class="{canWrap:!noWrap}">
    <div v-for="i in list" class="latest">
        <div class="t">
            <div v-if="i.Type==LatestWorkType.Wiki" class="wikiIcon">W</div>
            <div v-else-if="i.Type==LatestWorkType.File" class="icon" :style="getFileIconStyle(i.Title)">{{ getFileExt(i.Title) }}</div>
            <div class="tt">
                <RouterLink v-if="i.Type == LatestWorkType.Wiki" :to="jumpToViewWikiRoute(i.JumpParam)" target="_blank">
                    {{ i.Title }}</RouterLink>
                <RouterLink v-else-if="i.Type == LatestWorkType.File" :to="jumpToViewFileItemRoute(i.ObjId)" target="_blank">
                    {{ i.Title }}</RouterLink>
            </div>
        </div>
        <div class="right">
            <RouterLink class="u" :to="jumpToUserCenterRoute(i.UserName)">{{ i.UserName }}</RouterLink>
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
    word-break: break-all;
    min-width: 30px;
    color:black
}
.t{
    display: flex;
    align-items: center;
    gap: 5px;
    word-break: break-all;
}
.tt{
    cursor: pointer;
    a{
        color: black;
    }
}
.time{
    color: #888;
    white-space: nowrap;
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