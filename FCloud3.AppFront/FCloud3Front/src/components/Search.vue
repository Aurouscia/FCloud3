<script setup lang="ts">
import { inject, onMounted, ref } from 'vue';
import { Api } from '../utils/api';
import { QuickSearchResult, QuickSearchResultItem } from '../models/sys/quickSearch';

const props = defineProps<{
    placeholder?:string,
    allowFreeInput?:boolean|undefined,
    noResultNotice?:string,
    source:(s:string)=>Promise<QuickSearchResult|undefined>
}>()

const doneBtnStatus = ref<boolean>(false);
const isFreeInput = ref<boolean>(false);
const searching = ref<string>("");
const selectedId = ref<number>(0);
const cands = ref<QuickSearchResult>();
var timer:number = 0;
const delay:number = 500;
function refreshCand(){
    selectedId.value = 0;
    isFreeInput.value = true;
    if(!props.allowFreeInput || !searching.value){
        doneBtnStatus.value = false;
    }else{
        doneBtnStatus.value = true;
    }
    if(!searching.value){
        return;
    }
    window.clearTimeout(timer);
    timer = window.setTimeout(async()=>{
        emits('stopInput',searching.value);
        cands.value = await props.source(searching.value);
    },delay)
}
function clickCand(c:QuickSearchResultItem){
    searching.value = c.Name;
    selectedId.value = c.Id;
    doneBtnStatus.value = true;
    isFreeInput.value = false;
    if(cands.value){
        cands.value.Items = [];
    }
}
function done(){
    if(doneBtnStatus.value){
        emits('done',searching.value,selectedId.value);
    }
}
function clear(){
    searching.value = "";
    selectedId.value = 0;
    doneBtnStatus.value = false;
    cands.value = undefined;
}

const emits = defineEmits<{
    (e:'done',value:string,id:number):void
    (e:'stopInput',value:string):void
}>();
defineExpose({clear});

var api:Api;
onMounted(()=>{
    api = inject('api') as Api;
})
</script>

<template>
<div class="search">
    <div class="write">
        <input v-model="searching" @input="refreshCand" :placeholder="props.placeholder"/>
        <button class="confirm" :class="{disabled:!doneBtnStatus}" @click="done">确认</button>
    </div>
    <div v-if="cands && searching" class="cand">
        <div class="candItem" v-for="c in cands.Items" @click="clickCand(c)">
            {{ c.Name }}
            <div class="desc">{{ c.Desc }}</div>
        </div>
            <div class="noResult" v-show="isFreeInput && props.allowFreeInput && props.noResultNotice">{{ props.noResultNotice }}</div>
            <div class="noResult" v-show="isFreeInput && !props.allowFreeInput && cands.Items.length==0">
                没有匹配结果
            </div>
    </div>
</div>
</template>

<style scoped>
.desc{
    font-size: 10px;
    color:#888
}
.candItem:hover{
    background-color: #ccc;
}
.candItem,.noResult{
    border-bottom: 1px solid #aaa;
    padding: 5px;
    transition: 0.5s;
    cursor: pointer;
    user-select: none;
}
.noResult{
    color:#999;
    background-color: white;
    cursor:default;
}
.cand{
    position: absolute;
    top:30px;
    left:10px;right:50px;
    box-shadow: 0px 0px 5px 0px;
    background-color: white;
}
.write button.disabled{
    background-color: #aaa;
}
.write button{
    white-space: nowrap;
    border-radius: 0px 5px 5px 0px;
    padding: 5px;
    margin: 0px;
    transition: 0.5s;
}
.write input{
    border:2px solid cornflowerblue;
    border-radius: 5px 0px 0px 5px;
    flex-grow: 1;
    padding: 4px;
    margin:0px;
    height: 19px;
    display: block;
}
.write{
    display: flex;
    flex-wrap: nowrap;
    align-items: center;
    gap:0px;
    position: relative;
    z-index: 50;
}
.search{
    position: relative;
}
</style>