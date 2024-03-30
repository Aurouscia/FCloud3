<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { injectApi } from '../../provides';
import { Api } from '../../utils/api';
import { WikiTitleContainListModel, WikiTitleContainListModelItem, WikiTitleContainType } from '../../models/wiki/wikiTitleContain';
import Loading from '../Loading.vue';
import { pullAt, unionBy } from 'lodash';
import Search from '../Search.vue';

const props = defineProps<{
    type: WikiTitleContainType
    objectId: number
    getContent: ()=>string|null|undefined
}>(); 

const data = ref<WikiTitleContainListModel>();
const changed = ref<boolean>(false);
async function load(){
    data.value = await api.wiki.wikiTitleContain.getAll({Type: props.type, ObjectId: props.objectId});
    keepOrder()
    changed.value = false;
}
async function set() {
    const ids = data.value?.Items.map(x => x.WikiId);
    if(ids === undefined) return;
    const resp = await api.wiki.wikiTitleContain.setAll({Type: props.type, ObjectId: props.objectId, WikiIds: ids})
    if(resp){
        await load();
        emits('changed');
    }
}

function remove(wikiId: number){
    if(!data.value){return;}
    const index = data.value.Items.findIndex(x => x.WikiId === wikiId);
    if(index === -1) return;
    pullAt(data.value.Items, index);
    changed.value = true;
}
async function autoFill(){
    if(!data.value){return}
    const content = props.getContent();
    if(!content){return}
    const res = await api.wiki.wikiTitleContain.autoFill(content);
    if(res){
        const newItems:WikiTitleContainListModelItem[] = res.Items.map(x => {
            return {
                Id: 0,
                WikiId: x.Id,
                WikiTitle: x.Title
            }
        });
        const oriLength = data.value.Items.length;
        data.value.Items = unionBy(data.value?.Items, newItems, x=>x.WikiId);
        keepOrder()
        const newLength = data.value.Items.length;
        if(newLength > oriLength){
            changed.value = true;
        }
    }
}
function searchDone(title:string, id:number){
    if(!data.value){return;}
    if(data.value.Items.some(x=>x.WikiId === id)){return;}
    data.value?.Items.push({
        Id: 0,
        WikiId: id,
        WikiTitle: title
    })
    keepOrder();
    changed.value = true
}
function keepOrder(){
    if(!data.value){return;}
    data.value.Items.sort((x,y)=>x.WikiTitle.localeCompare(y.WikiTitle));
}

const emits = defineEmits<{
    (e:'changed'):void
}>();

let api:Api;
onMounted(async() => {
    api = injectApi();
    await load();
});
</script>

<template>
<div class="wikiTitleContain">
    <div class="panelName">
        包含的词条标题
        <div class="question">?</div>
        <div class="questionBody">
            只有在这里显示的词条标题才会在本段被自动生成链接
        </div>
    </div>
    <div>
        <Search v-if="api" :source="api.utils.quickSearch.wikiItem" @done="searchDone" :placeholder="'手动添加'"></Search>
        <button @click="autoFill" class="minor" style="width: 280px;margin: 5px 0px 0px 0px">自动添加</button>
    </div>
    <div v-if="data" class="list">
        <div v-for="item,idx in data.Items" :key="item.WikiId">
            <div>
                <span class="serial">{{ idx+1 }}.</span>
                {{ item.WikiTitle }}
            </div>
            <div @click="remove(item.WikiId)" class="removeBtn">移除</div>
        </div>
    </div>
    <Loading v-else>
    </Loading>
    <div v-if="changed" style="text-align: center;">
        <button class="ok" @click="set">保存更改</button>
    </div>
</div>
</template>

<style scoped>
.panelName{
    display: flex;
    justify-content: center;
    gap:5px;
    align-items: center;
    position: relative;
    padding: 5px;
    font-size: 20px;
}
.removeBtn{
    color:#aaa;
    font-size: 14px;
    cursor: pointer;
}
.list{
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: top;
    height: 240px;
    overflow-y: auto;
}
.list>div{
    width: 240px;
    display: flex;
    justify-content: space-between;
    padding: 5px;
    border-bottom: 1px solid #ddd;
}
.list .serial{
    font-size: 14px;
    color:#aaa;
}
.question{
    line-height: 15px;
    text-align: center;
    width: 15px;
    height: 15px;
    font-size: 12px;
    color: #aaa;
    border: 2px solid #aaa;
    border-radius: 100px;
    cursor: pointer;
}
.questionBody{
    position: absolute;
    width: 100px;
    top: 30px;
    display: none;
    z-index: 10000;
}
.question:hover+.questionBody{
    display: block;
    font-size: 14px;
    padding: 5px;
    background-color: white;
    color: #aaa;
    box-shadow: 0px 0px 5px 0px black;
    border-radius: 5px;
}
.wikiTitleContain{
    padding: 10px;
    position: relative;
}
</style>