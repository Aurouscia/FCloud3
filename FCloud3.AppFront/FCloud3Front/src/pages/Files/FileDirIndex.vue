<script setup lang="ts">
import { computed, onMounted, onUnmounted, provide, ref, useTemplateRef, watch } from 'vue';
import IndexMini, { IndexColumn } from '@/components/Index/IndexMini.vue';
import { Api } from '@/utils/com/api';
import { IndexQuery, IndexResult } from '@/components/Index';
import FileDirChild from './FileDirChild.vue';
import { useRouter } from 'vue-router';
import _ from 'lodash';
import SideBar from '@/components/SideBar.vue';
import settingsImg from '@/assets/settings.svg';
import newDirImg from '@/assets/newDir.svg';
import authgrantsImg from '@/assets/authgrants.svg';
import FileDirEdit from './FileDirEdit.vue';
import { FileDir,FileDirSubDir,FileDirItem, FileDirWiki } from '@/models/files/fileDir';
import FileDirItems from './FileDirItems.vue';
import ClipBoard, { ClipBoardItem, ClipBoardItemType, PutEmitCallBack } from '@/components/ClipBoard.vue';
import Functions from '@/components/Functions.vue';
import FileDirCreate from './FileDirCreate.vue';
import AuthGrants from '@/components/AuthGrants.vue';
import { injectApi } from '@/provides';
import { useIdentityInfoStore } from '@/utils/globalStores/identityInfo';
import FileItemEdit from './FileItemEdit.vue';
import { AuthGrantOn } from '@/models/identities/authGrant';
import { recoverTitle, setTitleTo } from '@/utils/titleSetter';
import { useIdentityRoutesJump } from '@/pages/Identities/routes/routesJump';
import { useWantViewWikiStore } from '@/utils/globalStores/wantViewWiki';
import { useWikiParsingRoutesJump } from '../WikiParsing/routes/routesJump';
import Footer from '@/components/Footer.vue';
import Search from '@/components/Search.vue';
import { useFilesRoutesJump } from './routes/routesJump';
import { useDirInfoTypeStore } from '@/utils/globalStores/dirInfoType';
import { storeToRefs } from 'pinia';
import { useMainDivDisplayStore } from '@/utils/globalStores/mainDivDisplay';


const props = defineProps<{
    path:string[]|string
}>();
const router = useRouter();
const { jumpToViewWiki } = useWikiParsingRoutesJump();
const { jumpToUserCenter } = useIdentityRoutesJump();
const wantViewWikiStore = useWantViewWikiStore();
const columns:IndexColumn[] = 
[
    {name:'Name',alias:'名称',canSearch:true,canSetOrder:true},
    {name:'Updated',alias:'上次更新',canSearch:false,canSetOrder:true}
]

const subDirs = ref<(FileDirSubDir & {showChildren?:boolean|undefined})[]>([]);
const loading = ref(true);
const items = ref<FileDirItem[]>([]);
const wikis = ref<FileDirWiki[]>([]);
const thisDirId = ref<number>(0);
const thisOwnerId = ref<number>(0);
const friendlyPath = ref<string[]>([]);
const friendlyPathAncestors = ref<string[]>([]);
const friendlyPathThisName = ref<string>();
const thisOwnerName = ref<string>();
const asDirId = ref<number>(0);
const asDirFriendlyPath = ref<string[]>([]);
const asDirPath = ref<string[]>([]);
const notAsDir = computed<boolean>(()=>!asDirId.value);
const showAsDirInfo = ref(false);
//会在OnMounted下一tick被MiniIndex执行，获取thisDirId
const fetchIndex:(q:IndexQuery)=>Promise<IndexResult|undefined>=async(q)=>{
    var p = props.path;
    if(typeof p === 'string'){
        if(p==''){p=[]}
        else{p = [p];}
    }
    showAsDirInfo.value = false
    const res = await api.files.fileDir.index(q,p)
    if(res){
        thisDirId.value = res.ThisDirId || 0;
        thisOwnerId.value = res.OwnerId || 0;
        thisOwnerName.value = res.OwnerName;
        items.value = res.Items
        subDirs.value = res.SubDirs
        wikis.value = res.Wikis
        friendlyPath.value = res.FriendlyPath;
        asDirId.value = res.AsDirId;
        asDirFriendlyPath.value = res.AsDirFriendlyPath || [];
        asDirPath.value = res.AsDirPath || [];
        setFriendlyPathData();
        hideFn.value = false;
        loading.value = false;
        const fetched:IndexResult = {
            PageCount:res.PageCount,
            PageIdx:res.PageIdx,
            TotalCount:res.TotalCount,
            Data:[],
            ColumnNames:[]
        }
        return fetched
    }
}


