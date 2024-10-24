<script setup lang="ts">
import { ref } from 'vue';
import { injectApi } from '@/provides';
import { canDisplayAsImage, fileSizeStr, fileLocationShow } from '@/utils/fileUtils';
import { FileItemDetail } from '@/models/files/fileItem';
import Loading from '@/components/Loading.vue';
import { fileDownloadLink } from '@/utils/com/api';
import { useFilesRoutesJump } from './routes/routesJump';

const { jumpToDir } = useFilesRoutesJump();
const props = defineProps<{
    fileItemId:string
}>();
const id = parseInt(props.fileItemId);
const api = injectApi();
const failed = ref(false);
const fileInfo = ref<FileItemDetail>();
if(!isNaN(id) && id>0){
    api.files.fileItem.getDetail(id).then(f=>{
        if(f){
            fileInfo.value = f;
        }
    })
}
else{
    failed.value = true
}

function jumpToLocation(){
    if(fileInfo.value?.DirPath){
        if(fileInfo.value.DirPath.length>0)
            jumpToDir(fileInfo.value.DirPath)
        else
            jumpToDir(['homeless-items'])
    }
}
</script>

<template>
    <h1>文件详情</h1>
    <div v-if="fileInfo && !failed" class="fileItemView">
        <img v-if="canDisplayAsImage(fileInfo.ItemInfo.StorePathName, fileInfo.ItemInfo.ByteCount)" :src="fileInfo.FullUrl"/>
        <div class="fileName">
            <a :href="fileInfo.FullUrl">{{ fileInfo.ItemInfo.DisplayName }}</a> ({{ fileSizeStr(fileInfo.ItemInfo.ByteCount) }})</div>
        <div class="loc" @click="jumpToLocation">
            文件位置: {{ fileLocationShow(fileInfo.DirFriendlyPath) }}
        </div>
        <a :href="fileDownloadLink(fileInfo.ItemInfo.Id)" download target="_blank" class="downloadBtn">下载</a>
    </div>
    <Loading v-else-if="!failed"></Loading>
    <div v-if="failed">可能原因：操作记录在旧版记录，无法自动跳转</div>
</template>

<style scoped lang="scss">
@use '@/styles/globalValues';

.fileItemView{
    text-align: center;
    display: flex;
    flex-direction: column;
    gap: 10px;
    align-items: center;
}
.loc{
    cursor: pointer;
    color: #999;
    &:hover{
        text-decoration: underline;
    }
}
.fileName{
    font-size: 20px;
    color: #999;
}
img{
    max-width: 70vw;
    max-height: calc(70vh - globalValues.$topbar-height);
    object-fit: contain;
    border-radius: 5px;
}
.downloadBtn{
    background-color: cornflowerblue;
    color: white;
    padding: 5px;
    border-radius: 5px;
    transition: 0.3s;
    &:hover{
        background-color:rgb(0, 49, 139);
        text-decoration: none;
    }
}
</style>