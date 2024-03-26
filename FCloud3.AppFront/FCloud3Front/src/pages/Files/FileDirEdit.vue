<script setup lang="ts">
import { inject, onMounted, ref } from 'vue';
import { Api } from '../../utils/api';
import { FileDir } from '../../models/files/fileDir';
import Loading from '../../components/Loading.vue';
import FileUpload from '../../components/FileUpload.vue';
import Search from '../../components/Search.vue';
import SwitchingTabs from '../../components/SwitchingTabs.vue';
import Notice from '../../components/Notice.vue';
import { useUrlPathNameConverter } from '../../utils/urlPathName';
import AuthProblem from '../../components/AuthProblem.vue';

const data = ref<FileDir>();
const props = defineProps<{
    id:number,
    path:string[]
}>()
var api:Api;

async function saveEdit(){
    if(data.value){
        data.value.Name = editingDirName.value||"";
        data.value.UrlPathName = editingDirUrlPathName.value||""
        const resp = await api.fileDir.editDirExe(data.value)
        if(resp){
            emit('infoUpdated',data.value);
        }
    }
} 
async function newFile(newFileItemId:number) {
    const resp = await api.fileDir.putInFile(props.id, newFileItemId);
    if(resp){
        emit('addedNewFile');
    }
}

const moveSearch = ref<InstanceType<typeof Search>>();
async function createWiki() {
    if(!data.value?.Id){
        return;
    }
    const resp = await api.wiki.createInDir(creatingWikiTitle.value||"",creatingWikiUrlPathName.value||"",data.value.Id)
    if(resp){
        creatingWikiTitle.value = "";
        creatingWikiUrlPathName.value = "";
        emit('addedNewFile');
    }
}
async function moveInWiki(_title:string, wikiId:number) {
    const resp = await api.fileDir.putInThings(props.id,[],[],[wikiId]);
    if(resp){
        moveSearch.value?.clear();
        emit('addedNewFile');
    }
}

const {name:editingDirName, converted:editingDirUrlPathName, run:runForDir} = useUrlPathNameConverter();
const {name:creatingWikiTitle, converted:creatingWikiUrlPathName, run:runForWiki} = useUrlPathNameConverter();

const emit = defineEmits<{
    (e: 'infoUpdated', newInfo:FileDir): void
    (e: 'addedNewFile'):void
}>()


onMounted(async()=>{
    api = inject('api') as Api;
    data.value = await api.fileDir.editDir(props.id);
    editingDirName.value = data.value?.Name;
    editingDirUrlPathName.value = data.value?.UrlPathName;
})
</script>

<template>
    <div v-if="data">
        <div>
            <h1>{{ data.Name }}</h1>
        </div>
        <div class="section">
            <SwitchingTabs :texts="['新词条','放词条','传文件','设置']">
                <div>
                    <div v-if="data.CanPutThings">
                        <table>
                            <tr>
                                <td>词条<br/>标题</td>
                                <td><input v-model="creatingWikiTitle" placeholder="必填"/></td>
                            </tr>
                            <tr>
                                <td>链接<br/>名称</td>
                                <td>
                                    <div>
                                        <button class="minor" @click="runForWiki">由标题自动生成</button>
                                    </div>
                                    <input v-model="creatingWikiUrlPathName" placeholder="必填" spellcheck="false"/>
                                </td>
                            </tr>
                            <tr class="noneBackground">
                                <td colspan="2">
                                    <button class="confirm" @click="createWiki">确认</button>
                                </td>
                            </tr>
                        </table>
                        <Notice type="warn">
                            请谨慎设置链接名称，每次修改将导致旧链接失效
                        </Notice>
                    </div>
                    <AuthProblem v-else></AuthProblem>
                </div>
                <div>
                    <div v-if="data.CanPutThings">
                        <Search ref="moveSearch" :source="api.utils.quickSearch.wikiItem" :placeholder="'词条标题'" :allow-free-input="false"
                            :no-result-notice="'无搜索结果'" @done="moveInWiki" ></Search>
                        <Notice type="info">移入词条将不会影响词条在其他文件夹的存在，如果需要“剪切”，请前往其他文件夹点击“移出”</Notice>
                    </div>
                    <AuthProblem v-else></AuthProblem>
                </div>
                <div>
                    <div v-if="data.CanPutThings">
                        <FileUpload @uploaded="newFile" dist="test"></FileUpload>
                        <Notice type="info">文件不能同时放在多个文件夹内</Notice>
                    </div>
                    <AuthProblem v-else></AuthProblem>
                </div>
                <div>
                    <div v-if="data.CanEditInfo">
                        <table>
                            <tr>
                                <td>词条<br/>标题</td>
                                <td><input v-model="editingDirName"/></td>
                            </tr>
                            <tr>
                                <td>链接<br/>名称</td>
                                <td>
                                    <button @click="runForDir" class="minor">由词条标题生成</button><br/>
                                    <input v-model="editingDirUrlPathName" spellcheck="false"/>
                                </td>
                            </tr>
                            <tr class="noneBackground">
                                <td colspan="2">
                                    <button class="confirm" @click="saveEdit">保存</button>
                                </td>
                            </tr>
                        </table>
                    </div>
                    <AuthProblem v-else></AuthProblem>
                </div>
            </SwitchingTabs>
        </div>
    </div>
    <Loading v-else></Loading>
</template>

<style scoped>
td{
    white-space: nowrap;
}
input{
    width: 160px;
}
table{
    margin: 0px auto 0px auto;
}
</style>