<script setup lang="ts">
import { inject, onMounted, ref } from 'vue';
import { Api } from '../../utils/api';
import { FileDir } from '../../models/files/fileDir';
import Loading from '../../components/Loading.vue';
import FileUpload from '../../components/FileUpload.vue';
import Search from '../../components/Search.vue';

const data = ref<FileDir>();
const props = defineProps<{
    id:number,
    path:string[]
}>()
var api:Api;

async function saveEdit(){
    if(data.value){
        const resp = await api.fileDir.editDirExe(data.value)
        if(resp){
            emit('infoUpdated',data.value);
        }
    }
} 
async function newFile(newFileItemId:number) {
    const resp = await api.fileDir.putInFile(props.path, newFileItemId);
    if(resp){
        emit('addedNewFile');
    }
}
async function moveInOrCreateWikiItem(wikiTitle:string) {
    const resp = await api.fileDir
}

const emit = defineEmits<{
    (e: 'infoUpdated', newInfo:FileDir): void
    (e: 'addedNewFile'):void
}>()


onMounted(async()=>{
    api = inject('api') as Api;
    data.value = await api.fileDir.editDir(props.id);
})
</script>

<template>
    <div v-if="data">
        <div>
            <h1>{{ data.Name }}</h1>
        </div>
        <div class="section" v-if="data.CanPutFile">
            <h2>在此上传新文件</h2>
            <FileUpload @uploaded="newFile" dist="test"></FileUpload>
        </div>
        <div class="section" v-if="data.CanPutWiki">
            <h2>在此新建/移入词条</h2>
            <Search :placeholder="'词条标题'" :allow-free-input="true"
                :no-result-notice="'点击确认将新建词条'" @done="(x)=>console.log(x)"></Search>
        </div>
        <div class="section" v-if="data.CanEditInfo">
            <h2>编辑文件夹信息</h2>
            <table>
                <tr>
                    <td>名称</td>
                    <td><input v-model="data.Name"/></td>
                </tr>
                <tr class="noneBackground">
                    <td></td>
                    <td>
                        <button @click="saveEdit">保存</button>
                    </td>
                </tr>
            </table>
        </div>
    </div>
    <Loading v-else></Loading>
</template>

<style scoped>

</style>