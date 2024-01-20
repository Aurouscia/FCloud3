<script setup lang="ts">
import _ from 'lodash';
import { CSSProperties, ref } from 'vue';

const props = defineProps<{
    currentDir:string
}>();

export type ClipBoardItemType = "fileDir"|"fileItem"
export interface ClipBoardItem{
    id:number,
    name:string,
    type:ClipBoardItemType
}
export type ClipBoardCallBack = (item:ClipBoardItem,clickE:MouseEvent)=>void;
const showBody = ref<boolean>(false);
const list = ref<Array<ClipBoardItem & {highlight?:boolean}>>([]);
function insert(item:ClipBoardItem,clickE:MouseEvent){
    const existing = list.value.find(x=>isSameItem(x,item))
    if(existing){
        var delay = 200;
        if(showBody.value){
            delay = 0;
        }
        showBody.value = true;
        setTimeout(()=>{
            existing.highlight = true;
            setTimeout(()=>{existing.highlight=false},1000)
        },delay)
    }
    else{
        list.value.push(item);
        playGuideAnim(clickE.pageX,clickE.pageY);
    }
}
function clear(){
    list.value = []
}
const movingThingStyle = ref<CSSProperties>();
function playGuideAnim(fromX:number,fromY:number){
        movingThingStyle.value = {
            opacity:1,
            display:'block',
            left: fromX+'px',
            top: fromY+'px',
            transition:'0.7s'
        }
    window.setTimeout(()=>{
        movingThingStyle.value = {
            display:'block',
            left:'10px',
            top: (window.innerHeight-70)+'px',
            transition:'0.7s'
        };
    },10)
    window.setTimeout(()=>{
        movingThingStyle.value = {
            display:'block',
            transition:'0.3s',
            left:'10px',
            top: (window.innerHeight-70)+'px',
            opacity:0
        }
    },700)
    window.setTimeout(()=>{
        movingThingStyle.value={}
    },1000)
}

const failMsg = ref<string|undefined>();
function putDownOne(item:ClipBoardItem){
    emit('putDown',[item],putEmitCallBackHandler)
}
function putDownAll(){
    emit('putDown',list.value,putEmitCallBackHandler)
}
function putEmitCallBackHandler(success:ClipBoardItem[],failMessage?:string){
    if(success){
        list.value = _.pullAllWith(list.value,success,isSameItem);
        failMsg.value = failMessage
    }else{
        failMsg.value = "操作失败"
    }
}

function isSameItem(x:ClipBoardItem,y:ClipBoardItem):boolean{
    return x.type==y.type&&x.id==y.id
}

export type PutEmitCallBack = (success:ClipBoardItem[],failMsg?:string)=>void
defineExpose({insert,clear})
const emit = defineEmits<{
    (e: 'putDown', item:ClipBoardItem[], callBack:PutEmitCallBack):void
}>()
</script>

<template>
<div v-if="list.length>0" class="clipBoard">
    <div v-show="showBody" class="list">
        <div class="notice">请导航到目标文件夹(点名字点进去)</div>
        <div v-if="currentDir" class="notice">
            点击"拿出"将把对象移动到<b>{{props.currentDir}}</b>
        </div>
        <div v-else-if="list.some(x=>x.type!='fileDir')" class="notice" style="color:red">当前位置只能拿出文件夹</div>
        <div class="listItem" v-for="item in list" :key="item.id" :style="{backgroundColor:item.highlight?'yellow':''}">
            <div>{{ item.name }}</div>
            <div class="putDown" @click="putDownOne(item)">拿出</div>
        </div>
        <div class="quickBtns">
            <div class="clearBtn" @click="clear">清空(保留原位)</div>
            <div class="putDown" @click="putDownAll">全部拿出</div>
        </div>
        <div v-if="failMsg" style="color:red;text-align: center;">{{ failMsg }}</div>
    </div>
    <div class="head" @click="showBody=!showBody">
        剪切板<b>({{ list.length }}个物品)</b> <span v-show="showBody" style="font-size: small;">点击收起</span>
    </div>
</div>
<div ref="movingThing" class="movingThing" :style="movingThingStyle">文件</div>
</template>

<style scoped>
.quickBtns{
    display: flex;
    justify-content: space-between;
    align-items: center;
}
.clearBtn:hover{
    text-decoration: underline;
}
.clearBtn{
    cursor: pointer;
    color:plum;
    text-align: center;
    margin: 10px;
}
.movingThing{
    background-color: plum;
    color:white;
    border: 2px solid white;
    padding: 5px;
    position: fixed;
    display: none;
    z-index: 1000;
}
.putDown:hover{
    color: black;
    font-weight: bold;
}
.putDown{
    color:#666;
    text-decoration: underline;
    transition: 0.5s;
    cursor: pointer;
}
.head{
    cursor: pointer;
    user-select: none;
    text-decoration: underline;
}
.listItem{
    margin: 2px;
    padding: 3px;
    color:#333;
    transition: 0.3s;
    user-select: none;
    display: flex;
    justify-content: space-between;
    gap:10px;
    border-bottom: 1px solid #333;
}
.listItem:hover{
    background-color: #ccc;
}
.list{
    background-color: white;
    padding: 5px;
    margin: 5px;
    border-radius: 5px;
}
.clipBoard{
    background-color: plum;
    color:white;
    padding: 5px;
    position: fixed;
    left:10px;
    bottom:20px;
    box-shadow: 0px 0px 5px 0px black;
    z-index: 100;
}
.notice{
    color:#aaa;
    font-size: small;
}
</style>