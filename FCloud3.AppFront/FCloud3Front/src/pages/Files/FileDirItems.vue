<script setup lang="ts">
import { FileDirItem, FileDirWiki} from '@/models/files/fileDir';
import { fileSizeStr, getFileIconStyle, getFileExt } from '@/utils/fileUtils';
import ClipBoard, { ClipBoardItemType } from '@/components/ClipBoard.vue';
import { Ref, inject, onMounted } from 'vue';
import FileItemEdit from './FileItemEdit.vue';
import Functions from '@/components/Functions.vue';
import { Api } from '@/utils/com/api';
import { useRouter } from 'vue-router';
import _ from 'lodash';
import { useWantViewWikiStore } from '@/utils/globalStores/wantViewWiki';

const props = defineProps<{
    dirId:number,
    items?: Array<FileDirItem>|undefined,
    wikis?: Array<FileDirWiki>|undefined,
    compact?:boolean
}>()
const router = useRouter();
const wantViewWikiStore = useWantViewWikiStore();

var clipBoard:Ref<InstanceType<typeof ClipBoard>>
var api:Api;
function toClipBoard(e:MouseEvent, item:FileDirItem|FileDirWiki, type:ClipBoardItemType){
    clipBoard.value.insert({
        id:item.Id,
        name:item.Name,
        type:type
    },e)
}

function jumpToWiki(urlPathName:string){
    //告知包含本组件的父组件想要跳转到词条
    //如果父组件是个FileDirChild（列表展开文件夹）
    //    那么其会使用router.push跳转到自身对应的文件夹，再取出想要跳转的词条名
    //如果父组件是个FileDirIndex（文件夹主视图）
    //    那么其会直接取出要跳转的词条名跳转
    wantViewWikiStore.set(urlPathName)
    emit('beforeJumpToWiki')
}
async function removeWiki(id:number){
    if(!props.dirId){return}
    await api.wiki.removeFromDir(id,props.dirId)
    emit('needRefresh');
}
function jumpToWikiEdit(urlPathName:string){
    router.push({name:'wikiEdit',params:{urlPathName:urlPathName}})
}

function editFile(f:FileDirItem){
    editPanel.value.startEditingFile(f, (newName:string|-1)=>{
        if(typeof(newName) == 'string')
            f.Name = newName;
        else{
            if(props.items){
                const delIdx = props.items.findIndex(x=>x.Id == f.Id);
                _.pullAt(props.items, delIdx)
            }
        }
    });
}
function downloadHref(fileId:number){
    return import.meta.env.VITE_BASEURL + '/api/FileItem/Download?id=' + fileId
}

let editPanel: Ref<InstanceType<typeof FileItemEdit>>
onMounted(()=>{
    clipBoard = inject('clipboard') as Ref<InstanceType<typeof ClipBoard>>;
    api = inject('api') as Api;
    editPanel = inject('fileItemEdit') as Ref<InstanceType<typeof FileItemEdit>>
})
const emit = defineEmits<{
    (e:'needRefresh'):void,
    (e:'beforeJumpToWiki'):void
}>()
</script>

<template>
<div v-if="props.items" class="dirItems" :class="{compact}">
    <div class="item" v-for="wiki in props.wikis" :key="wiki.Id">
        <div class="iconName">
            <div class="wikiIcon">W</div>
            <div class="name">
                <a @click="jumpToWiki(wiki.UrlPathName)">{{ wiki.Name }}</a> 
            </div>
            <Functions x-align="left" y-align="up" :entry-size="20">
                <button class="cancel" @click="removeWiki(wiki.Id)">移出</button>
                <button class="minor" @click="toClipBoard($event,wiki,'wikiItem')">移动</button>
                <button class="confirm" @click="jumpToWikiEdit(wiki.UrlPathName)">编辑</button>
            </Functions>
        </div>
        <div class="size">
        </div>
    </div>
    <div class="item" v-for="item in props.items" :key="item.Id">
        <div class="iconName">
            <div class="icon" :style="getFileIconStyle(item.Name)">{{ getFileExt(item.Name) }}</div>
            <div class="name">
                <a :href="item.Url" target="_blank">{{ item.Name }}</a> 
            </div>
            <Functions x-align="left" y-align="up" :entry-size="20">
                <button class="minor" @click="toClipBoard($event,item,'fileItem')">移动</button>
                <button @click="editFile(item)">设置</button>
                <a :href="downloadHref(item.Id)" download target="_blank" class="downloadBtn">下载</a>
            </Functions>
        </div>
        <div class="size">
            {{ fileSizeStr(item.ByteCount) }}
        </div>
    </div>
</div>
</template>

<style scoped lang="scss">
.size{
    font-size: 15px;
    color: #666
}
.downloadBtn{
    display: block;
    background-color: green;
    color:white;
    padding: 5px;
    margin: 2px;
    border-radius: 5px;
    border: none;
    font-size: unset;
    margin-bottom: 2px;
    transition: 0.4s;
    cursor: pointer;
    &:hover{
        background-color: darkgreen;
        text-decoration: none;
    }
}
.itemPanelEntry:hover{
    background-color: #ccc;
}
.itemPanelEntry{
    transition: 0.5s;
    cursor: pointer;
    padding: 0px 5px 0px 5px;
}
.itemPanelContainer{
    position: relative;
}
.itemPanel img:hover{
    border-color: #ccc;
}
.itemPanel img{
    width: 90px;
    height: 90px;
    border-radius: 5px;
    object-fit: cover;
    border:2px solid white;
    transition: 0.5s;
}
.itemPanel{
    position: absolute;
    left: -10px;
    width: 100px;
    background-color: white;
    padding: 5px;
    border:2px solid black;
    border-radius: 5px;
    cursor:default;
    z-index: 100;
}
.iconName{
    display: flex;
    align-items: center;
    gap:5px;
    flex-grow: 1;
    height: 100%;
}
.name{
    flex-grow: 0;
    flex-shrink: 0;
    text-align: left;
}
.item:hover{
    background-color: white;
}
.item{
    display: flex;
    flex-direction: row;
    justify-content:space-between;
    align-items: center;
    gap:10px;
    padding: 5px;
    transition: 0.3s;
    height: 30px;
}
.compact .item{
    height: unset;
}
.dirItems{
    margin-top: 0px;
}
</style>