function jumpToSubDir(name:string){
    var path =  _.concat(pathThis.value,name);
    path = _.filter(path, x=>!!x)
    router.push({name:'files',params:{path}});
}
function jumpToAncestor(idxInChain:number){
    router.push({name:'files',params:{path: _.take(pathThis.value,idxInChain+1)}})
}
function infoUpdated(newInfo:FileDir){
    if(newInfo.UrlPathName && newInfo.UrlPathName!=pathThisName.value){
        var path = _.concat(pathAncestors.value,newInfo.UrlPathName);
        path = _.filter(path, x=>!!x)
        router.replace({name:'files',params:{path}});
    }
    if(newInfo.Name && newInfo.Name!=friendlyPathThisName.value){
        friendlyPathThisName.value = newInfo.Name;
    }
}

const pathAncestors = ref<string[]>([]);
const pathThisName = ref<string>("");
const pathThis = ref<string[]>([]);
const isRoot = ref<boolean>(false);
function setPathData(){
    if(typeof props.path==='string'){
        pathAncestors.value = [];
        pathThisName.value = props.path;
        pathThis.value = [props.path];
        isRoot.value = false;
    }else{
        pathThis.value = _.filter(props.path, x=>!!x)
        pathAncestors.value = _.take(pathThis.value,pathThis.value.length-1)
        pathThisName.value = _.last(pathThis.value)||"";
        isRoot.value = pathThis.value.length==0;
    }
}
function setFriendlyPathData(){
    if(friendlyPath.value.length==0){
        friendlyPath.value = [];
        friendlyPathThisName.value = '';
        friendlyPathAncestors.value = [];
        setTitleTo('作品目录')
    }else{
        friendlyPath.value = _.filter(friendlyPath.value, x=>!!x)
        friendlyPathAncestors.value = _.take(friendlyPath.value,friendlyPath.value.length-1)
        friendlyPathThisName.value = _.last(friendlyPath.value)||"";
        setTitleTo(friendlyPathThisName.value)
    }
}
const sidebar = useTemplateRef('sidebar')
function startEditDirInfo(){
    sidebar.value?.extend();
}
const newDirSidebar = useTemplateRef('newDirSidebar')
function startCreatingDir(){
    newDirSidebar.value?.extend();
}
function dirCreated(pathUrlName:string){
    newDirSidebar.value?.fold();
    jumpToSubDir(pathUrlName);
}
async function deleteDir(dirId:number){
    const resp = await api.files.fileDir.delete(dirId);
    if(resp){
        await index.value?.reloadData();
    }
}
const authgrantsSidebar = useTemplateRef('authgrantsSidebar')
function startGrantingAuth(){
    authgrantsSidebar.value?.extend()
}

function autoPageSize() {
    const pos = index.value?.getPosElement();
    if (pos) {
        const posTop = pos.offsetTop
        const winH = window.innerHeight;
        const itemCount = Math.floor((winH - posTop - 30) / 40) - 1;//如果样式调整，这里可能失效
        index.value?.setPageSizeOverride(itemCount)
    }
    else
        index.value?.setPageSizeOverride(20)
}
let resizeStopTimer = 0;
function windowResizeHandler(){
    window.clearTimeout(resizeStopTimer);
    resizeStopTimer = window.setTimeout(()=>{
        const qobj = index.value?.getQObj();
        const anyOpened = subDirs.value.length==0 || subDirs.value.every(x=>!x.showChildren)
        if(anyOpened && qobj && qobj.Page==1){
            //当用户停止resize500ms后，且当前在第一页，且没有文件夹被展开，则重设pageSize并重新加载
            autoPageSize();
            index.value?.reloadData()
        }
    }, 500)
}

const { jumpToViewFileItemRoute, jumpToDirFromId, jumpToDirRoute } = useFilesRoutesJump()
const fileSearchSidebar = useTemplateRef('fileSearchSidebar')
const dirSearchSidebar = useTemplateRef('dirSearchSidebar')
function fileSearchDone(id:number){
    router.push(jumpToViewFileItemRoute(id))
}
function dirSearchDone(id:number){
    jumpToDirFromId(id)
}

