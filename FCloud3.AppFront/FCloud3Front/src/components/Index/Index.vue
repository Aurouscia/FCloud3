<script setup lang="ts">
import { nextTick, onMounted, ref } from 'vue';
import { IndexQuery, IndexResult, indexQueryDefault, indexResultDefault } from './index.ts';
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
    qInit?:IndexQuery,
    hidePage?:boolean|undefined,
    hideHead?:boolean|undefined,
}>();
const i = ref<IndexResult>();

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
    i.value = (await props.fetchIndex(query.value||indexQueryDefault)) || indexResultDefault;
    emit('reloadData',i.value);
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
        reloadData();
    }else{
        target.searchText = "";
        target.editing = false;
    }
}
function startEdit(colName:string){
    const target = cols.value.find(x=>x.name==colName);
    if(!target || !target.canSearch){return;}
    target.editing = true;
    nextTick(()=>{
        search.value[0].focus();
    })
}
async function endEdit(colName:string){
    const target = cols.value.find(x=>x.name==colName);
    if(!target){return;}
    target.editing = false;
    target.searchText = "";
    await reloadData();
}

const emit = defineEmits<{
    (e: 'reloadData', value: IndexResult): void
}>()

const query = ref<IndexQuery>(indexQueryDefault)
const searchStrsAlias = ref<string[]>([]);
const orderByAlias = ref<string>("");
const cols = ref<IndexColumn[]>([]);
const search = ref<HTMLInputElement[]>([]);
onMounted(async()=>{
    if(props.qInit){
        query.value = props.qInit;
    }
    cols.value = props.columns;
    await reloadData();
})
</script>

<template>
<table class="index" v-if="qInit&&i">
    <tbody>
        <tr v-if="!props.hidePage" class="indexControl">
            <th class="info" :colspan="cols.length-1">
                <div>
                    <b style="margin-right: 10px;">{{ i.TotalCount }}条数据</b>
                    <span v-if="orderByAlias">排序:{{ orderByAlias }}{{ query.OrderRev?'(反)':'(正)' }}</span>
                </div>
                <div v-if="searchStrsAlias && searchStrsAlias.length>0">
                    {{ searchStrsAlias.join(" & ") }}
                </div>

            </th>
            <th>
                第{{ qInit.Page }}页
                    <button class="queryAdjust" @click="changePage('prev')" :class="{highlight:qInit.Page && qInit.Page>1}">▲</button>
                    <button class="queryAdjust" @click="changePage('next')" :class="{highlight:qInit.Page && qInit.Page<i.PageCount}">▼</button>
                    <br/>
                    <span style="font-size: small;text-wrap: nowrap;">
                        共{{ i.PageCount }}页
                        每页<input v-model="qInit.PageSize" @blur="reloadData" class="miniInput"/>条
                    </span>
            </th>
        </tr>
        <tr v-if="!props.hideHead">
            <th v-for="c in cols">
                <span v-if="c.searchText||c.editing">
                    <input class="search" v-model="c.searchText" ref="search" @blur="setSearch(c.name)" placeholder="搜索">
                    <button class="clearSearch" @click="endEdit(c.name)">x</button>
                </span>
                <span v-else class="colName" @click="startEdit(c.name)">{{ c.alias }}</span>
                <span class="order" v-if="c.canSetOrder">
                    <button class="queryAdjust" @click="setOrder(c.name,true)" :class="{highlight:qInit.OrderBy==c.name && qInit.OrderRev}">▲</button>
                    <button class="queryAdjust" @click="setOrder(c.name,false)" :class="{highlight:qInit.OrderBy==c.name && !qInit.OrderRev}">▼</button>
                </span>
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
.info{
    font-size: small;
    font-weight: normal;
    line-height: 1.8em;
}
.order{
    margin: 0px 0px 0px 5px;
}
.search{
    width: 80px;
    margin: 0px;
    border: none;
    border-radius: 5px;
    text-align: center;
}
.colName{
    cursor: pointer;
}
th{
    padding:5px;
    min-width: 100px;
}
.miniInput{
    width: 1.5em;text-align: center;margin: 0px;background:white
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
.indexControl th{
    background-color: #bbb;
}
.index{
    width: 100%;
}
.index tbody{
    overflow: scroll;
}
</style>