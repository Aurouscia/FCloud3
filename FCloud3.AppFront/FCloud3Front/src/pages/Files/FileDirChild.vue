<script setup lang="ts">
import { Ref, inject, onMounted, ref } from 'vue';
import FileDirChild from './FileDirChild.vue'
import { useRouter } from 'vue-router';
import _ from 'lodash';
import FileDirItems from './FileDirItems.vue';
import ClipBoard, { ClipBoardItemType } from '../../components/ClipBoard.vue';
import Functions from '../../components/Functions.vue';
import Loading from '../../components/Loading.vue';
import { FileDirIndexResult, FileDirItem, FileDirSubDir, FileDirWiki, getFileItemsFromIndexResult, getSubDirsFromIndexResult, getWikiItemsFromIndexResult } from '../../models/files/fileDir';
import { IndexQuery, indexQueryDefault } from '../../components/Index';
import { Api } from '../../utils/api';

const router = useRouter();

function jumpToSubDir(name:string){
    var path =  _.concat(props.path,name);
    path = _.filter(path, x=>!!x)
    router.push({name:'files',params:{path}});
}

function beforeJumpWiki(){
    //此处为文件夹视图内展开的文件夹显示处（可能有很多层），
    //在跳转到词条之前，先跳转到文件夹本身的页面，避免进入词条后回退到根文件夹
    //跳转到词条本身的操作在FileDirIndex.vue的watch里
    jumpToSubDir('')
}

function isEmptyDir(){
    return items.value.length==0 && subDirs.value.length==0 && wikis.value.length==0;
}
async function deleteDir(dirId:number){
    const resp = await api.fileDir.delete(dirId);
    if(resp){
        await loadData();
    }
}

const subDirs = ref<(FileDirSubDir & {showChildren?:boolean|undefined})[]>([]);
const items = ref<FileDirItem[]>([]);
const wikis = ref<FileDirWiki[]>([]);
const more = ref<boolean>(false);
function moreJump(){
    jumpToSubDir("")
}

const props = defineProps<{
    dirId:number,
    path:string[]|string,
    fetchFrom:(q:IndexQuery, path:string[])=>Promise<FileDirIndexResult|undefined>
}>();

//const data = ref<FileDirIndexResult>();
const showLoading = ref<boolean>(true);
var clip:Ref<InstanceType<typeof ClipBoard>>
var api:Api;

async function loadData(){
    var path = _.filter(props.path, x=>!!x)
    var data = await props.fetchFrom(indexQueryDefault(), path);    
    if(data){
        showLoading.value = false;
        items.value = getFileItemsFromIndexResult(data.Items)
        subDirs.value = getSubDirsFromIndexResult(data.SubDirs)
        wikis.value = getWikiItemsFromIndexResult(data.Wikis)
        more.value = data.SubDirs.PageCount > 1
    }
}

onMounted(async()=>{
    clip = inject('clipboard') as Ref<InstanceType<typeof ClipBoard>>;
    api = inject('api') as Api;
    await loadData();
})
function toClipBoard(e:MouseEvent, id:number, name:string, type:ClipBoardItemType){
    clip.value?.insert({
        id:id,
        name:name,
        type:type
    },e)
}
</script>

<template>
    <div class="fileDirChild">
        <div v-for="subdir in subDirs" :key="subdir.Id">
            <div class="subdir">
                <div>
                    <div class="foldBtn" v-show="!subdir.showChildren" @click="subdir.showChildren = true" style="color:#999">▶
                    </div>
                    <div class="foldBtn" v-show="subdir.showChildren" @click="subdir.showChildren = false" style="color:black">▼
                    </div>
                    <div class="subdirName" @click="jumpToSubDir(subdir.UrlPathName)">{{ subdir.Name }}</div>
                    <Functions :entry-size="20" x-align="left" y-align="up">
                        <button class="danger" @click="deleteDir(subdir.Id)">删除</button>
                        <button class="minor" @click="toClipBoard($event,subdir.Id,subdir.Name,'fileDir')">移动</button>
                    </Functions>
                </div>
                <div>
                </div>
            </div>
            <div class="detail" v-if="subdir.showChildren">
                <FileDirChild :dir-id="subdir.Id" :path="_.concat(props.path, subdir.UrlPathName)" :fetch-from="props.fetchFrom">
                </FileDirChild>
            </div>
        </div>
        <FileDirItems :dir-id="props.dirId" :items="items" :wikis="wikis" @need-refresh="loadData" @beforeJumpToWiki="beforeJumpWiki" :compact="true"></FileDirItems>
        <div v-if="isEmptyDir()" class="emptyDir">
            <Loading v-if="showLoading"></Loading>
            <div v-else>空文件夹</div>
        </div>
        <div v-if="more" @click="moreJump" class="emptyDir more">点击查看更多</div>
    </div>
</template>

<style scoped>
.subdirName{
    font-weight: bold;
}
.subdirName:hover{
    text-decoration: underline;
    cursor: pointer;
}
.emptyDir{
    margin-top: 10px;
    text-align: left;
    color:#666;
    font-size: small;
}
.more:hover{
    text-decoration: underline;
    cursor: pointer;
}
.foldBtn{
    width: 20px;
    overflow: visible;
    cursor: pointer;
    user-select: none;
}
.fileDirChild{
    padding-left: 5px;
    position: relative;
}
.detail{
    display: flex;
    flex-direction: column;
    gap:5px;
    padding-bottom: 5px;
    border-left: 1px solid black;
    border-bottom: 1px solid black;
    margin-left: 11px;
    margin-bottom: 5px;
    padding-left: 4px;
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
    justify-content: left;
    gap:20px;
    align-items: center;
    padding: 4px;
    transition: 0.5s;
    height: 20px;
}
.subdir:hover{
    background-color: white;
}
</style>