const index = useTemplateRef('index')
let api:Api = injectApi();
const iden = useIdentityInfoStore().iden
const mainDivDisplayStore = useMainDivDisplayStore()
onMounted(async()=>{
    setPathData();
    autoPageSize();
    mainDivDisplayStore.enforceScrollY = true;
    window.addEventListener('resize', windowResizeHandler)
    await index.value?.reloadData()
})
onUnmounted(()=>{
    mainDivDisplayStore.resetToDefault()
    window.removeEventListener('resize', windowResizeHandler)
    recoverTitle()
})
watch(props,async(_newVal)=>{
    const wantViewWiki = wantViewWikiStore.readAndReset();
    if(wantViewWiki){
        //如果是“想要查看词条”触发了跳转，则不需要加载本文件夹实际内容，只需在路径历史里留下印记，可以返回这里即可
        jumpToViewWiki(wantViewWiki)
        return;
    }

    const timer = setTimeout(()=>{
        friendlyPathThisName.value = "跳转中..."
        thisDirId.value = -1;
        loading.value = true;
    }, 200)
    setPathData();
    sidebar.value?.fold();
    index.value?.clearSearch(true);
    index.value?.resetPage();
    await index.value?.reloadData();
    clearTimeout(timer);
});
function wantViewWiki(){
    const w = wantViewWikiStore.readAndReset()
    jumpToViewWiki(w);
}

const hideFn = ref<boolean>(false);
function hideFnUpdate(){
    if(subDirs.value.length==0 || subDirs.value.every(x=>!x.showChildren)){
        hideFn.value = false;
    }else{
        hideFn.value = true;
    }
}

const clip = useTemplateRef('clip')
const fileItemEdit = useTemplateRef('fileItemEdit')
provide('clipboard', clip)
provide('fileItemEdit', fileItemEdit)
function toClipBoard(e:MouseEvent, id:number, name:string, type:ClipBoardItemType){
    clip.value?.insert({
        id:id,
        name:name,
        type:type
    },e)
}
async function clipBoardAction(move:ClipBoardItem[], putEmitCallBack:PutEmitCallBack){
    if(!notAsDir.value){
        putEmitCallBack([],'此处为快捷方式目录，不能放入')
        return;
    }
    const fileItemIds = move.filter(x=>x.type=='fileItem').map(x=>x.id);
    const fileDirIds = move.filter(x=>x.type=='fileDir').map(x=>x.id);
    const wikiItemIds = move.filter(x=>x.type=='wikiItem').map(x=>x.id);
    const res = await api.files.fileDir.putInThings(thisDirId.value,fileItemIds,fileDirIds,wikiItemIds);
    if(res){
        const fileItemSuccess:ClipBoardItem[] = res.FileItemSuccess?.map(x=>{return{id:x,type:'fileItem',name:''}})||[]
        const fileDirSuccess:ClipBoardItem[] = res.FileDirSuccess?.map(x=>{return{id:x,type:'fileDir',name:''}})||[]
        const wikiItemSuccess:ClipBoardItem[] = res.WikiItemSuccess?.map(x=>{return{id:x,type:'wikiItem',name:''}})||[]
        console.log(fileItemSuccess,fileDirSuccess,wikiItemSuccess);
        const success = _.concat(fileItemSuccess, fileDirSuccess, wikiItemSuccess)
        console.log(success);
        putEmitCallBack(success,res.FailMsg)
        index.value?.reloadData();
    }
}

const { infoType } = storeToRefs(useDirInfoTypeStore())
</script>

