<script setup lang="ts">
import { Ref, inject, onMounted, ref } from 'vue';
import FileDirChild from './FileDirChild.vue'
import { useRouter } from 'vue-router';
import _ from 'lodash';
import FileDirItems from './FileDirItems.vue';
import ClipBoard, { ClipBoardItemType } from '../../components/ClipBoard.vue';
import Functions from '../../components/Functions.vue';
import Loading from '../../components/Loading.vue';
import { FileDirIndexResult, FileDirItem, FileDirSubDir, FileDirWiki } from '../../models/files/fileDir';
import { IndexQuery, IndexResult, unlimitedIndexQueryDefault } from '../../components/Index';
import { Api } from '../../utils/api';

const router = useRouter();

function jumpToSubDir(name:string){
    var path =  _.concat(props.path,name);
    path = _.filter(path, x=>!!x)
    router.push({name:'files',params:{path}});
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
//TODO：这三个难看的玩意是不是可以改进一下
function renderItems(i:IndexResult|undefined){
    items.value = [];
    i?.Data?.forEach(r=>{
        items.value.push({
            Id:parseInt(r[0]),
            Name:r[1],
            Updated:r[2],
            OwnerName:r[3],
            ByteCount:parseInt(r[4]),
            Url:r[5]
        })
    })
}
function renderSubdirs(i:IndexResult|undefined){
    subDirs.value = [];
    i?.Data?.forEach(r=>{
        subDirs.value?.push({
            Id: parseInt(r[0]),
            Name:r[1],
            UrlPathName:r[2],
            Updated:r[3],
            OwnerName:r[4],
            ByteCount:parseInt(r[5]),
            FileNumber:parseInt(r[6])
        })
    })
}
function renderWikis(i:IndexResult|undefined){
    wikis.value = [];
    i?.Data?.forEach(r=>{
        wikis.value?.push({
            Id: parseInt(r[0]),
            Name:r[1],
            UrlPathName:r[2],
            Updated:r[3],
            OwnerName:r[4],
        })
    })
}

const props = defineProps<{
    dirId:number,
    path:string[]|string,
    fetchFrom:(q:IndexQuery, path:string[])=>Promise<FileDirIndexResult|undefined>
}>();

//const data = ref<FileDirIndexResult>();
const showLoading = ref<boolean>(false);
var clip:Ref<InstanceType<typeof ClipBoard>>
var api:Api;

async function loadData(){
    var path = _.filter(props.path, x=>!!x)
    var timer = window.setTimeout(()=>{
        showLoading.value = true;
    },800);
    var data = await props.fetchFrom(unlimitedIndexQueryDefault, path);    
    if(data){
        window.clearTimeout(timer);
        showLoading.value = false;
        renderItems(data.Items);
        renderSubdirs(data.SubDirs);
        renderWikis(data.Wikis);
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
                    <Functions :entry-size="20" x-align="left">
                        <button class="minor" @click="toClipBoard($event,subdir.Id,subdir.Name,'fileDir')">移动</button>
                        <button class="danger" @click="deleteDir(subdir.Id)">删除</button>
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
        <FileDirItems :dir-id="props.dirId" :items="items" :wikis="wikis" @need-refresh="loadData"></FileDirItems>
        <div v-if="isEmptyDir()" class="emptyDir">
            空文件夹
        </div>
        <Loading v-if="showLoading"></Loading>
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
    color:#999;
    font-size: small;
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
    padding-bottom: 10px;
    border-left: 1px solid black;
    border-bottom: 1px solid black;
    margin-left: 11px;
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
}
.subdir:hover{
    background-color: white;
}
</style>