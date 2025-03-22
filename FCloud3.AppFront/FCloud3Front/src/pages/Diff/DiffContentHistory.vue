<script setup lang="ts">
import { onMounted, onUnmounted, ref } from 'vue';
import { injectApi, injectPop } from '@/provides';
import { Api } from '@/utils/com/api';
import { DiffContentType, diffContentTypeFromStr } from '@/models/diff/diffContentTypes';
import { DiffContentHistoryResult, DiffContentHistoryResultItem } from '@/models/diff/diffContentHistories';
import DiffContentDetail from './DiffContentDetail.vue';
import { DiffContentStepDisplay } from '@/models/diff/diffContentDetails';
import { watchWindowWidth } from '@/utils/eventListeners/windowSizeWatcher';
import SideBar from '@/components/SideBar.vue';
import Loading from '@/components/Loading.vue';
import { recoverTitle, setTitleTo } from '@/utils/titleSetter';
import _ from 'lodash'
import { useIdentityInfoStore } from '@/utils/globalStores/identityInfo';
import { UserType } from '@/models/identities/user';
import { useIdentityRoutesJump } from '../Identities/routes/routesJump';
import { useMainDivDisplayStore } from '@/utils/globalStores/mainDivDisplay';


const props = defineProps<{
    type?: string
    objId?: string
    // or
    wikiPathName?: string
}>();


const history = ref<DiffContentHistoryResult>()
const { iden } = useIdentityInfoStore()
const { jumpToUserCenterRoute } = useIdentityRoutesJump()
const mainDivDisplayStore = useMainDivDisplayStore()

let displays:DiffContentStepDisplay[] = []
const displaying = ref<DiffContentStepDisplay>()
const detailSidebar = ref<InstanceType<typeof SideBar>>()
async function switchDetail(id:number){
    if(!displays.some(d=>d.Id == id)){
        const resp = (await api.diff.diffContent.detail(id))?.Items || [];
        _.pullAllWith(displays, resp, (x,y)=>x.Id==y.Id)
        displays.push(...resp);
    }
    displaying.value = displays.find(x=>x.Id == id);
    detailSidebar.value?.extend()
}

async function setHidden(diff:DiffContentHistoryResultItem){
    const targetHidden = !diff.H;
    const res = await api.diff.diffContent.setHidden(diff.Id, targetHidden)
    if(res){
        diff.H = targetHidden;
    }
}

const tooNarrow = ref<boolean>(false);
function tooNarrowOrNot(width:number){
    tooNarrow.value = width<800;
}

function arrowKeyHandler(event:KeyboardEvent){
    event.preventDefault()
    const id = displaying.value?.Id
    if(!id){
        const firstHistory = history.value?.Items.at(0)
        if(firstHistory)
            switchDetail(firstHistory.Id)
        return;
    }
    const idx = history.value?.Items.findIndex(x=>x.Id == id);
    const maxIdx = (history.value?.Items.length||0)-1;
    if(idx===undefined || idx<0)
        return;
    if('key' in event){
        let newIdx = -1
        if(event.key == 'ArrowUp'){
            if(idx>0){
                newIdx = idx-1;
            }
        }
        else if(event.key == 'ArrowDown'){
            if(idx<maxIdx){
                newIdx = idx+1;
            } 
        }
        if(newIdx>=0){
            const target = history.value?.Items[newIdx]
            if(target){
                switchDetail(target.Id)
                centralizeHistoryTrInList(target.Id)
            }
        }
    }
}

const historyList = ref<HTMLElement>()
function trEleId(historyId:number){
    return `history-${historyId}`
}
function centralizeHistoryTrInList(id:number){
    const listEle = historyList.value
    const ele = document.getElementById(trEleId(id))
    if(ele && listEle){
        const eleTop = ele.offsetTop
        const eleHeight = ele.offsetHeight
        const listHeight = listEle.clientHeight
        listEle.scrollTo({
            top: eleTop-listHeight/2+eleHeight/2,
            behavior: 'smooth'
        })
    }
}

let api:Api;
let disposeWidthWatch:undefined|(()=>void)
const pop = injectPop();
onMounted(async()=>{
    setTitleTo('编辑历史')
    mainDivDisplayStore.restrictContentMaxWidth = false;
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
    
    window.addEventListener('keydown', arrowKeyHandler)
    disposeWidthWatch = watchWindowWidth(tooNarrowOrNot)
    tooNarrowOrNot(window.innerWidth)
})
onUnmounted(async()=>{
    mainDivDisplayStore.resetToDefault()
    recoverTitle()
    window.removeEventListener('keydown', arrowKeyHandler)
    disposeWidthWatch?.()
})
</script>

<template>
<div class="diffContentHistory">
    <div class="historyList" :class="{grow:tooNarrow}" ref="historyList">
        <table v-if="history">
            <thead>
                <tr>
                    <th class="t">时间</th>
                    <th>操作者</th>
                    <th class="c">变动</th>
                    <th v-if="iden.Type >= UserType.Admin" style="width: 15px;"></th>
                </tr>
            </thead>
            <tbody>
                <tr v-for="i in history.Items" :class="{selected:displaying?.Id==i.Id, hidden:i.H}"
                    @click="switchDetail(i.Id)" :id="trEleId(i.Id)">
                    <td class="t">
                        {{ i.T }}
                    </td>
                    <td class="u">
                        <RouterLink :to="jumpToUserCenterRoute(i.UName)" target="_blank">{{ i.UName }}</RouterLink>
                    </td>
                    <td class="ard">
                        <div class="ar">
                            <span class="a">{{ i.A ? '+'+i.A : ''}}</span>
                            <span class="r">{{ i.R ? '-'+i.R : ''}}</span>
                        </div>
                    </td>
                    <td v-if="iden.Type >= UserType.Admin" class="hideBtn" :class="{hide:!i.H, unhide:i.H}" @click="setHidden(i)">
                        ×
                    </td>
                </tr>
            </tbody>
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
@use '@/styles/globalValues';

.diffContentHistory{
    display: flex;
    flex-direction: row;
    gap: 10px;
    height: globalValues.$body-height;
    box-sizing: border-box;
    padding: 10px 0px 10px 0px;
}
.historyList{
    overflow-y: auto;
    width: 400px;
    flex-shrink: 0;
    flex-grow: 0;
    &.grow{
        flex-grow: 1;
    }
}
tr:hover td{
    background-color: #ddd;
}
tr.hidden:hover td{
    background-color: rgb(255, 214, 255);
}
    td{
        cursor: pointer;
    }
    tr.hidden td{
        background-color: rgb(255, 231, 255);
    }
    tr.selected td{
        background-color: #ccc;
    }
    tr.selected.hidden td{
        background-color: rgb(255, 196, 255);
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
        width: 70px;
    }
    th.c{
        width: 100px;
    }
    .u{
        word-break: break-all;
        a{
            color: black;
        }
    }
    table{
        width: 100%;
        table-layout: fixed;
    }
.hideBtn{
    color:white;
    cursor: pointer;
}
td.hide:hover{
    background-color: red !important;
}
td.unhide:hover{
    background-color: green !important;
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