<template>
    <div class="fileDir">
        <div style="position: relative;">
            <div class="ancestors">
                <div>
                    <span @click="jumpToAncestor(-1)">根目录</span>/
                </div>
                <div v-for="a,idx in friendlyPathAncestors">
                    <span @click="jumpToAncestor(idx)">{{ a }}</span>/
                </div> 
            </div>
            <div v-if="thisDirId<0" class="thisName">
                {{ friendlyPathThisName }}
            </div>
            <div v-else-if="friendlyPathThisName && thisDirId>0" class="thisName">
                {{ friendlyPathThisName }}
                <img class="settingsBtn" @click="startEditDirInfo" :src='settingsImg'/>
                <img v-if="notAsDir" class="settingsBtn paddedBtn" @click="startCreatingDir" :src='newDirImg'/>
                <img v-if="notAsDir" v-show="iden.Id == thisOwnerId" class="settingsBtn paddedBtn" @click="startGrantingAuth" :src="authgrantsImg"/>
                <button v-if="!notAsDir" class="minor asDirIcon" @click="showAsDirInfo=true">
                    快捷方式
                </button>
                <div v-if="showAsDirInfo && asDirFriendlyPath" class="asDirInfo">
                    <button class="lite asDirInfoClose" @click="showAsDirInfo=false">×</button>
                    <div>本目录是<b>快捷方式</b></div>
                    <div>实际位置：</div>
                    <RouterLink :to="jumpToDirRoute(asDirPath)">{{ asDirFriendlyPath?.join('/') }}</RouterLink>
                </div>
            </div>
            <div v-else-if="thisDirId==0" class="thisName">
                根目录
                <img class="settingsBtn paddedBtn" @click="startCreatingDir" :src='newDirImg'/>
            </div>
        </div>
        <div class="ownerAndInfoType">
            <div v-if="thisDirId>0" class="owner">
                目录所有者 <span @click="jumpToUserCenter(thisOwnerName||'??')">{{ thisOwnerName }}</span>
            </div>
            <div v-else class="owner searchEntry">
                <button class="lite" @click="fileSearchSidebar?.extend">搜索文件</button> 
                <button class="lite" @click="dirSearchSidebar?.extend">搜索目录</button>
            </div>
            <div class="dirInfoTypeSelector">
                <select v-model="infoType">
                    <option :value="'ownerName'">所有者</option>
                    <option :value="'lastUpdate'">更新</option>
                    <option :value="'size'">尺寸</option>
                </select>
            </div>
        </div>
        <IndexMini ref="index" :fetch-index="fetchIndex" :columns="columns" :display-column-count="1"
            :hide-page="false" :hide-fn="hideFn" :no-load-on-mounted="true">
            <tr v-for="item in subDirs" :key="item.Id">
                <td>
                    <div class="subdir">
                        <div>
                            <div class="foldBtn" v-show="!item.showChildren" @click="item.showChildren=true;hideFnUpdate()" style="color:#999">▶</div>
                            <div class="foldBtn" v-show="item.showChildren" @click="item.showChildren=false;hideFnUpdate()" style="color:black">▼</div>
                            <div class="subdirName nowrapEllipsis" @click="jumpToSubDir(item.UrlPathName)">{{ item.Name }}</div>
                            <Functions :entry-size="20" x-align="left" y-align="up">
                                <button class="danger" @click="deleteDir(item.Id)">删除</button>
                                <button class="minor" @click="toClipBoard($event,item.Id,item.Name,'fileDir')">移动</button>
                            </Functions>
                        </div>
                        <div class="dirSysInfo">
                            {{ infoType=='ownerName' ? item.OwnerName : (infoType=='lastUpdate' ? item.Updated : '') }}
                        </div>
                    </div>
                    <div class="detail" v-if="item.showChildren">
                        <FileDirChild :dir-id="item.Id" :path="_.concat(props.path, item.UrlPathName)" :fetch-from="api.files.fileDir.index"></FileDirChild>
                    </div>
                </td>
            </tr>
            <tr v-if="items.length>0 || wikis.length>0">
                <td>
                    <FileDirItems :dir-id="thisDirId" :items="items" :wikis="wikis" @need-refresh="index?.reloadData" @before-jump-to-wiki="wantViewWiki"></FileDirItems>
                </td>
            </tr>
            <tr v-if="subDirs.length==0 && items.length==0 && wikis.length==0">
                <td>
                    <div v-if="!loading" class="emptyDir">空文件夹</div>
                </td>
            </tr>
        </IndexMini>
    </div>    
    <Footer></Footer>
    <SideBar ref="sidebar">
        <FileDirEdit v-if="thisDirId!=0" :id="thisDirId" :path="pathThis" :is-as-dir="!notAsDir"
            @info-updated="infoUpdated" @added-new-file="index?.reloadData"></FileDirEdit>
    </SideBar>
    <SideBar ref="newDirSidebar">
        <FileDirCreate :dir-id="thisDirId" :dir-name="friendlyPathThisName||''" @created="dirCreated"></FileDirCreate>
    </SideBar>
    <SideBar ref="authgrantsSidebar">
        <AuthGrants :on="AuthGrantOn.Dir" :on-id="thisDirId"></AuthGrants>
    </SideBar>
    <SideBar ref="fileSearchSidebar">
        <h1>搜索文件</h1>
        <Search :source="api.etc.quickSearch.fileItem" @done="(_name,id)=>fileSearchDone(id)" :placeholder="'输入文件名'"></Search>
    </SideBar>
    <SideBar ref="dirSearchSidebar">
        <h1>搜索目录</h1>
        <Search :source="api.etc.quickSearch.fileDir" @done="(_name,id)=>dirSearchDone(id)" :placeholder="'输入目录名'"></Search>
    </SideBar>
    <FileItemEdit ref="fileItemEdit" @need-refresh="index?.reloadData"></FileItemEdit>
    <ClipBoard ref="clip" :current-dir="friendlyPathThisName||'根文件夹'" @put-down="clipBoardAction"></ClipBoard>
