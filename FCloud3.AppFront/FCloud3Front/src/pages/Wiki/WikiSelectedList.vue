<script setup lang="ts">
import LongPress from '@/components/LongPress.vue';
import Search from '@/components/Search.vue';
import SideBar from '@/components/SideBar.vue';
import { WikiSelectedDto } from '@/models/wiki/wikiSelected';
import { injectApi } from '@/provides';
import { onMounted, ref, nextTick, useTemplateRef } from 'vue';

const api = injectApi()
const list = ref<WikiSelectedDto[]>([])
async function load(){
    const res = await api.wiki.wikiSelected.getList();
    if(res){
        list.value = res
    }
}

const editing = ref<WikiSelectedDto>()
const isCreating = ref(false);
const wsEditSidebar = useTemplateRef('wsEditSidebar')
function startCreating(){
    isCreating.value = true;
    editing.value = {
        Id:0,
        WikiItemId:0,
        DropAfterHr:48,
        CreatorUserId:0,
        Order:0
    }
    nextTick(()=>{
        wsEditSidebar.value?.extend()
    })
}
function startEditing(ws:WikiSelectedDto){
    isCreating.value = false;
    editing.value = ws
    nextTick(()=>{
        wsEditSidebar.value?.extend()
    })
}
async function editDone(){
    if(!editing.value)
        return;
    if(isCreating.value){
        const res = await api.wiki.wikiSelected.insert(editing.value)
        if(res){
            wsEditSidebar.value?.fold()
            await load();
        }
    }else{
        const res = await api.wiki.wikiSelected.edit(editing.value)
        if(res){
            wsEditSidebar.value?.fold()
            await load();
        }
    }
}
function searchDone(val:string, id:number){
    if(!editing.value)
        return
    editing.value.WikiItemId = id;
    editing.value.WikiTitle = val;
}

async function removeEditing(){
    if(!editing.value)
        return;
    const res = await api.wiki.wikiSelected.remove(editing.value.Id);
    if(res){
        wsEditSidebar.value?.fold()
        await load()
    }
}

onMounted(async()=>{
    await load()
})
</script>

<template>
<h1>
    精选词条管理
    <div class="h1Btns">
        <button @click="startCreating">添加</button>
    </div>
</h1>
<table>
<thead>
    <tr>
        <th style="width: 30px;"></th>
        <th style="min-width: 220px">标题</th>
        <th style="min-width: 220px">封面图</th>
        <th style="width: 100px;">设置者</th>
        <th style="width: 100px;">时长</th>
        <th style="width: 50px;"></th>
    </tr>
</thead>
<tbody>
    <tr v-for="w in list">
        <td>
            {{ w.Order }}
        </td>
        <td>
            {{ w.WikiTitle }}
            <div class="intro">
                {{ w.Intro }}
            </div>
        </td>
        <td>
            <img :src="w.CoverUrl||'/fcloud.svg'">
        </td>
        <td>
            <div class="setter">
                {{ w.CreatorName }}
            </div>
        </td>
        <td class="time">
            {{ w.DropAfterHr }}小时
            <div class="leftTime">(剩余{{ w.LeftHr }}小时)</div>
        </td>
        <td>
            <button class="minor" @click="startEditing(w)">编辑</button>
        </td>
    </tr>
</tbody></table>
<SideBar ref="wsEditSidebar">
    <h1>编辑</h1>
    <table v-if="editing"><tbody>
        <tr>
            <td>词条</td>
            <td v-if="isCreating">
                <div v-show="editing.WikiItemId" class="editingWikiTitle">已选择：{{ editing?.WikiTitle }}</div>
                <Search :source="api.etc.quickSearch.wikiItem" :compact="true"
                    @done="searchDone" :placeholder="'搜索词条标题'">
                </Search>
            </td>
            <td v-else>
                {{ editing?.WikiTitle }}
            </td>
        </tr>
        <tr>
            <td>简介</td>
            <td>
                <textarea v-model="editing.Intro" class="infoEditTextarea"></textarea>
            </td>
        </tr>
        <tr>
            <td>时长</td>
            <td>
                <input v-model="editing.DropAfterHr" type="number" min="1" max="72"/>
            </td>
        </tr>
        <tr class="noneBackground">
            <td colspan="2">
                <button @click="editDone">确定</button>
            </td>
        </tr>
    </tbody></table>
    <div v-if="editing && !isCreating" class="removal">
        <LongPress :reached="removeEditing">长按移除</LongPress>
    </div>
</SideBar>
</template>

<style scoped lang="scss">
.removal{
    margin-top: 10px;
    display: flex;
    justify-content: center;
}
th, .time{
    white-space: nowrap;
}
.leftTime, .intro{
    font-size: small;
    color: #666;
    text-align: center;
}
h1{
    display: flex;
    justify-content: space-between;
    align-items: center;
}
.h1Btns{
    flex-shrink: 0;
    font-size: medium;
}
.editingWikiTitle{
    max-width: 220px;
    margin: 10px 0px 10px 0px;
    text-align: center;
    color:red;
    font-weight: bold;
}
.setter{
    max-width: 100%;
}
.setter, .editingWikiTitle{
    text-overflow: ellipsis;
    overflow: hidden;
    white-space: nowrap;
}
img{
    width: 200px;
    height: 150px;
    border-radius: 10px;
    object-fit: cover;
}
table{
    min-width: 100%;
}
tr:hover td{
    background-color: #ddd;
}
</style>