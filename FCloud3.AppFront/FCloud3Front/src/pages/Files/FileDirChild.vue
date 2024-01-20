<script setup lang="ts">
import { Ref, inject, onMounted, ref } from 'vue';
import { TakeContentResult } from '../../models/files/fileDir';
import FileDirChild from './FileDirChild.vue'
import { useRouter } from 'vue-router';
import _ from 'lodash';
import FileDirItems from './FileDirItems.vue';
import ClipBoard, { ClipBoardItemType } from '../../components/ClipBoard.vue';
import Functions from '../../components/Functions.vue';
import Loading from '../../components/Loading.vue';

const router = useRouter();

function jumpToSubDir(name:string){
    var path =  _.concat(props.path,name);
    path = _.filter(path, x=>!!x)
    router.replace({name:'files',params:{path}});
}
function isEmptyDir(){
    return data.value?.SubDirs.length==0 && data.value?.Items.length==0;
}

const props = defineProps<{
    dirId:number,
    path:string[]|string,
    fetchFrom:(dir:number)=>Promise<TakeContentResult|undefined>
}>();

const data = ref<TakeContentResult>();
const showLoading = ref<boolean>(false);
var clip:Ref<InstanceType<typeof ClipBoard>>
onMounted(async()=>{
    clip = inject('clipboard') as Ref<InstanceType<typeof ClipBoard>>;
    var timer = window.setTimeout(()=>{
        showLoading.value = true;
    },800);
    data.value = await props.fetchFrom(props.dirId);    
    if(data.value){
        window.clearTimeout(timer);
        showLoading.value = false;
    }
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
        <div v-for="subdir in data?.SubDirs" :key="subdir.Id">
            <div class="subdir">
                <div>
                    <div class="foldBtn" v-show="!subdir.showChildren" @click="subdir.showChildren = true" style="color:#999">▶
                    </div>
                    <div class="foldBtn" v-show="subdir.showChildren" @click="subdir.showChildren = false" style="color:black">▼
                    </div>
                    <div class="subdirName" @click="jumpToSubDir(subdir.Name)">{{ subdir.Name }}</div>
                    <Functions :entry-size="20" x-align="left">
                        <button class="minor" @click="toClipBoard($event,subdir.Id,subdir.Name,'fileDir')">移动</button>
                        <button class="danger">删除</button>
                    </Functions>
                </div>
                <div>
                </div>
            </div>
            <div class="detail" v-if="subdir.showChildren">
                <FileDirChild :dir-id="subdir.Id" :path="_.concat(props.path, subdir.Name)" :fetch-from="props.fetchFrom">
                </FileDirChild>
            </div>
        </div>
        <FileDirItems :items="data?.Items"></FileDirItems>
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