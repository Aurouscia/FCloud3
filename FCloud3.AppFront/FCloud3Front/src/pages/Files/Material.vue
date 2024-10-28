<script setup lang="ts">
import { onMounted, ref, watch } from 'vue';
import { Api } from '@/utils/com/api';
import { injectApi, injectPop } from '@/provides';
import Index, { IndexColumn } from '@/components/Index/Index.vue';
import { MaterialIndexItem, getMaterialItemsFromIndexResult } from '@/models/files/material';
import { IndexQuery, IndexResult } from '@/components/Index';
import SideBar from '@/components/SideBar.vue';
import Pop from '@/components/Pop.vue';
import Notice from '@/components/Notice.vue';

const onlyMine = ref<boolean>(true);
const columns:IndexColumn[] = [
    {
        name:"Name",
        alias:"名称",
        canSetOrder:true,
        canSearch:true,
    },{
        name:"Desc",
        alias:"描述",
        canSetOrder:false,
        canSearch:true
    },{
        name:"Updated",
        alias:"时间",
        canSetOrder:true,
        canSearch:false
    }
]
const qInit:IndexQuery = {
    PageSize : 30,
    Page: 1
}

const createSideBar = ref<InstanceType<typeof SideBar>>();
const creatingFileInput = ref<HTMLInputElement>();
const creatingName = ref<string>("")
const creatingDesc = ref<string>("")
const creatingFile = ref<File>();
function creatingFileChange(e:Event){
    const tar = e.target as HTMLInputElement;
    if(!tar || !tar.files){
        return;
    }
    creatingFile.value = tar.files[0];
}
async function create(){
    if(!creatingFile.value){
        pop.value?.show("请选择上传文件", "failed");
        return;
    }
    const resp = await api.files.material.add(
        creatingName.value,
        creatingDesc.value,
        creatingFile.value
    );
    if(resp){
        creatingName.value = "";
        creatingDesc.value = "";
        creatingFile.value = undefined;
        if(creatingFileInput.value)
            creatingFileInput.value.value = "";
        createSideBar.value?.fold();
        await index.value?.reloadData();
    }
}

const detailSidebar = ref<InstanceType<typeof SideBar>>();
const detailId = ref<number>(0);
const detailName = ref<string>("");
const detailDesc = ref<string>("");
function showDetail(m:MaterialIndexItem){
    detailSidebar.value?.extend();
    detailId.value = m.Id;
    detailName.value = m.Name;
    detailDesc.value = m.Desc;
}
async function editInfo() {
    const resp = await api.files.material.editInfo(
        detailId.value,
        detailName.value,
        detailDesc.value
    );
    if(resp){
        detailSidebar.value?.fold();
        await index.value?.reloadData();
    }
}
const editingFileInput = ref<HTMLInputElement>();
const editingFile = ref<File>();
function editingFileChange(e:Event){
    const tar = e.target as HTMLInputElement;
    if(!tar || !tar.files){
        return;
    }
    editingFile.value = tar.files[0];
}
async function editContent(){
    if(!editingFile.value){
        pop.value?.show("请选择上传文件", "failed");
        return;
    }
    const resp = await api.files.material.editContent(
        detailId.value,
        editingFile.value
    );
    if(resp){
        editingFile.value = undefined;
        if(editingFileInput.value)
            editingFileInput.value.value = "";
        detailSidebar.value?.fold();
        await index.value?.reloadData();
    }
}

async function deleteMaterial(id:number){
    if(window.confirm("确定删除吗？")){
        const resp = await api.files.material.delete(id);
        if(resp){
            detailSidebar.value?.fold();
            await index.value?.reloadData();
        }
    }
}

const items = ref<MaterialIndexItem[]>([])
const index = ref<InstanceType<typeof Index>>();
function setItems(r:IndexResult){
    items.value = getMaterialItemsFromIndexResult(r);
}

let api:Api;
const injected = ref<boolean>(false);
let pop = ref<InstanceType<typeof Pop>>();
onMounted(async()=>{
    api = injectApi();
    injected.value = true;
    pop = injectPop();
})
watch(onlyMine, async()=>{
    if(injected.value){
        await index.value?.reloadData();
    }
})
</script>

