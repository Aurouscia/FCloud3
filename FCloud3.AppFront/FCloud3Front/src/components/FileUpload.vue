<script setup lang="ts">
import { Ref, inject, onMounted, ref } from 'vue';
import { fileSizeStr, getFileIconStyle,getFileExt } from '../utils/fileUtils';
import { StagingFile, FileUploadDist} from '../models/files/fileItem';
import _ from 'lodash'
import Pop from './Pop.vue';
import { Api } from '../utils/api';

const props = defineProps<{
    dist:FileUploadDist,
    single?:boolean
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
    tar.value='';
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


async function commit(idx:number){
    const target = fileList.value[idx];
    if(!target){return;}
    if(target.file.size>10*1000*1000){
        pop.value.show("文件过大，请拆分或压缩质量","failed")
        return;
    }
    const resp = await api.fileItem.save(target,props.dist);
    if(resp){
        delFile(idx);
        emit('uploaded',resp.CreatedId)
    }
}

const emit = defineEmits<{
    (e: 'uploaded', fileItemId: number): void
}>()

var pop: Ref<InstanceType<typeof Pop>>
var api: Api;
onMounted(()=>{
    pop = inject('pop') as Ref<InstanceType<typeof Pop>>;
    api = inject('api') as Api;
})
</script>

<template>
    <div class="fileUpload">
        <div class="staging" :class="{single:props.single}">
            <div class="fileItem" v-for="f,i in fileList" :key="f.file.name">
                <div class="fileIcon" :style="getFileIconStyle(f.file.name)">{{ getFileExt(f.file.name) }}</div>
                <div class="fileBody">
                    <div class="itemName" @click="f.editing=!f.editing">{{ f.displayName }}</div>
                    <div class="itemControl">
                        <div class="fileSize" :style="{backgroundColor:f.file.size>10*1000*1000?'red':''}">{{ fileSizeStr(f.file.size)}}</div>
                        <button class="ok" @click="commit(i)">上传</button>
                        <button class="cancel" @click="delFile(i)">取消</button>
                    </div>
                </div>
                <div class="nameEdit" v-show="f.editing">
                    <div>
                        <input v-model="f.displayName"/>
                        <button @click="f.editing=false" class="ok">OK</button>
                    </div>
                </div>
            </div>
        </div>
        <div class="controls">
            <div class="uploadBtn" :class="{disabled:props.single&&fileList.length>=1}" @click="fileInput?.showPicker()">
                选择文件
            </div>
            <div v-if="!props.single" class="uploadBtn" @click="dirInput?.showPicker()">
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
.nameEdit div{
    display: flex;
    flex-direction: column;
    align-items: center;
}
.nameEdit{
    position: absolute;
    top:32px;left:10px;
    background-color: #aaa;
    border:2px solid white;
    padding-left: 10px;
    height: 70px;
    width:170px;
    z-index: 100;
    border-radius: 10px;
    display: flex;
    flex-direction: column;
    justify-content: space-around;
    align-items:start;
    color:white
}

.itemControl button,.fileSize{
    margin: auto 0px auto 0px;
    padding-top: 0px;
    width: 46px;
    height: 22px;
    line-height: 22px;
    text-align: center;
    border-radius: 2px;
    color:white;
    cursor: pointer;
}
.itemControl{
    display: flex;
    flex-direction: row;
    align-items: center;
    justify-content: center;
    gap:2px
}
.itemName:hover{
    text-decoration: underline;
}
.itemName{
    overflow:hidden;
    text-overflow: ellipsis;
    cursor: pointer;
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

.fileBody{
    white-space: nowrap;
    background-color: white;
    border-radius:5px;
    padding: 4px;
    position: relative;
    text-align: center;
    height:50px;
    width: 200px;
    overflow: hidden;
    text-overflow: ellipsis;
    display: flex;
    flex-direction: column;
    justify-content: space-between;
}
.fileSize{
    background-color: #666;
    color:white;
    font-size: small;
}
.fileItem{
    margin: 5px 0px 5px 0px;
    padding: 0px 5px 0px 5px;
    display: flex;
    align-items: center;
    gap:5px;
    position: relative;
}
.staging.single{
    height: 70px;
    overflow: hidden;
}
.staging{
    width: 220px;
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
    height: 50px;
}
.fileUpload{
    display: flex;
    flex-direction: column;
    width: fit-content;
}
.uploadBtn.disabled{
    background-color: #999;
    cursor:not-allowed;
}
.uploadBtn.disabled:hover{
    background-color: #999;
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
.fileUpload{
    margin: 0px auto 0px auto;
}
</style>