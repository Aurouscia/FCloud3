<script setup lang="ts">
import { nextTick, onMounted, onUnmounted, ref} from 'vue'
import { WikiParaDisplay, WikiParaRendered} from '@/models/wiki/wikiPara'
import { WikiParaTypes} from '@/models/wiki/wikiParaTypes'
import { MouseDragListener } from '@/utils/eventListeners/mouseDrag';
import Functions from '@/components/Functions.vue';
import { useRouter } from 'vue-router';
import { Api } from '@/utils/com/api';
import addIconSrc from '@/assets/add.svg';
import dragYIconSrc from '@/assets/dragY.svg';
import { WikiItem } from '@/models/wiki/wikiItem';
import { watchWindowWidth } from '@/utils/eventListeners/windowSizeWatcher';
import SwitchingTabs from '@/components/SwitchingTabs.vue';
import Loading from '@/components/Loading.vue';
import Notice from '@/components/Notice.vue';
import SideBar from '@/components/SideBar.vue';
import WikiFileParaEdit from './WikiFileParaEdit.vue';
import TextParaListItem from './ParaListItem/TextParaListItem.vue';
import FileParaListItem from './ParaListItem/FileParaListItem.vue';
import { useUrlPathNameConverter } from '@/utils/urlPathName';
import { injectApi } from '@/provides';
import TableParaListItem from './ParaListItem/TableParaListItem.vue';
import AuthGrants from '@/components/AuthGrants.vue';
import WikiParaInfo from './WikiParaInfo.vue';
import { AuthGrantOn } from '@/models/identities/authGrant';
import { recoverTitle, setTitleTo } from '@/utils/titleSetter';
import { useWikiParsingRoutesJump } from '../WikiParsing/routes/routesJump';
import { useTableRoutesJump } from '../Table/routes/routesJump';
import { useTextSectionRoutesJump } from '../TextSection/routes/routesJump';
import LongPress from '@/components/LongPress.vue';
import { useIdentityRoutesJump } from '../Identities/routes/routesJump';
import { useWikiRoutesJump } from './routes/routesJump';

const paras = ref<Array<WikiParaRendered>>([])
const spaces = ref<Array<number>>([]);
const paraYSpace = 130;
var api:Api;
const router = useRouter();
const { jumpToViewWiki } = useWikiParsingRoutesJump();
const { jumpToWikiLocations } = useWikiRoutesJump();
const { jumpToFreeTableEdit } = useTableRoutesJump();
const { jumpToTextSectionEdit } = useTextSectionRoutesJump();
const { jumpToSelfUserCenter } = useIdentityRoutesJump();

const props = defineProps<{
    urlPathName:string
}>();

