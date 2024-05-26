<script lang="ts" setup>
import { onMounted, ref } from 'vue';
import { WikiTemplateListItem } from '../../models/wikiParsing/wikiTemplate';
import { injectApi } from '../../provides';
import { Api } from '../../utils/com/api';
import { useWikiParsingRoutesJump } from './routes/routesJump';

const data = ref<WikiTemplateListItem[]>([]);
async function load() {
    const res = await api.wikiParsing.wikiTemplate.getList();
    if (res)
        data.value = res;
}

const wantCreate = ref<boolean>();
const creatingName = ref<string>("");
async function create() {
    const res = await api.wikiParsing.wikiTemplate.add(creatingName.value);
    if (res) {
        wantCreate.value = false;
        creatingName.value = "";
        await load();
    }
}

const { jumpToWikiTemplateEditor } = useWikiParsingRoutesJump();
async function enterEdit(id:number) {
    jumpToWikiTemplateEditor(id);
}

var api:Api;
onMounted(async()=>{
    api = injectApi();
    await load();
})
</script>

<template>
<div class="templateList">
    <h1>
        模板列表
    </h1>
    <div style="position: relative;">
        <button @click="wantCreate = !wantCreate">新建</button>
        <div class="create" v-if="wantCreate">
            <input type="text" placeholder="名称" v-model="creatingName"/><br/>
            <button @click="create">创建</button>
        </div>
    </div>
    <table>
        <tr>
            <th class="nameTh">
                名称
            </th>
            <th>
                创建者
            </th>
            <th>
                上次更新
            </th>
        </tr>
        <tr v-for="item in data" :key="item.Id">
            <td>
                <span class="name" @click="enterEdit(item.Id)">
                    {{ item.Name }}
                </span>
            </td>
            <td>
                {{ item.CreatorName }}
            </td>
            <td>
                {{ item.Updated }}
            </td>
        </tr>
    </table>
</div>
</template>

<style scoped>
.name:hover{
    cursor: pointer;
    text-decoration: underline;
    color: cornflowerblue;
}
.nameTh{
    width: 45%;
}
table{
    table-layout: fixed;
    width: 100%;
}
.create{
    position: absolute;
    top:35px;left:5px;
    background-color: white;
    padding: 5px;
    border: 2px solid cornflowerblue;
    border-radius: 5px;
    box-shadow: 0px 0px 5px 0px black;
    text-align: center;
}
.templateList{
    min-height: 200px;
}
</style>