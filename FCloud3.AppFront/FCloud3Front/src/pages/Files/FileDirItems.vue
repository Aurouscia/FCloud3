<script setup lang="ts">
import { FileDirItem, FileDirWiki} from '../../models/files/fileDir';
import { fileSizeStr, getFileIconStyle, getFileExt} from '../../utils/fileUtils';
import ClipBoard, { ClipBoardItemType } from '../../components/ClipBoard.vue';
import { Ref,inject, onMounted } from 'vue';
import Functions from '../../components/Functions.vue';
import { Api } from '../../utils/api';
import { useRouter } from 'vue-router';

const props = defineProps<{
    dirId:number,
    items?: Array<FileDirItem>|undefined,
    wikis?: Array<FileDirWiki>|undefined
}>()
const router = useRouter();

var clipBoard:Ref<InstanceType<typeof ClipBoard>>
var api:Api;
function toClipBoard(e:MouseEvent, item:FileDirItem|FileDirWiki, type:ClipBoardItemType){
    clipBoard.value.insert({
        id:item.Id,
        name:item.Name,
        type:type
    },e)
}
async function removeWiki(id:number){
    if(!props.dirId){return}
    await api.wiki.removeFromDir(id,props.dirId)
    emit('needRefresh');
}
function jumpToWikiEdit(urlPathName:string){
    router.push({name:'wikiEdit',params:{urlPathName:urlPathName}})
}
onMounted(()=>{
    clipBoard = inject('clipboard') as Ref<InstanceType<typeof ClipBoard>>;
    api = inject('api') as Api;
})
const emit = defineEmits<{
    (e:'needRefresh'):void
}>()
</script>

<template>
<div v-if="props.items" class="dirItems">
    <div class="item" v-for="wiki in props.wikis" :key="wiki.Id">
        <div class="iconName">
            <div class="wikiIcon">W</div>
            <div class="name">
                <a href="#" >{{ wiki.Name }}</a> 
            </div>
            <Functions x-align="left" :entry-size="20">
                <button class="confirm" @click="jumpToWikiEdit(wiki.UrlPathName)">编辑</button>
                <button class="minor" @click="toClipBoard($event,wiki,'wikiItem')">移动</button>
                <button class="cancel" @click="removeWiki(wiki.Id)">移出</button>
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
            <Functions x-align="left" :entry-size="20">
                <button class="minor" @click="toClipBoard($event,item,'fileItem')">移动</button>
                <button class="danger">删除</button>
            </Functions>
        </div>
        <div class="size">
            {{ fileSizeStr(item.ByteCount) }}
        </div>
    </div>
</div>
</template>

<style scoped>
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
}
.wikiIcon{
    width: 20px;
    height: 20px;
    line-height: 20px;
    border-radius: 3px;
    text-align: center;
    background-color: white;
    border:1px solid black;
    color:black;
    font-family: 'Times New Roman', Times, serif;
    font-size: 15px;
}
.icon{
    width: 20px;
    height: 20px;
    line-height: 20px;
    text-align: center;
    border: 1px solid white;
    border-radius: 3px;
    font-size: 10px;
    color:white;
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
}
.dirItems{
    margin-top: 5px;
}
</style>