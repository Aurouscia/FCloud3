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
        let changed = 
            query.value.OrderBy != target.name 
            || query.value.OrderRev != rev;
        query.value.OrderBy = target.name;
        query.value.OrderRev = rev;
        orderByAlias.value = target.alias;
        if(changed){
            query.value.Page = 1
            await reloadData()
        }    
    }
}
async function clearOrder() {
    let changed = query.value.OrderBy != undefined;
    query.value.OrderBy = undefined
    orderByAlias.value = ""
    
    if(changed){
        query.value.Page = 1;
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
defineExpose({reloadData})

const query = ref<IndexQuery>(indexQueryDefault())
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
<table class="index" v-if="query&&i">
    <thead>
        <tr v-if="!props.hidePage" class="indexControl">
            <th :colspan="3">
                <div class="ops">
                    <div class="pageOps">
                        <button class="queryAdjust" @click="changePage('prev')" :class="{highlight:query.Page && query.Page>1}">◀</button>
                            第{{ query.Page }}页
                        <button class="queryAdjust" @click="changePage('next')" :class="{highlight:query.Page && query.Page<i.PageCount}">▶</button>
                        <br/>
                        <span style="font-size: small;text-wrap: nowrap;">
                                共{{ i.PageCount }}页
                                每页<input v-model="query.PageSize" @blur="reloadData" class="miniInput"/>条
                        </span>
                    </div>
                    <div class="info">
                        <div>
                            <b style="margin-right: 10px;">{{ i.TotalCount }}条数据</b>
                            <span v-if="orderByAlias">
                                排序:{{ orderByAlias }}{{ query.OrderRev?'(反)':'(正)' }}
                                <span class="clearOrder" @click="clearOrder">清除</span>
                            </span>
                        </div>
                        <div v-if="searchStrsAlias && searchStrsAlias.length>0">
                            {{ searchStrsAlias.join(" & ") }}
                        </div>
                    </div>
                </div>
            </th>
        </tr>
        <tr v-if="!props.hideHead" class="colNamesTr">
            <th v-for="c in cols">
                <span v-if="c.searchText||c.editing">
                    <input class="search" v-model="c.searchText" ref="search" @blur="setSearch(c.name)" placeholder="搜索">
                    <button class="clearSearch" @click="endEdit(c.name)">x</button>
                </span>
                <span v-else class="colName" @click="startEdit(c.name)">{{ c.alias }}</span>
                <span class="order" v-if="c.canSetOrder">
                    <button class="queryAdjust" @click="setOrder(c.name,true)" :class="{highlight:query.OrderBy==c.name && query.OrderRev}">▲</button>
                    <button class="queryAdjust" @click="setOrder(c.name,false)" :class="{highlight:query.OrderBy==c.name && !query.OrderRev}">▼</button>
                </span>
            </th>
        </tr>
    </thead>
    <tbody>
        <slot></slot>
    </tbody>
</table>
<div v-else>
    <Loading></Loading>
</div>
</template>

<style scoped lang="scss">
.ops{
    display: flex;
    justify-content: flex-start;
    gap: 10px;
    padding: 5px 10px 5px 10px;
    height: 50px;
    align-items: center;
    .info{
        font-size: small;
        font-weight: normal;
        line-height: 1.8em;
        text-align: left;
        .clearOrder{
            text-decoration: underline;
            cursor: pointer;
            &:hover{
                font-weight: bold;
            }
        }
    }
    .pageOps{
        width: 150px
    }
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
.colNamesTr{
    height: 40px;
    position: sticky;
    top:74px;
}
th{
    padding:5px;
    min-width: 80px;
    white-space: nowrap;
}
.miniInput{
    width: 1.5em;
    text-align: center;
    margin: 0px;
    background:white;
    padding: 0px;
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
    color: #ddd;
}
button.highlight{
    color:white
}
.indexControl{ 
    position: sticky;
    top:2px;
    th{
        background-color: #bbb;
    }
}
.index{
    width: 100%;
}
.index tbody{
    overflow: scroll;
}
</style>