function order2PosY(order:number){
    return order*paraYSpace+15;
}
function posY2order(posY:number){
    return Math.round((posY-15)/paraYSpace);
}
function calculatePosY(){
    paras.value.forEach((p) => {
        if(!p.isMoveing){
            p.posY = order2PosY(p.displayOrder||0);
        }
    });
}
function calculateOrderForMoving(){
    const p = paras.value.find(x=>x.isMoveing);
    const others = paras.value.filter(x=>!x.isMoveing);
    if(p){
        const pposY = order2PosY(p.Order) + offsetY;
        const pOrder = posY2order(pposY);
        others.forEach(x=>{
            if(pOrder>=x.Order && p.Order<x.Order){
                x.displayOrder = x.Order - 1;
            }
            else if(pOrder<=x.Order && p.Order>x.Order){
                x.displayOrder = x.Order + 1;
            }
            else{
                x.displayOrder = x.Order;
            }
        })
        p.posY = pposY;
        calculatePosY();
    }
}
var originalOrder:string;
async function endMoving(){
    const p = paras.value.find(x=>x.isMoveing);
    const others = paras.value.filter(x=>!x.isMoveing);
    if(p && info.value){
        const pposY = order2PosY(p.Order) + offsetY;
        const pOrder = posY2order(pposY);
        p.Order = pOrder;
        p.displayOrder = pOrder;
        p.posY = pposY;
        p.isMoveing = false;
        others.forEach(x=>{
            x.Order = x.displayOrder||0;
        })
        calculatePosY();
        moving = false;
        const list = paras.value.map(x=>{return{Id:x.ParaId,Order:x.Order}})
        list.sort((x,y)=>{
            return x.Order-y.Order
        })
        const ids = list.map(x=>x.Id);
        const newOrder = JSON.stringify(ids);
        if(!originalOrder || newOrder!=originalOrder){
            originalOrder = newOrder;
            const resp = await api.wiki.wikiItem.setParaOrders({
                id:info.value.Id,
                orderedParaIds:ids
            })
            setTimeout(()=>{
                refresh(resp);
            },500)
        }
    }
}
async function InsertPara(type:WikiParaTypes,afterOrder:number){
    if(!info.value){return;}
    const resp = await api.wiki.wikiItem.insertPara({
        id:info.value.Id,
        afterOrder,
        type
    })
    refresh(resp);
}
const fileParaEdit = ref<InstanceType<typeof SideBar>>();
const fileParaEditing = ref<WikiParaDisplay>();
async function EnterEdit(paraId:number)
{
    const target = paras.value.find(x=>x.ParaId == paraId);
    if(!target){return;}
    if(target.Type==0){
        if(target.UnderlyingId && target.UnderlyingId>0){
            jumpToTextSectionEdit(target.UnderlyingId);
            return;
        }
        const resp = await api.textSection.textSection.createForPara({paraId:paraId});
        if(resp){
            const newlyCreatedId = resp.CreatedId;
            jumpToTextSectionEdit(newlyCreatedId);
            return;
        }
    }
    else if(target.Type==1){
        fileParaEditing.value = target;
        fileParaEdit.value?.extend();
    }
    else if(target.Type==2){
        if(target.UnderlyingId && target.UnderlyingId>0){
            jumpToFreeTableEdit(target.UnderlyingId);
            return;
        }
        const resp = await api.table.freeTable.createForPara(paraId);
        if(resp){
            const newlyCreatedId = resp.CreatedId;
            jumpToFreeTableEdit(newlyCreatedId);
            return;
        }
    }
}
async function RemovePara(paraId:number){
    const target = paras.value.find(x=>x.ParaId == paraId);
    if(!target || !info.value){return;}
    if(window.confirm(`确定要将[${target.Title}]从本词条移除`)){
        const resp = await api.wiki.wikiItem.removePara({
            id:info.value.Id,
            paraId:paraId,
        });
        refresh(resp);
    }
}

const editingInfoId = ref<number>(0);
const editingInfoNameOverride = ref<string|null>(null)
const wikiParaInfo = ref<InstanceType<typeof WikiParaInfo>>()
async function StartEditInfo(paraId:number) {
    const target = paras.value.find(x=>x.ParaId == paraId);
    if(!target){return;}
    disposeListeners()
    editingInfoId.value = paraId;
    editingInfoNameOverride.value = target.NameOverride;
    await nextTick()
    wikiParaInfo.value?.comeout()
}

var editingFileParaChanged = false;
async function fileEditFold(){
    if(editingFileParaChanged){
        await Load(false,true);
    }
    editingFileParaChanged = false;
}

const loadComplete = ref<boolean>(false)
async function Load(loadInfo:boolean=true, loadParas:boolean=true){
    if(loadInfo){
        const infoResp = await api.wiki.wikiItem.edit(props.urlPathName)
        if(!infoResp){
            return;
        }
        info.value = infoResp;
        editingWikiTitle.value = info.value.Title;
        editingUrlPathName.value = info.value.UrlPathName;
        setTitleTo("词条编辑 - " + editingWikiTitle.value)
    }
    if(loadParas){
        if(!info.value){return;}
        const parasResp = await api.wiki.wikiItem.loadSimple(info.value.Id);
        if(parasResp){
            loadComplete.value = true;
        }
        originalOrder = JSON.stringify(parasResp?.map(x=>x.ParaId))
        refresh(parasResp);
    }
}
async function refresh(p:WikiParaDisplay[]|undefined) {
    if(p){
        paras.value = p;
        paras.value.forEach(x=>x.displayOrder=x.Order);
        spaces.value = new Array<number>(paras.value.length+1)
    }
    calculatePosY();
}

const info = ref<WikiItem>();
const {name:editingWikiTitle, converted:editingUrlPathName, run:autoUrlName} = useUrlPathNameConverter();

