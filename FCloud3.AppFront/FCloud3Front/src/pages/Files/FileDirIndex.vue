<script setup lang="ts">
import { inject, onMounted, ref, watch } from 'vue';
import IndexMini, { IndexColumn } from '../../components/Index/IndexMini.vue';
import { Api } from '../../utils/api';
import { IndexQuery, IndexResult } from '../../components/Index';
import FileDirChild from './FileDirChild.vue';
import { useRouter } from 'vue-router';
import _ from 'lodash';
import SideBar from '../../components/SideBar.vue';
import settingsImg from '../../assets/settings.svg';
import FileUpload from '../../components/FileUpload.vue';

interface FileDirIndexItem{
    Id:number,
    Name:string,
    Update:string,
    OwnerId:number,
    OwnerName:string,
    ByteCount:number,
    FileNumber:number,

    showChildren?:boolean|undefined
}
const props = defineProps<{
    path:string[]|string
}>();
const router = useRouter();
const columns:IndexColumn[] = 
[
    {name:'Id',alias:'号',canSearch:true,canSetOrder:true},
    {name:'Name',alias:'名称',canSearch:true,canSetOrder:true},
    {name:'Update',alias:'上次更新',canSearch:false,canSetOrder:true}
]

const data = ref<FileDirIndexItem[]>([]);
var thisDirId = 0;
//会在OnMounted下一tick被MiniIndex执行，获取thisDirId
const fetchIndex:(q:IndexQuery)=>Promise<IndexResult|undefined>=async(q)=>{
    var p = props.path;
    if(typeof p === 'string'){
        if(p==''){p=[]}
        else{p = [p];}
    }
    const res = await api.files.index(q,p)
    thisDirId = res?.ThisDirId || 0;
    return res?.Data;
}
function render(i:IndexResult){
    data.value = [];
    i.Data.forEach(r=>{
        data.value?.push({
            Id: parseInt(r[0]),
            Name:r[1],
            Update:r[2],
            OwnerId:parseInt(r[3]),
            OwnerName:r[4],
            ByteCount:parseInt(r[5]),
            FileNumber:parseInt(r[6])
        })
    })
}
function jumpToSubDir(name:string){
    var path =  _.concat(props.path,name);
    path = _.filter(path, x=>!!x)
    router.replace({name:'files',params:{path}});
}
function jumpToAncestor(idxInChain:number){
    router.replace({name:'files',params:{path: _.take(props.path,idxInChain+1)}})
}

const pathAncestors = ref<string[]>([]);
const pathThisName = ref<string>("");
function setPathDisplays(){
    if(typeof props.path==='string' || props.path.length==0){
        pathAncestors.value = []
        pathThisName.value = ''
    }else{
        pathAncestors.value = _.take(props.path,props.path.length-1)
        pathThisName.value = _.last(props.path)||"";
    }
}

const sidebar = ref<InstanceType<typeof SideBar>>();
function startEditDirInfo(){
    sidebar.value?.extend();
}

const index = ref<InstanceType<typeof IndexMini>>();
var api:Api;
const ok = ref<boolean>(false);
onMounted(async()=>{
    api = inject('api') as Api;
    ok.value = true;//api的inject必须和Index的Mount不在一个tick里，否则里面获取不到fetchIndex
    setPathDisplays();
})
watch(props,async(_newVal)=>{
    setPathDisplays();
    await index.value?.reloadData();
});
</script>

<template>
    <div v-if="ok" class="fileDir">
        <div>
            <div class="ancestors">
                <div>
                    <span @click="jumpToAncestor(-1)">根目录</span>/
                </div>
                <div v-for="a,idx in pathAncestors">
                    <span @click="jumpToAncestor(idx)">{{ a }}</span>/
                </div> 
            </div>
            <div class="thisName">
                {{ pathThisName }}
                <img class="settingsBtn" @click="startEditDirInfo" :src='settingsImg'/>
            </div>
        </div>
        <IndexMini ref="index" :fetch-index="fetchIndex" :columns="columns" :display-column-count="1" @reload-data="render">
            <tr v-for="item in data">
                <td>
                    <div class="subdir">
                        <div>
                            <div class="foldBtn" v-show="!item.showChildren" @click="item.showChildren=true" style="color:#999">▶</div>
                            <div class="foldBtn" v-show="item.showChildren" @click="item.showChildren=false" style="color:black">▼</div>
                            <div class="subdirName" @click="jumpToSubDir(item.Name)">{{ item.Name }}</div>
                        </div>
                        <div>
                        </div>
                    </div>
                    <div class="detail" v-if="item.showChildren">
                        <FileDirChild :dir-id="item.Id" :path="_.concat(props.path, item.Name)" :fetch-from="api.files.takeContent"></FileDirChild>
                    </div>
                </td>
            </tr>
        </IndexMini>
    </div>    
    <SideBar ref="sidebar">
        <div>
            <h1>文件夹操作</h1>
        </div>
        <div class="section">
            <h2>上传新文件</h2>
            <FileUpload dist="test"></FileUpload>
        </div>
        <div class="section">
            <h2>编辑文件夹信息</h2>
            <table>
                <tr>
                    <td>名称</td>
                    <td><input/></td>
                </tr>
                <tr>
                    <td>待填</td>
                    <td><input/></td>
                </tr>
                <tr class="noneBackground">
                    <td></td>
                    <td>
                        <button @click="">保存</button>
                    </td>
                </tr>
            </table>
        </div>
    </SideBar>
</template>

<style>
.settingsBtn{
    object-fit: contain;
    width: 1.2em;
    height: 1.2em;
    padding: 2px;
    background-color: #666;
    border-radius: 10px;
}
.ancestors{
    font-size: small;
    color:gray;
    display: flex;
}
.ancestors div span{
    padding: 0px 3px 0px 3px;
}
.ancestors div span:hover{
    text-decoration: underline;
    cursor: pointer;
}
.thisName{
    font-size: large;
    margin-bottom: 10px;
    user-select: none;
}
.detail{
    display: flex;
    flex-direction: column;
    gap:5px;
    margin-top: 5px;
    padding-top: 5px;
    border-top: 1px solid black;
}
.foldBtn{
    width: 20px;
    overflow: visible;
    cursor: pointer;
}
.subdirName{
    text-align: left;
}
.subdirName:hover{
    text-decoration: underline;
    cursor: pointer;
}
.subdir div{
    display: flex;
    flex-direction: row;
    justify-content: left;
    align-items: center;
    gap:5px
}
.subdir{
    display: flex;
    flex-direction: row;
    justify-content: left;
    gap:20px;
    align-items: center;
    padding: 4px;
}
.fileDir{
    height: 600px;
}
</style>