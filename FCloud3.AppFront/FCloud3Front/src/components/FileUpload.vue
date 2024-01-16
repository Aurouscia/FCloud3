<script setup lang="ts">
import { CSSProperties, Ref, inject, onMounted, ref } from 'vue';
import { fileSizeStr, getFileType } from '../utils/fileUtils';
import { StagingFile, FileUploadDist } from '../models/files/StagingFile';
import _ from 'lodash'
import Pop from './Pop.vue';
import { Api } from '../utils/api';

const props = defineProps<{
    dist:FileUploadDist
}>();

const fileInput = ref<HTMLInputElement>();
const dirInput = ref<HTMLInputElement>();
async function inputChange(e:Event){
    const tar = e.target as HTMLInputElement;
    if(!tar || !tar.files){
        return;
    }
    const newFiles = _.differenceWith(tar.files,fileList.value,
        (x,y)=>x.webkitRelativePath+x.name==y.file.webkitRelativePath+y.file.name)
    if(newFiles.length>0){
        const sf = newFiles.map(x=>{return{
            file:x,
            displayName:x.name
        }})
        fileList.value.push(...sf);
    }
}
function delFile(idx:number){
    _.pullAt(fileList.value,idx)
}

const fileList = ref<StagingFile[]>([]);

function getFileExt(str:string):string{
    var parts = str.split('.');
    if(parts.length==1){
        return "???";
    }
    var ext = parts[parts.length-1];
    if(ext.length>4){
       ext = ext.substring(0,4);
    }
    return ext;
}
function getFileIconStyle(fileName:string):CSSProperties{
    var type = getFileType(fileName);
    if(type=='image'){
        return {backgroundColor:'#A7D0D8'}
    }
    if(type=="video"){
        return {backgroundColor:'#D6ACC3'}
    }
    if(type=='text'){
        return {backgroundColor:'#96B23C'}
    }
    if(type=='audio'){
        return {backgroundColor:'#EFD67F'}
    }
    return {backgroundColor:'#AAAAAA'}
}
async function commit(idx:number){
    const target = fileList.value[idx];
    if(!target){return;}
    const resp = await api.files.save(target,props.dist);
    if(resp){
        delFile(idx);
    }
}

var pop: Ref<InstanceType<typeof Pop>>
var api: Api;
onMounted(()=>{
    pop = inject('pop') as Ref<InstanceType<typeof Pop>>;
    api = inject('api') as Api;
})
</script>

<template>
    <div class="fileUpload">
        <div class="staging">
            <div class="fileItem" v-for="f,i in fileList" :key="f.file.name">
                <div class="fileIcon" :style="getFileIconStyle(f.file.name)">{{ getFileExt(f.file.name) }}</div>
                <div class="fileName">
                    <span @click="f.editing=!f.editing">{{ f.displayName }}</span>
                    <span class="commitBtn" @click="commit(i)">✔</span>
                    <span class="delBtn" @click="delFile(i)">×</span>
                </div>
                <div class="nameEdit" v-show="f.editing">
                    <div>
                        <input v-model="f.displayName"/>
                        <button @click="f.editing=false" class="ok">OK</button>
                    </div>
                </div>
                <div class="fileSize" :style="{color:f.file.size>10*1000*1000?'red':''}">{{ fileSizeStr(f.file.size)}}</div>
            </div>
        </div>
        <div class="controls">
            <div class="uploadBtn" @click="fileInput?.showPicker()">
                选择文件
            </div>
            <div class="uploadBtn" @click="dirInput?.showPicker()">
                选择文件夹
            </div>
        </div>
        <input @change="inputChange" ref="fileInput" type="file" multiple/>
        <input @change="inputChange" ref="dirInput" type="file" webkitdirectory>
    </div>
</template>

<style scoped>
.nameEdit input{
    border: none;
    margin: 2px;
    width: 150px;
}
.nameEdit{
    position: absolute;
    top:32px;left:50px;
    background-color: #aaa;
    border:2px solid white;
    padding-left: 10px;
    height: 50px;
    width:200px;
    z-index: 100;
    border-radius: 10px;
    display: flex;
    flex-direction: column;
    justify-content: space-around;
    align-items:start;
    color:white
}
.commitBtn{
    background-color: green;
    right: 3px;
}
.commitBtn:hover{
    background-color: darkgreen;
}
.delBtn{
    background-color: red;
    right: 25px;
}
.delBtn:hover{
    background-color: darkred;
}
.delBtn,.commitBtn{
    position: absolute;
    top:0px;bottom: 0px;
    margin: auto 0px auto 0px;
    width: 1.2em;
    height: 1.2em;
    line-height: 1.2em;
    text-align: center;
    border-radius: 2px;
    color:white;
    display: none;
    cursor: pointer;
}
.fileName:hover .delBtn,.fileName:hover .commitBtn{
    display: block;
}
.fileIcon{
    border-radius: 5px;
    border:2px solid white;
    font-size: small;
    color:white;
    width: 28px;
    height: 28px;
    line-height: 28px;
    text-align: center;
    flex-shrink: 0;
}

.fileName,.fileSize{
    white-space: nowrap;
    background-color: white;
    border-radius:5px;
    padding: 4px;
    line-height: 20px;
    height:20px;
    position: relative;
}
.fileName{
    width: 200px;
    overflow: hidden;
    text-overflow: ellipsis;
    cursor: pointer;
}
.fileSize{
    width: 40px;
    font-size: small;
    padding: 1px;
    height: 26px;
    line-height: 26px;
}
.fileItem{
    padding: 5px;
    display: flex;
    align-items: center;
    gap:5px;
    position: relative;
}
.staging{
    width: 300px;
    height: 310px;
    background-color: #ddd;
    border-radius: 5px;
    overflow-y: auto;
}
.controls{
    display: flex;
    flex-direction: row;
    gap:5px;
    justify-content: center;
    align-items: center;
}
.fileUpload{
    display: flex;
    flex-direction: column;
    width: fit-content;
    gap:10px;
}
.uploadBtn{
    background-color: cornflowerblue;
    color:white;
    position: relative;
    height: 35px;
    width: 100px;
    border-radius: 5px;
    text-align: center;
    line-height: 35px;
    cursor: pointer;
    transition: 0.5s;
}
.uploadBtn:hover{
    background-color:rgb(76, 125, 217)
}
.fileUpload input[type=file]{
    display: none;
}
</style>