<template>
    <div class="materialFunctions">
        <div class="left">
            <button @click="createSideBar?.extend">新建素材</button>
        </div>
        <div class="right">
            <input type="checkbox" v-model="onlyMine">只看自己的
        </div>
    </div>
    <div class="matTable">
    <Index v-if="injected" :fetch-index="q => api.files.material.index(q, onlyMine)" :columns="columns" @reload-data="setItems" ref="index" :q-init="qInit">
        <tr v-for="m in items" @click="showDetail(m)">
            <td>
                <div class="materialContent">
                    <img :src="m.Src"/>
                    <div>{{ m.Name }}</div>
                </div>
            </td>
            <td>
                {{ m.Desc }}
            </td>
            <td>
                {{ m.Time }}
            </td>
        </tr>
    </Index>
    </div>
    <SideBar ref="createSideBar">
        <h1>新建素材</h1>
        <div>
            <table><tbody>
                <tr>
                    <td>简短<br/>名称</td>
                    <td><input v-model="creatingName"></td>
                </tr>
                <tr>
                    <td>简短<br/>描述</td>
                    <td><input v-model="creatingDesc"></td>
                </tr>
                <tr>
                    <td>文件</td>
                    <td><input @change="creatingFileChange" ref="editingFileInput" type="file" accept=".jpg, .jpeg, .png, .svg, .gif"/></td>
                </tr>
                <tr>
                    <td colspan="2">
                        <button @click="create">创建</button>
                    </td>
                </tr>
            </tbody></table>
        </div>
        <Notice :type="'warn'">
            取名请谨慎，后续更改名称会造成词条内引用失效
        </Notice>
    </SideBar>
    <SideBar ref="detailSidebar">
        <h1>更改信息</h1>
        <div>
            <table><tbody>
                <tr>
                    <td>简短<br/>名称</td>
                    <td><input v-model="detailName"></td>
                </tr>
                <tr>
                    <td>简短<br/>描述</td>
                    <td><input v-model="detailDesc"></td>
                </tr>
                <tr>
                    <td colspan="2">
                        <button @click="editInfo">更改信息</button>
                    </td>
                </tr>
            </tbody></table>
        </div>
        <h1>更改文件</h1>
        <div>
            <table><tbody>
                <tr>
                    <td>文件</td>
                    <td><input @change="editingFileChange" ref="creatingFileInput" type="file" accept=".jpg, .jpeg, .png, .svg, .gif"/></td>
                </tr>
                <tr>
                    <td colspan="2">
                        <button @click="editContent">更改文件</button>
                    </td>
                </tr>
            </tbody></table>
        </div>
        <Notice :type="'warn'" :title="'⚠已知缺陷'">
            修改素材名称将造成使用原名称的词条无法再显示该素材<br/><br/>
            修改素材文件可能无法立即对词条中的显示造成更改，请以某种方式影响词条的更新时间使词条重新解析
        </Notice>
        <div class="deleteBtn">
            <button class="danger" @click="deleteMaterial(detailId)">删除本素材</button>
        </div>
    </SideBar>
</template>

<style scoped lang="scss">
@use '@/styles/globalValues';

tr{
    cursor: pointer;
}
.matTable{
    height: calc(100vh - globalValues.$topbar-height - 70px);
}
.materialContent{
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    justify-content: space-around;
    div{
        min-width: 120px;
        flex-grow: 1;
        text-align: center;
    }
    img{
        height: 30px;
        border-radius: 3px;
    }
}
.materialFunctions{
    display:flex;
    justify-content:space-between;
    margin-top: 10px;
    margin-bottom:10px;
    .left{
        display: flex;
        align-items: center;
    }
    .right{
        background-color: #eee;
        border-radius: 5px;
        padding: 5px;
        display: flex;
        align-items: center;
        font-size: 16px;
        input[type="checkbox"]{
            height: 20px;
            width: 20px;
        }
    }
}
.deleteBtn{
    margin-top: 10px;
    display: flex;
    justify-content: center;
}
</style>