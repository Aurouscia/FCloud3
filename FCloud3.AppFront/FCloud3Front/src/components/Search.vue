<script setup lang="ts">
import { onMounted, onUnmounted, ref } from 'vue';
import { QuickSearchResult, QuickSearchResultItem } from '../models/sys/quickSearch';

const props = defineProps<{
    placeholder?:string,
    allowFreeInput?:boolean|undefined,
    noResultNotice?:string,
    source:(s:string)=>Promise<QuickSearchResult|undefined>,
    dontClearAfterDone?:boolean,
    compact?:boolean
}>()

const doneBtnStatus = ref<boolean>(false);
const isFreeInput = ref<boolean>(false);
const searching = ref<string>("");
const selectedId = ref<number>(0);
const cands = ref<QuickSearchResult>();
const isSearching = ref<boolean>(false);
const inputing = ref<boolean>(false);
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
        clear()
        return;
    }
    inputing.value = true;
    window.clearTimeout(timer);
    timer = window.setTimeout(async()=>{
        emits('stopInput',searching.value);
        inputing.value = false;
        isSearching.value = true;
        if(cands.value)
            cands.value.Items = [];
        selectCand.value = -1;
        cands.value = await props.source(searching.value);
        isSearching.value = false;
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
        if(!props.dontClearAfterDone){
            clear()
        }
    }
}
function clear(){
    searching.value = "";
    selectedId.value = 0;
    doneBtnStatus.value = false;
    cands.value = undefined;
    selectCand.value = -1;
}

const emits = defineEmits<{
    (e:'done',value:string,id:number):void
    (e:'stopInput',value:string):void
}>();
defineExpose({clear});


const selectCand = ref<number>(-1);
function keyEventHandler(e:KeyboardEvent){
    if(!cands.value?.Items){
        return;
    }
    const key = e.key;
    if(key == 'ArrowUp' && selectCand.value>0){
        selectCand.value -= 1;
        e.preventDefault();
    }
    else if(key == 'ArrowDown' && selectCand.value < cands.value.Items.length - 1){
        selectCand.value += 1;
        e.preventDefault();
    }
    else if(key == 'Enter'){
        if(selectCand.value >= 0 && selectCand.value < cands.value.Items.length){
            const item = cands.value.Items[selectCand.value];
            if(item){
                clickCand(item)
            }
        }
        else if(doneBtnStatus.value){
            done()
        }
    }
}

onMounted(()=>{
    cands.value = {
        Items:[],
        DescIsSrc:false
    }
    window.addEventListener('keydown', keyEventHandler);
})
onUnmounted(()=>{
    window.removeEventListener('keydown',keyEventHandler);
})
</script>

<template>
    <div class="search" :class="{compact}">
        <div class="write">
            <input v-model="searching" @input="refreshCand" :placeholder="props.placeholder" />
            <button class="confirm" :class="{ disabled: !doneBtnStatus }" @click="done">确认</button>
        </div>
        <div v-if="cands && searching" class="cand">
            <div class="candItem" v-for="c,idx in cands.Items" @click="clickCand(c)" :class="{selected:idx == selectCand}">
                <div v-if="!cands.DescIsSrc">
                    {{ c.Name }}
                    <div class="desc">{{ c.Desc }}</div>
                </div>
                <div v-else class="candItemImage">
                    <img :src="c.Desc"/>
                    <div>{{ c.Name }}</div>
                </div>
            </div>
            <div class="noResult" v-show="isFreeInput && props.allowFreeInput && props.noResultNotice">{{
                props.noResultNotice }}</div>
            <div class="noResult" v-show="isFreeInput && !props.allowFreeInput && cands.Items.length == 0">
                <div v-if="inputing">请继续输入...</div>
                <div v-else-if="isSearching">搜索中...</div>
                <div v-else>没有匹配结果</div>
            </div>
        </div>
    </div>
</template>

<style scoped lang="scss">
.desc{
    font-size: 10px;
    color:#888
}
.candItem:hover{
    background-color: #ccc;
}
.candItemImage{
    display: flex;
    gap:5px;
    align-items: center;
    justify-content: left;
}
.candItemImage img{
    height: 25px;
    max-width: 40px;
    object-fit: contain;
}
.candItem,.noResult{
    border-bottom: 1px solid #aaa;
    padding: 5px;
    transition: 0.5s;
    cursor: pointer;
    user-select: none;
    transition: 0.1s;
}
.candItem.selected{
    background-color: cornflowerblue;
    color:white
}
.noResult{
    color:#999;
    background-color: white;
    cursor:default;
}
.cand{
    position: absolute;
    top:32px;
    left:10px;right:50px;
    box-shadow: 0px 0px 6px 0px black;
    background-color: white;
    z-index: 200;
}
.write button.disabled{
    background-color: #aaa;
}
.write button{
    white-space: nowrap;
    border-radius: 0px 5px 5px 0px;
    padding: 5px;
    margin: 0px;
    height: 100%;
    transition: 0.5s;
}
.write input{
    border:2px solid cornflowerblue;
    border-radius: 5px 0px 0px 5px;
    padding: 4px;
    margin:0px;
    height: 100%;
    display: block;
}
.write{
    display: flex;
    flex-wrap: nowrap;
    align-items: center;
    justify-content: center;
    gap:0px;
    position: relative;
    z-index: 50;
}
.search{
    position: relative;
}
.search.compact{
    input{
        width: 150px;
    }
    width: 210px;
}
</style>