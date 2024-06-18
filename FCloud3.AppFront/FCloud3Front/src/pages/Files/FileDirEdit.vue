<script setup lang="ts">
import { inject, onMounted, ref } from 'vue';
import { Api } from '@/utils/com/api';
import { FileDir } from '@/models/files/fileDir';
import Loading from '@/components/Loading.vue';
import FileUpload from '@/components/FileUpload.vue';
import Search from '@/components/Search.vue';
import SwitchingTabs from '@/components/SwitchingTabs.vue';
import Notice from '@/components/Notice.vue';
import { useUrlPathNameConverter } from '@/utils/urlPathName';
import AuthProblem from '@/components/AuthProblem.vue';
import { useFilesRoutesJump } from './routes/routesJump';
import LongPress from '@/components/LongPress.vue';

const data = ref<FileDir>();
const props = defineProps<{
    id:number,
    path:string[]
}>()
var api:Api;

const { jumpToDir } = useFilesRoutesJump();

async function saveEdit(){
    if(data.value){
        data.value.Name = editingDirName.value||"";
        data.value.UrlPathName = editingDirUrlPathName.value||""
        const resp = await api.files.fileDir.editExe(data.value)
        if(resp){
            emit('infoUpdated',data.value);
        }
    }
} 
async function newFile(newFileItemId:number) {
    const resp = await api.files.fileDir.putInFile(props.id, newFileItemId);
    if(resp){
        emit('addedNewFile');
    }
}

const moveSearch = ref<InstanceType<typeof Search>>();
async function createWiki() {
    if(!data.value?.Id){
        return;
    }
    const resp = await api.wiki.wikiItem.createInDir(creatingWikiTitle.value||"",creatingWikiUrlPathName.value||"",data.value.Id)
    if(resp){
        creatingWikiTitle.value = "";
        creatingWikiUrlPathName.value = "";
        emit('addedNewFile');
    }
}
async function moveInWiki(_title:string, wikiId:number) {
    const resp = await api.files.fileDir.putInThings(props.id,[],[],[wikiId]);
    if(resp){
        moveSearch.value?.clear();
        emit('addedNewFile');
    }
}

async function del() {
    const res = await api.files.fileDir.delete(props.id);
    if(res){
        const to = props.path.slice(0, props.path.length - 1)
        jumpToDir(to);
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
    data.value = await api.files.fileDir.edit(props.id);
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
                        <Search ref="moveSearch" :source="api.etc.quickSearch.wikiItem" :placeholder="'词条标题'" :allow-free-input="false"
                            :no-result-notice="'无搜索结果'" @done="moveInWiki" ></Search>
                        <Notice type="info">移入词条将不会影响词条在其他目录的存在，如果需要“剪切”，请前往其他文件夹点击“移出”</Notice>
                    </div>
                    <AuthProblem v-else></AuthProblem>
                </div>
                <div>
                    <div v-if="data.CanPutThings">
                        <FileUpload @uploaded="newFile" dist="test"></FileUpload>
                        <Notice type="info">文件不能同时放在多个目录内</Notice>
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
                        <div style="text-align: center;margin-top: 20px;">
                            <LongPress :reached="del">长按删除本目录</LongPress>
                        </div>
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