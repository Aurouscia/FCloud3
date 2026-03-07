<script setup lang="ts">
import SimpleAuTable from '@/components/SimpleAuTable.vue';
import { WikiParaRawContentRes } from '@/models/wiki/wikiPara';
import { WikiParaType } from '@/models/wiki/wikiParaType';
import { injectApi } from '@/provides';
import { computed, onMounted, onUnmounted, ref } from 'vue';
import Loading from '@/components/Loading.vue';
import { useDiffRoutesJump } from '../Diff/routes/routesJump';
import { diffContentTypeFromParaType } from '@/models/diff/diffContentTypes';
import { recoverTitle, setTitleTo } from '@/utils/titleSetter';
import { useIdentityInfoStore } from '@/utils/globalStores/identityInfo';
import { useFilesRoutesJump } from '../Files/routes/routesJump';
import { useRouter } from 'vue-router';
import { allowCopy } from '@/utils/wikiView/allowCopy';

const props = defineProps<{
    paraId:string
}>()
const { jumpToDiffContentHistoryRoute } = useDiffRoutesJump()
const { jumpToViewFileItemRoute } = useFilesRoutesJump()
const router = useRouter()

const api = injectApi()
const paraId = parseInt(props.paraId) || 0
const data = ref<WikiParaRawContentRes>()
const allowCopyThisPara = computed(()=>{
    if(allowCopy(data.value?.Content))
        return true
    return userId && userId == data.value?.OwnerId
})

async function load(){
    data.value = await api.wiki.wikiPara.viewParaRawContent(paraId)
    if(data.value?.ParaType == WikiParaType.File){
        router.replace(jumpToViewFileItemRoute(data.value.ObjId))
    }
}
const userId = useIdentityInfoStore().iden.Id

onMounted(async()=>{
    setTitleTo('段落源码')
    await load()
})
onUnmounted(()=>{
    recoverTitle()
})
</script>

<template>
<h1 v-if="data">{{ data.ParaName || "无名段落" }} - 源码</h1>
<h1 v-else>加载中</h1>
<div v-if="data" class="viewParaRawContent" :class="{noCopy:!allowCopyThisPara}">
    <div class="info">
        <b>上次编辑: {{ data.LastEdit }}</b>
        <RouterLink :to="jumpToDiffContentHistoryRoute(diffContentTypeFromParaType(data.ParaType), data.ObjId)" target="_blank">
            <button class="lite">查看编辑历史</button>
        </RouterLink>
    </div>
    <div class="allowCopyNotice" v-if="allowCopyThisPara">本段落允许读者复制源码</div>
    <div v-if="data.ParaType==WikiParaType.Text" class="textContent">
        {{ data.Content }}
    </div>
    <div v-else-if="data.ParaType==WikiParaType.Table">
        <SimpleAuTable v-if="data" :data="data.Content"></SimpleAuTable>
    </div>
</div>
<Loading v-else></Loading>
</template>

<style scoped lang="scss">
.textContent{
    background-color: #eee;
    padding: 10px;
    white-space: pre-wrap;
}
.info{
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 10px;
}
.noCopy{
    user-select: none;
}
.allowCopyNotice{
    background-color: green;
    padding: 4px;
    color: white;
    text-align: center;
    border-radius: 8px;
    margin-bottom: 10px;
}
</style>