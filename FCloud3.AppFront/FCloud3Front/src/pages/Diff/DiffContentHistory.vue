<script setup lang="ts">
import { onMounted, onUnmounted, ref } from 'vue';
import { injectApi, injectPop } from '@/provides';
import { Api } from '@/utils/com/api';
import { DiffContentType, diffContentTypeFromStr } from '@/models/diff/diffContentTypes';
import { DiffContentHistoryResult } from '@/models/diff/diffContentHistories';
import DiffContentDetail from './DiffContentDetail.vue';
import { DiffContentStepDisplay } from '@/models/diff/diffContentDetails';
import { watchWindowWidth } from '@/utils/eventListeners/windowSizeWatcher';
import SideBar from '@/components/SideBar.vue';
import Loading from '@/components/Loading.vue';
import { recoverTitle, setTitleTo } from '@/utils/titleSetter';
import _ from 'lodash'

const props = defineProps<{
    type?: string
    objId?: string
    // or
    wikiPathName?: string
}>();


const history = ref<DiffContentHistoryResult>()
const selectedHistoryIdx = ref<number>(-1)

let displays:DiffContentStepDisplay[] = []
const displaying = ref<DiffContentStepDisplay>()
const detailSidebar = ref<InstanceType<typeof SideBar>>()
async function switchDetail(id:number){
    selectedHistoryIdx.value = id;
    if(!displays.some(d=>d.Id == id)){
        const resp = (await api.diff.diffContent.detail(id))?.Items || [];
        _.pullAllWith(displays, resp, (x,y)=>x.Id==y.Id)
        displays.push(...resp);
    }
    displaying.value = displays.find(x=>x.Id == id);
    detailSidebar.value?.extend()
}

const tooNarrow = ref<boolean>(false);
function tooNarrowOrNot(width:number){
    tooNarrow.value = width<700;
}

let api:Api;
let disposeWidthWatch:undefined|(()=>void)
const pop = injectPop();
onMounted(async()=>{
    setTitleTo('编辑历史')
    api = injectApi();
    if(props.type && props.objId){
        const type = diffContentTypeFromStr(props.type)
        const objId = parseInt(props.objId)
        if(!!objId && type != DiffContentType.None)
            history.value = await api.diff.diffContent.history(type, objId)
    }else if(props.wikiPathName){
        history.value = await api.diff.diffContent.historyForWiki(props.wikiPathName)
    }
    if(!history.value){
        pop.value.show('页面参数错误','failed')
    }
    
    disposeWidthWatch = watchWindowWidth(tooNarrowOrNot)
    tooNarrowOrNot(window.innerWidth)
})
onUnmounted(async()=>{
    recoverTitle()
    disposeWidthWatch?.()
})
</script>

<template>
<div class="diffContentHistory">
    <div class="historyList" :class="{grow:tooNarrow}">
        <table v-if="history">
            <tr>
                <th class="t">时间</th>
                <th>操作者</th>
                <th class="c">变动</th>
            </tr>
            <tr v-for="i in history.Items" @click="switchDetail(i.Id)" :class="{selected:selectedHistoryIdx==i.Id}">
                <td class="t">
                    {{ i.T }}
                </td>
                <td class="u">
                    {{ i.UName }}
                </td>
                <td>
                    <div class="ar">
                        <span class="a">{{ i.A ? '+'+i.A : ''}}</span>
                        <span class="r">{{ i.R ? '-'+i.R : ''}}</span>
                    </div>
                </td>
            </tr>
        </table>
        <Loading v-else></Loading>
    </div>
    <SideBar v-if="tooNarrow" ref="detailSidebar">
        <div class="detailInsideSidebar">
            <DiffContentDetail :display="displaying"></DiffContentDetail>
        </div>
    </SideBar>
    <DiffContentDetail v-else :display="displaying"></DiffContentDetail>
</div>
</template>

<style scoped lang="scss">
@import '@/styles/globalValues';

.diffContentHistory{
    display: flex;
    flex-direction: row;
    gap: 10px;
    height: $body-height;
    box-sizing: border-box;
    padding: 10px 0px 10px 0px;
}
.historyList{
    overflow-y: auto;
    width: 300px;
    flex-shrink: 0;
    flex-grow: 0;
    td{
        cursor: pointer;
    }
    tr.selected td{
        background-color: #bbb;
    }
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
    td.t{
        font-size: 14px;
    }
    th.t{
        width: 80px;
    }
    th.c{
        width: 100px;
    }
    .u{
        word-break: break-all;
    }
    table{
        width: 100%;
        table-layout: fixed;
    }
    &.grow{
        flex-grow: 1;
    }
}
.detailInsideSidebar{
    position: absolute;
    top: 0px;
    left: 0px;
    right: 0px;
    bottom: 0px;
    display: flex;
    padding: 10px;
}
</style>