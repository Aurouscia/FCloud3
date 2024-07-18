<script setup lang="ts">
import { inject, onBeforeMount, ref } from 'vue';
import FileUpload from '@/components/FileUpload.vue';
import Search from '@/components/Search.vue';
import SwitchingTabs from '@/components/SwitchingTabs.vue';
import { Api } from '@/utils/com/api';
import { FileItemDetail } from '@/models/files/fileItem';
import { fileLocationShow } from '@/utils/fileUtils';
import { useFilesRoutesJump } from '../Files/routes/routesJump';

const { jumpToDir } = useFilesRoutesJump()
const props = defineProps<{
    paraId?:number,
    fileId?:number,
}>();

const search = ref<InstanceType<typeof Search>>();
async function setFileId(id:number){
    if(!props.paraId){return;}
    const res = await api.wiki.wikiPara.setFileParaFileId(props.paraId, id);
    if(res){
        emit('fileIdSet');
        search.value?.clear();
        await loadDetail(id);
    }
}

const detail = ref<FileItemDetail>();
async function loadDetail(fileId?:number){
    if(!fileId){
        fileId = props.fileId;
    }
    if(!fileId){return;}
    api.files.fileItem.getDetail(fileId).then(res=>{
        if(res){
            detail.value = res;
        }
    });
}
function jumpToLocation(){
    if(detail.value?.DirPath){
        if(detail.value.DirPath.length>0)
            jumpToDir(detail.value.DirPath)
        else
            jumpToDir(['homeless-items'])
    }
}
var api:Api;
onBeforeMount(()=>{
    //如果mount之后再inject会造成Search组件认为api是undefined
    api = inject('api') as Api;
    loadDetail();
})
const emit = defineEmits<{
    (e:'fileIdSet'):void
}>()
</script>

<template>
    <div class="wikiFileParaEdit">
        <h1>文件段落设置</h1>
        <div class="detail" v-if="detail">
            文件：<span class="detailValue">{{ detail.ItemInfo.DisplayName }}</span><br/>
            路径：<span class="detailValue path" @click="jumpToLocation">{{ fileLocationShow(detail.DirFriendlyPath) }}</span><br/>
        </div>
        <SwitchingTabs :texts="['选择已有','新上传']">
            <div>
                <Search ref="search" :source="api.etc.quickSearch.fileItem" @done="(_val,id)=>setFileId(id)"></Search>
            </div>
            <FileUpload :dist="'wikiFile'" :single="true" @uploaded="setFileId">
            </FileUpload>
        </SwitchingTabs>
    </div>
</template>

<style scoped>
.path:hover{
    cursor: pointer;
    text-decoration: underline;
}
.detailValue{
    color:gray;
}
.detail{
    white-space: nowrap;
    overflow-x: auto;
    padding: 10px;
    margin-bottom: 10px;
    background-color: #eee;
    border-radius: 5px;
    line-height: 2em;
}
</style>