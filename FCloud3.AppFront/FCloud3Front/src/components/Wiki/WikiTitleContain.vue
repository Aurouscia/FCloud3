<script setup lang="ts">
import { Ref, onMounted, ref } from 'vue';
import { injectApi, injectPop } from '@/provides';
import { Api } from '@/utils/com/api';
import { WikiTitleContainListModel, WikiTitleContainListModelItem, WikiTitleContainType } from '@/models/wiki/wikiTitleContain';
import Loading from '../Loading.vue';
import { pullAt, unionBy } from 'lodash';
import Search from '../Search.vue';
import Pop from '../Pop.vue';

const props = defineProps<{
    type: WikiTitleContainType
    objectId: number
    getContent?: ()=>string|null|undefined
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
async function autoFill(suppressNoneWarning = false){
    if(!data.value || !props.getContent){return}
    const content = props.getContent();
    if(!content){return}
    const res = await api.wiki.wikiTitleContain.autoFill(props.objectId, props.type, content);
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
            await set();
        }else{
            if(!suppressNoneWarning)
                pop.value.show('没有找到可以添加的', "warning");
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
defineExpose({autoFill})

let api:Api;
let pop:Ref<InstanceType<typeof Pop>>;
onMounted(async() => {
    api = injectApi();
    pop = injectPop();
    await load();
});
</script>

<template>
<div class="wikiTitleContain">
    <div class="panelName">
        包含的词条标题
        <div class="question">?</div>
        <div class="questionBody">
            <p>只有在这里显示的词条标题才会在本段被自动生成链接。</p>
            <p>手动删除的将不再会被自动添加，需通过下方搜索框添加。</p>
            <p>本列表会在段落内容编辑保存时自动更新一次。</p>
            <p>链接在同一段落内只生成一次，被隐藏的词条不会生成链接。</p>
        </div>
    </div>
    <div>
        <Search v-if="api" :source="api.etc.quickSearch.wikiItem" @done="searchDone" :placeholder="'手动添加'"></Search>
        <button @click="()=>autoFill()" v-if="getContent" class="minor" style="width: 260px;margin: 5px 0px 0px 0px">自动添加</button>
    </div>
    <div v-if="data" class="list">
        <div v-for="item,idx in data.Items" :key="item.WikiId" class="listItem">
            <div class="listItemBody">
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

<style scoped lang="scss">
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
    justify-content: flex-start;
    min-height: 340px;
    max-height: calc(100vh - 200px);
    overflow-y: auto;
}
.listItem{
    width: 230px;
    flex-shrink: 0;
    flex-grow: 0;
    display: flex;
    justify-content: space-between;
    gap: 5px;
    padding: 5px;
    border-bottom: 1px solid #ddd;
    white-space: nowrap;
    .listItemBody{
        flex-shrink: 1;
        min-width: 0px;
        overflow: hidden;
        text-overflow: ellipsis;
    }
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
    font-weight: bold;
    color: #aaa;
    border: 2px solid #aaa;
    border-radius: 100px;
    cursor: pointer;
}
.questionBody{
    position: absolute;
    width: 150px;
    top: 35px;
    display: none;
    z-index: 10000;
    gap: 10px;
    flex-direction: column;
    p{
        text-indent: 20px;
    }
}
.question:hover+.questionBody{
    display: flex;
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