async function saveInfoEdit(){
    if(!info.value){return;}
    if(!editingWikiTitle.value || !editingUrlPathName.value){
        return;
    }
    info.value.Title = editingWikiTitle.value;
    info.value.UrlPathName = editingUrlPathName.value;
    const resp = await api.wiki.wikiItem.editExe(info.value);
    if(resp){
        if(props.urlPathName != info.value.UrlPathName){
            router.replace({name:'wikiEdit',params:{urlPathName:info.value.UrlPathName}})
        }
    }
}

async function del() {
    if(info.value){
        const res = await api.wiki.wikiItem.delete(info.value.Id);
        if(res){
            jumpToSelfUserCenter();
        }
    }
}

var offsetY = 0;
var moving:boolean = false;
var wide = ref<boolean>(false);
var listenerOn = false;
var disposeMouseListener:undefined|(()=>void|undefined);
var disposeResizeListener:undefined|(()=>void|undefined);
function initLisenters(){
    if(listenerOn){return;}
    console.log('注册侦听器')
    const mouse = new MouseDragListener();
    disposeMouseListener = mouse.startListen(
        (_x,y)=>{
            offsetY = y;
            calculateOrderForMoving();
        },
        (_x, _y)=>{
            endMoving();
        },
        ()=>moving
    );
    disposeResizeListener = watchWindowWidth((width)=>{
        wide.value = width>700;
    })
    listenerOn = true;
}
function disposeListeners(){
    console.log('丢弃侦听器')
    if(disposeMouseListener)
        disposeMouseListener();
    if(disposeResizeListener)
        disposeResizeListener();
    listenerOn = false;
}
function tabSwitched(idx:number){
    if(idx==0){
        initLisenters();
    }else{
        disposeListeners();
    }
}
onMounted(async()=>{
    api = injectApi();
    await Load();
    //initLisenters();
})
onUnmounted(()=>{
    recoverTitle()
    disposeListeners();
})
</script>

<template>
    <h1>
        {{ info?.Title }}
        <div class="h1Btns">
            <button v-if="info" @click="jumpToWikiLocations(info?.UrlPathName)">位置管理</button>
            <button v-if="info" @click="jumpToViewWiki(info?.UrlPathName)" class="ok">完成</button>
        </div>
    </h1>
    <SwitchingTabs v-if="loadComplete" :texts="['编辑内容','基础信息','权限设置']" @switch="tabSwitched">
    <div class="paras" ref="parasDiv">
        <div v-for="p in paras" :key="p.ParaId" class="para" :style="{top:p.posY+'px'}"
        :class="{moving:p.isMoveing}">
            <img @mousedown="p.isMoveing=true" @touchstart="p.isMoveing=true;moving=true"
                class="dragY paraButton" :src="dragYIconSrc"/>
            <TextParaListItem v-if="p.Type==0" :w="p"></TextParaListItem>
            <FileParaListItem v-else-if="p.Type==1" :w="p"></FileParaListItem>
            <TableParaListItem v-else :w="p"></TableParaListItem>
            <div class="menu paraButton">
                <button @click="EnterEdit(p.ParaId)">编辑</button>
                <button @click="StartEditInfo(p.ParaId)">设置</button>
                <button @click="RemovePara(p.ParaId)">移除</button>
            </div>
        </div>
        <div v-if="paras" v-for="_,idx in spaces">
            <div class="btnsBetweenPara">
                <Functions :img-src="addIconSrc" :entry-size="30">
                    <button @click="InsertPara(WikiParaTypes.Text, idx - 1)">文本</button>
                    <button @click="InsertPara(WikiParaTypes.File, idx - 1)">文件</button>
                    <button @click="InsertPara(WikiParaTypes.Table, idx - 1)">表格</button>
                </Functions>
            </div>
        </div>
        <Loading v-else></Loading>
        <div v-if="paras && paras.length==0" class="hint">
            请点击<img :src="addIconSrc" class="smallAvatar"/>添加段落以开始创作
        </div>
    </div>
    <div>
        <div class="wikiInfo" v-if="info">
            <table>
                <tr>
                    <td>词条标题</td>
                    <td>
                        <input v-model="editingWikiTitle"/>
                    </td>
                </tr>
                <tr>
                    <td>词条链接名</td>
                    <td>
                        <button @click="autoUrlName" class="minor">由词条标题生成</button><br/>
                        <input v-model="editingUrlPathName"/>
                    </td>
                </tr>
                <tr>
                    <td colspan="2">
                        <button class="confirm" @click="saveInfoEdit">确认</button>
                    </td>
                </tr>
            </table>
            <div :style="{maxWidth:'300px'}">
                <Notice :type="'warn'" >
                    修改链接名称将导致已分享的词条查看链接失效，请谨慎操作。<br/>
                </Notice>
            </div>
            <div style="text-align: center; margin-top: 20px;">
                <LongPress :reached="del">长按删除词条</LongPress>
            </div>
        </div>
        <Loading v-else></Loading>
    </div>
    <div>
        <AuthGrants v-if="info" :on="AuthGrantOn.WikiItem" :on-id="info.Id"></AuthGrants>
    </div>
    </SwitchingTabs>
    <Loading v-else></Loading>
    <SideBar ref="fileParaEdit" @extend="disposeListeners" @fold="initLisenters()">
        <WikiFileParaEdit v-if="fileParaEditing" :para-id="fileParaEditing.ParaId"
            :file-id="fileParaEditing.UnderlyingId" @file-id-set="editingFileParaChanged=true;fileEditFold()"></WikiFileParaEdit>
    </SideBar>
    <WikiParaInfo :para-id="editingInfoId" :current-name-override="editingInfoNameOverride"
        @close="initLisenters" @need-reload="Load(false,true)" ref="wikiParaInfo"></WikiParaInfo>
