<script setup lang="ts">
import { computed, onMounted, ref } from 'vue';
import { IndexQuery, IndexResult, indexQueryDefault } from './index.ts';
import Loading from '../Loading.vue';

export interface IndexColumn{
    name:string,
    alias:string,
    canSetOrder:boolean,
    canSearch:boolean,
    type?:"str"|"int"|"bool"|"img"|undefined,

    editing?:boolean|undefined,
    searchText?:string|undefined
}
const props = defineProps<{
    fetchIndex:(q:IndexQuery)=>Promise<IndexResult|undefined>,
    columns:IndexColumn[],
    displayColumnCount:number,
    qInit?:IndexQuery,

    hidePage?:boolean|undefined,
    hideHead?:boolean|undefined,
    hideFn?:boolean|undefined
}>();
const i = ref<IndexResult>();

var pageSizeOverride:number = 0;
function setPageSizeOverride(size:number){
    pageSizeOverride = size;
}

const hide = ref<boolean>(false);
async function reloadData(){
    const searchStrs:string[] = [];
    const searchStrs_alias:string[] = [];
    cols.value.forEach(x=>{
        if(x.searchText){
            searchStrs.push(`${x.name}=${x.searchText}`);
            searchStrs_alias.push(`${x.alias}=${x.searchText}`);
        }
    })
    query.value.Search = searchStrs;
    searchStrsAlias.value = searchStrs_alias;
    if(pageSizeOverride && pageSizeOverride>=5){
        query.value.PageSize = pageSizeOverride;
    }
    var timer = window.setTimeout(()=>hide.value = true,300);
    const resp = await props.fetchIndex(query.value||indexQueryDefault)
    if(resp){
        window.clearTimeout(timer);
        hide.value = false;
        i.value = resp;
        emit('reloadData',i.value);
    }
}
async function changePage(direction:"prev"|"next"){
    if(i.value && query.value){
        var p = query.value.Page || 0;
        if(direction=='next'){
            p+=1;
        }else{
            p-=1;
        }
        if(p<=0){
            p=1;
        }
        if(p>i.value.PageCount){
            p = i.value.PageCount;
        }
        if(p!=query.value.Page){
            query.value.Page=p;
            await reloadData();
        }
    }
}
async function setOrder(by:string, rev:boolean) {
    const target = cols.value.find(x=>x.name==by);
    if(!target){return;}
    if(query.value){
        query.value.OrderBy = target.name;
        query.value.OrderRev = rev;
        orderByAlias.value = target.alias;
        await reloadData()
    }
}
async function setSearch(colName:string) {
    const target = cols.value.find(x=>x.name==colName);
    if(!target){return;}
    if(target.searchText?.trim()){
        
    }else{
        target.searchText = "";
        target.editing = false;
    }
    reloadData();
}
async function clearSearch() {
    cols.value.forEach(x=>{x.searchText="";x.editing=false});
    searchPanelOpen.value = false;
    await reloadData();
}
async function clearOrder() {
    query.value.OrderBy = undefined;
    orderPanelOpen.value = false;
    reloadData();
}

const emit = defineEmits<{
    (e: 'reloadData', value: IndexResult): void
}>()
defineExpose({reloadData,setPageSizeOverride})

const query = ref<IndexQuery>(indexQueryDefault)
const searchStrsAlias = ref<string[]>([]);
const orderByAlias = ref<string>("");
const cols = ref<IndexColumn[]>([]);
const search = ref<HTMLInputElement[]>([]);

const searchPanelOpen = ref<boolean>(false);
const orderPanelOpen = ref<boolean>(false);
onMounted(async()=>{
    if(props.qInit){
        query.value = props.qInit;
        if(!query.value.Page){
            query.value.Page=1;
        }
    }
    cols.value = props.columns;
    await reloadData();
})

const highlightSearchBtn = computed<boolean>(()=>{
    if(searchPanelOpen.value){return true;}
    if(!query.value.Search){return false;}
    if(query.value.Search.length==0){return false;}
    if(query.value.Search.every(x=>!x)){return false;}
    return true;
})
const highlightOrderBtn = computed<boolean>(()=>orderPanelOpen.value || !!query.value.OrderBy)
</script>