</template>

<style scoped lang="scss">
@use '@/styles/globalValues';

.asDirIcon{
    font-size: 14px;
    line-height: 14px;
}
.asDirInfo{
    position: fixed;
    inset: 0px;
    top: calc(globalValues.$topbar-height + 10px);
    width: 220px;
    height: 100px;
    margin-left: auto;
    margin-right: auto;
    box-shadow: 0px 0px 10px 0px black;
    padding: 10px;
    font-size: 16px;
    background-color: white;
    border-radius: 10px;
    z-index: 1000;
    display: flex;
    justify-content: center;
    align-items: center;
    flex-direction: column;
    gap: 5px;
    a{
        font-size: 12px;
    }
    .asDirInfoClose{
        color: plum;
        position: absolute;
        top: 5px;
        right: 10px;
        font-size: 22px;
        cursor: pointer;
    }
}

.ownerAndInfoType{
    display: flex;
    flex-direction: row;
    justify-content: space-between;
    align-items: center;
    height: 28px;
    .dirInfoTypeSelector{
        select{
            margin: 2px;
            padding: 2px;
            font-size: 15px;
        }
    }
    .owner{
        color: #999;
        white-space: nowrap;
        overflow: hidden;
        text-overflow: ellipsis;
    }
    .owner span{
        font-weight: bold;
        cursor: pointer;
    }
    .owner span:hover{
        text-decoration: underline;
    }
}
.searchEntry button{
    text-decoration: underline;
    margin-right: 8px;
}
.settingsBtn:hover{
    background-color: #999;
    cursor: pointer;
}
.settingsBtn{
    object-fit: contain;
    width: 20px;
    height: 20px;
    padding: 2px;
    background-color: #bbb;
    border-radius: 5px;
    transition: 0.5s;
}
.paddedBtn{
    width: 18px;
    height: 18px;
    padding: 4px;
}
.ancestors{
    font-size: small;
    color:gray;
    display: flex;
    overflow: hidden;
    flex-wrap: wrap;
}
.ancestors div span{
    padding: 0px 3px 0px 3px;
    white-space: nowrap;
}
.ancestors div span:hover{
    text-decoration: underline;
    cursor: pointer;
}
.thisName{
    font-size: 20px;
    margin-top: 2px;
    margin-bottom: 2px;
    user-select: none;
    display: flex;
    flex-direction: row;
    align-items:center;
    gap:5px
}
.emptyDir{
    text-align: left;
    color:#999;
    font-size: small;
}
.detail{
    display: flex;
    flex-direction: column;
    gap:5px;
    padding-bottom: 10px;
    border-left: 1px solid black;
    border-bottom: 1px solid black;
    margin-left: 11px;
    padding-left: 4px;
}
.foldBtn{
    width: 20px;
    overflow: visible;
    cursor: pointer;
    user-select: none;
}
.subdirName{
    text-align: left;
    font-weight: bold;
    max-width: calc(100vw - 100px);
    display: block !important;
}
.subdirName:hover{
    text-decoration: underline;
    cursor: pointer;
}
.subdir div{
    display: flex;
    flex-direction: row;
    justify-content: left;
    align-items: center;
    gap:5px
}
.subdir{
    display: flex;
    flex-direction: row;
    justify-content: space-between;
    gap:20px;
    align-items: center;
    padding: 4px;
    height: 20px;
}
.fileDir{
    padding-top: 10px;
    min-height: calc(100vh - 100px);
}
</style>