</template>

<style scoped lang="scss">
@import '@/styles/globalValues';

h1{
    display: flex;
    justify-content: space-between;
    align-items: center;
}
.h1Btns{
    flex-shrink: 0;
    font-size: medium;
}

.wikiInfo>*{
    margin: 0px auto 0px auto;
}

.fileLink{
    margin-top: 16px;
    white-space: nowrap;
    overflow: hidden;
    text-overflow:ellipsis
}
.fileLink span{
    font-size: 12px;
}

.btnsBetweenPara{
    display: flex;
    height: 30px;
    margin-bottom: 100px;
    justify-content:center;
    align-items: center;
}

.paras {
    min-width: 100%;
    max-width: 600px;
    max-height: calc($body-height - 230px);
    overflow-y: auto;
    padding-bottom: 100px;
    position: relative;
    background-color: white;
}

.para {
    position: absolute;
    border-radius: 5px;
    background-color: #eee;
    padding: 10px;
    margin: 10px;
    height: 90px;
    transition: 0.5s;
    width: calc(100% - 40px);
}
.moving{
    background-color: #aaa;
    border: 2px solid #000;
    transition: 0.1s;
    z-index: 110;
}
.paraContent{
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
    height: 2em;
    position: absolute;
    top:43px;
    left:20px;
    max-width: calc(100% - 60px)
}

.paraTitle h2{
    font-size: 20px;
    font-weight: 600;
}
.paraTitle{
    display: flex;
    justify-content: space-between;
    align-items: center;
}
.menu{
    position: absolute;
    width: 130px;
    height: 30px;
    right: 5px;
    bottom: 5px;
    display: flex;
    justify-content: space-between;
    padding: 0px 6px 0px 6px !important;
    gap: 3px;
    button{
        background-color: transparent;
        border: none;
        color:#666;
        padding: 0px;
        margin: 0px;
        &:hover{
            color: black;
            text-decoration: underline;
        }
    }
}
.dragY{
    width: 30px;
    height: 30px;
    object-fit: contain;
    cursor: pointer;
    position: absolute;
    right:5px;
    top:5px;
    z-index: 105;
}

.paraButton{
    background-color: #ddd;
    padding: 2px;
    border-radius: 5px;
    transition: 0.5s;
    border: 2px solid #eee;
}
.moving .paraButton{
    background-color: #999;
    transition: 0.1s;
}

.hint{
    text-align: center;
    color: #666
}
</style>

<style>
.paras .icon{
    background-color: white;
    padding: 2px;
    border-radius: 4px;
}
</style>