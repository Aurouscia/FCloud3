<script setup lang="ts">
import { onMounted, onUnmounted, provide, ref, watch } from 'vue';
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
import { FileDir,FileDirSubDir,FileDirItem, FileDirWiki, getFileItemsFromIndexResult, getSubDirsFromIndexResult, getWikiItemsFromIndexResult } from '@/models/files/fileDir';
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
//会在OnMounted下一tick被MiniIndex执行，获取thisDirId
const fetchIndex:(q:IndexQuery)=>Promise<IndexResult|undefined>=async(q)=>{
    var p = props.path;
    if(typeof p === 'string'){
        if(p==''){p=[]}
        else{p = [p];}
    }
    const res = await api.fileDir.index(q,p)
    if(res){
        thisDirId.value = res.ThisDirId || 0;
        thisOwnerId.value = res.OwnerId || 0;
        thisOwnerName.value = res.OwnerName;
        items.value = getFileItemsFromIndexResult(res.Items)
        subDirs.value = getSubDirsFromIndexResult(res.SubDirs)
        wikis.value = getWikiItemsFromIndexResult(res.Wikis)
        friendlyPath.value = res.FriendlyPath;
        setFriendlyPathData();
        hideFn.value = false;
        loading.value = false;
        return res?.SubDirs;
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
    if(typeof props.path==='string' || props.path.length==0){
        pathAncestors.value = [];
        pathThisName.value = '';
        pathThis.value = [];
        isRoot.value = true;
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
const sidebar = ref<InstanceType<typeof SideBar>>();
function startEditDirInfo(){
    sidebar.value?.extend();
}
const newDirSidebar = ref<InstanceType<typeof SideBar>>();
function startCreatingDir(){
    newDirSidebar.value?.extend();
}
function dirCreated(pathUrlName:string){
    newDirSidebar.value?.fold();
    jumpToSubDir(pathUrlName);
}
async function deleteDir(dirId:number){
    const resp = await api.fileDir.delete(dirId);
    if(resp){
        await index.value?.reloadData();
    }
}
const authgrantsSidebar = ref<InstanceType<typeof SideBar>>();
function startGrantingAuth(){
    authgrantsSidebar.value?.extend()
}

function autoPageSize() {
    const pos = index.value?.getPosElement();
    if (pos) {
        const posTop = pos.offsetTop
        const winH = window.innerHeight;
        const itemCount = Math.floor((winH - posTop - 20) / 40) - 1;//如果样式调整，这里可能失效
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

const index = ref<InstanceType<typeof IndexMini>>();
let api:Api = injectApi();
const iden = useIdentityInfoStore().iden
onMounted(async()=>{
    setPathData();
    autoPageSize();
    window.addEventListener('resize', windowResizeHandler)
    await index.value?.reloadData()
})
onUnmounted(()=>{
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

const clip = ref<InstanceType<typeof ClipBoard>>();
const fileItemEdit = ref<InstanceType<typeof FileItemEdit>>();
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
    const fileItemIds = move.filter(x=>x.type=='fileItem').map(x=>x.id);
    const fileDirIds = move.filter(x=>x.type=='fileDir').map(x=>x.id);
    const wikiItemIds = move.filter(x=>x.type=='wikiItem').map(x=>x.id);
    const res = await api.fileDir.putInThings(thisDirId.value,fileItemIds,fileDirIds,wikiItemIds);
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
</script>

<template>
    <div class="fileDir">
        <div>
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
                <img class="settingsBtn paddedBtn" @click="startCreatingDir" :src='newDirImg'/>
                <img v-show="iden.Id == thisOwnerId" class="settingsBtn paddedBtn" @click="startGrantingAuth" :src="authgrantsImg"/>
            </div>
            <div v-else-if="thisDirId==0" class="thisName">
                根目录
                <img class="settingsBtn paddedBtn" @click="startCreatingDir" :src='newDirImg'/>
            </div>
        </div>
        <div v-if="thisDirId>0" class="owner">
            目录所有者 <span @click="jumpToUserCenter(thisOwnerName||'??')">{{ thisOwnerName }}</span>
        </div>
        <div v-else class="owner">
            　
        </div>
        <IndexMini ref="index" :fetch-index="fetchIndex" :columns="columns" :display-column-count="1"
            :hide-page="false" :hide-fn="hideFn" :no-load-on-mounted="true">
            <tr v-for="item in subDirs" :key="item.Id">
                <td>
                    <div class="subdir">
                        <div>
                            <div class="foldBtn" v-show="!item.showChildren" @click="item.showChildren=true;hideFnUpdate()" style="color:#999">▶</div>
                            <div class="foldBtn" v-show="item.showChildren" @click="item.showChildren=false;hideFnUpdate()" style="color:black">▼</div>
                            <div class="subdirName" @click="jumpToSubDir(item.UrlPathName)">{{ item.Name }}</div>
                            <Functions :entry-size="20" x-align="left" y-align="up">
                                <button class="danger" @click="deleteDir(item.Id)">删除</button>
                                <button class="minor" @click="toClipBoard($event,item.Id,item.Name,'fileDir')">移动</button>
                            </Functions>
                        </div>
                        <div class="date">
                            {{ item.Updated }}
                        </div>
                    </div>
                    <div class="detail" v-if="item.showChildren">
                        <FileDirChild :dir-id="item.Id" :path="_.concat(props.path, item.UrlPathName)" :fetch-from="api.fileDir.index"></FileDirChild>
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
    <SideBar ref="sidebar">
        <FileDirEdit v-if="thisDirId!=0" :id="thisDirId" :path="pathThis" @info-updated="infoUpdated" @added-new-file="index?.reloadData"></FileDirEdit>
    </SideBar>
    <SideBar ref="newDirSidebar">
        <FileDirCreate :dir-id="thisDirId" :dir-name="friendlyPathThisName||''" @created="dirCreated"></FileDirCreate>
    </SideBar>
    <SideBar ref="authgrantsSidebar">
        <AuthGrants :on="AuthGrantOn.Dir" :on-id="thisDirId"></AuthGrants>
    </SideBar>
    <FileItemEdit ref="fileItemEdit" @need-refresh="index?.reloadData"></FileItemEdit>
    <ClipBoard ref="clip" :current-dir="friendlyPathThisName||'根文件夹'" @put-down="clipBoardAction"></ClipBoard>
</template>

<style scoped lang="scss">
@import '@/styles/globalValues';

.date{
    font-size: 15px;
    color: #666
}
@media screen and (max-width: 500px){
    .date{
        display: none !important;
    }
}
.owner{
    color: #999;
    margin-bottom: 2px;
}
.owner span{
    font-weight: bold;
    cursor: pointer;
}
.owner span:hover{
    text-decoration: underline;
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
}
.ancestors div span{
    padding: 0px 3px 0px 3px;
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
    min-height: 400px;
}
</style>