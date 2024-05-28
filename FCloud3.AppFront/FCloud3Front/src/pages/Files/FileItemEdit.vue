<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { injectApi } from '@/provides';
import { Api } from '@/utils/com/api';
import SideBar from '@/components/SideBar.vue';
import { fileNameWithoutExt, getFileExt, canDisplayAsImage } from '@/utils/fileUtils';
import { FileDirItem } from '@/models/files/fileDir';
import { useIdentityRoutesJump } from '@/pages/Identities/routes/routesJump';

const { jumpToUserCenter } = useIdentityRoutesJump();
const editingFileId = ref<number>();
const editingFileName = ref<string>();
const editingFileExt = ref<string>();
const editingFileBytes = ref<number>();
const editingFileUrl = ref<string>();
const editFileSidebar = ref<InstanceType<typeof SideBar>>();
const owner = ref<string>();
let ok:(newName:string|-1)=>void
async function startEditingFile(f:FileDirItem, okCallBack:(newName:string|-1)=>void){
    editingFileId.value = f.Id;
    editingFileName.value = fileNameWithoutExt(f.Name);
    editingFileExt.value = getFileExt(f.Name, false, true);
    editingFileBytes.value = f.ByteCount;
    editingFileUrl.value = f.Url;
    owner.value = f.OwnerName;
    editFileSidebar.value?.extend();
    ok = okCallBack;
}
async function editingFileOk(){
    if(!editingFileId.value){
        return false;
    }
    const fullName = `${editingFileName.value}${editingFileExt.value}`;
    const res = await api.fileItem.editInfo(editingFileId.value, fullName);
    if(res){
        editingFileName.value = undefined;
        editingFileExt.value = undefined;
        editingFileId.value = undefined;
        owner.value = undefined;
        editFileSidebar.value?.fold();
        ok(fullName)
    }
}
async function deleteFile() {
    if(!editingFileId.value){
        return false;
    }
    const res = await api.fileItem.deleteFile(editingFileId.value);
    if(res){
        editingFileName.value = undefined;
        editingFileExt.value = undefined;
        editingFileId.value = undefined;
        editFileSidebar.value?.fold();
        ok(-1)
    }
}

defineExpose({
    startEditingFile
})
const emit = defineEmits<{
    (e:'needRefresh'):void
}>()
let api:Api;
onMounted(()=>{
    api = injectApi();
})
</script>

<template>
    <SideBar ref="editFileSidebar">
        <h1>编辑文件信息</h1>
        <div class="fileEdit">
            <div class="owner">文件所有者 <span @click="jumpToUserCenter(owner||'??')">{{ owner }}</span></div>
            <img v-if="canDisplayAsImage(editingFileExt||'', editingFileBytes||0)" :src="editingFileUrl"/>
            <div>
                <input v-model="editingFileName" type="text" />{{ editingFileExt }}
            </div>
            <button class="ok" @click="editingFileOk">重命名</button>
            <button class="danger" style="margin-top: 100px;" @click="deleteFile">删除该文件</button>
        </div>
    </SideBar>
</template>

<style scoped>
.owner{
    color: #999;
    margin-bottom: 10px;
}
.owner span{
    font-weight: bold;
    cursor: pointer;
}
.owner span:hover{
    text-decoration: underline;
}
.fileEdit{
    display: flex;
    flex-direction: column;
    align-items: center;
}
img{
    max-width: 200px;
    max-height: 200px;
    object-fit: contain;
}
</style>