<template>
<table class="index" v-if="query && i && !hide">
    <tbody>
        <tr v-if="!props.hideHead">
            <th :colspan="props.displayColumnCount">
                <div class="indexControl">
                    <div v-if="!props.hidePage" class="pageControl">
                        <button class="queryAdjust" @click="changePage('prev')" :class="{highlight:query.Page && query.Page>1}">◀</button>
                        第{{ query.Page }}页
                        <button class="queryAdjust" @click="changePage('next')" :class="{highlight:query.Page && query.Page<i.PageCount}">▶</button>
                    </div>
                    <div v-else>　</div>
                    <div v-if="!hideFn">
                        <button class="tabToggle" 
                            @click="searchPanelOpen=!searchPanelOpen;orderPanelOpen=false"
                            :class="{highlightToggle:highlightSearchBtn}">
                            {{ (query.Search && query.Search.length>0)?'已筛选':'搜索' }}
                        </button>
                        <button class="tabToggle"
                            @click="orderPanelOpen=!orderPanelOpen;searchPanelOpen=false"
                            :class="{highlightToggle:highlightOrderBtn}">
                            {{ query.OrderBy?'已排序':'排序' }}
                        </button>
                    </div>
                    <div v-if="searchPanelOpen" class="searchPanel">
                        <div  v-for="c in cols">
                            <div v-if="c.canSearch" class="searchItem">
                                {{ c.alias }}
                                <input class="search" v-model="c.searchText" ref="search" @blur="setSearch(c.name)" placeholder="搜索">
                            </div>
                        </div>
                        <div class="searchPanelBtns">
                            <button class="cancel" @click="clearSearch">清空</button>
                            <button class="ok" @click="searchPanelOpen=false">OK</button>
                        </div>
                    </div>
                    <div v-if="orderPanelOpen" class="orderPanel">
                        <div  v-for="c in cols">
                            <div v-if="c.canSetOrder" class="orderItem">
                                <div>{{ c.alias }}</div>
                                <div>
                                    <button class="queryAdjust" @click="setOrder(c.name,true)" :class="{highlight:query.OrderBy==c.name && query.OrderRev}">▲</button>
                                    <button class="queryAdjust" @click="setOrder(c.name,false)" :class="{highlight:query.OrderBy==c.name && !query.OrderRev}">▼</button>
                                </div>
                            </div>
                        </div>
                        <button class="cancel" @click="clearOrder">清空</button>
                    </div>
                </div>
            </th>
        </tr>
        <slot></slot>
    </tbody>
</table>
<div v-else>
    <Loading></Loading>
</div>
</template>

<style scoped>
.orderItem button.highlight{
    color:black
}
.orderItem{
    display: flex;
    flex-direction: row;
    justify-content: space-between;
    border-bottom: 1px solid #999;
    margin: 5px;gap:10px;
    font-weight: normal;
}
.searchPanelBtns button,.orderPanel button{
    width: 4em;
    height: 1.8em;
    line-height: 1.8em;
    padding: 0px;
    cursor: pointer;
}
.searchPanelBtns{
    display: flex;
    flex-direction: row;
}
.searchItem{
    font-weight:normal;
    margin: 2px;
    padding: 2px;
    display: flex;
    justify-content: space-between;
    gap:3px;
    border-bottom: 1px solid #999;
}
.searchPanel,.orderPanel{
    position: absolute;
    top:2em;
    right:0px;
    background-color: white;
    border: 2px solid black;
    color:black;
    border-radius: 5px;
    z-index: 1000;
}
.pageControl{
    padding: 4px;
    background-color: #bbb;
}
.order{
    margin: 0px 0px 0px 5px;
}
.search{
    width: 80px;
    margin: 0px;
    border: 1px solid #444;
    border-radius: 5px;
    text-align: center;
}
.miniInput{
    width: 1.5em;text-align: center;margin: 0px;background:white
}
button.highlightToggle{
    color:black !important;
    font-weight: bold;
}
button.tabToggle{
    background-color: white;
    color:#bbb;
    padding: 0px 3px 0px 3px;
}
button.clearSearch{
    background:none;
    margin: 0px 0px 0px 5px;
    padding: 0px;
}
button.queryAdjust{
    background-color: transparent;
    width: fit-content;
    padding: 0px;
    margin: 0px;
    color: #ccc;
}
button.highlight{
    color:white
}
th{
    background-color: white;
    position: relative;
    height: 2em;
}
th *{
    white-space: nowrap;
}
.indexControl{
    border: 2px #bbb solid;
    position: absolute;
    left: 0px;right: 0px;top: 0px;bottom: 0px;
    display: flex;
    flex-direction: row;
    align-items: center;
    justify-content:space-between;
}
.index{
    width: 100%;
}
.index tbody{
    overflow: scroll;
}
</style>