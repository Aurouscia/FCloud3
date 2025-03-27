<script setup lang="ts">
import { ref } from 'vue';
import { useUrlPathNameConverter } from '@/utils/urlPathName';
import Search from '@/components/Search.vue';
import { injectApi } from '@/provides';

const props = defineProps<{
    dirId:number,
    dirName:string
}>();

const {name:creatingDirName, converted:creatingDirUrlPathName, run:runAutoUrl} = useUrlPathNameConverter();

const asDirId = ref<number>(0);
const asDirFullName = ref<string>();
function asDirSearchDone(value:string, id:number, desc?:string){
    asDirId.value = id;
    if(desc)
        desc = desc+'/'
    asDirFullName.value = desc + value;
}

async function create() {
    if(!creatingDirName.value||!creatingDirUrlPathName.value){return;}
    const res = await api.files.fileDir.create(
        props.dirId, creatingDirName.value, creatingDirUrlPathName.value, asDirId.value);
    if(res){
        emit('created',creatingDirUrlPathName.value);
        creatingDirName.value = "";
        creatingDirUrlPathName.value = "";
    }
}

const emit = defineEmits<{
    (e:'created',urlPathName:string):void
}>()

var api = injectApi()
</script>

<template>
    <div class="dirCreate">
        <div>
            <h1>新建文件夹</h1>
        </div>
        <div class="section">
            <table><tbody>
                <tr><td colspan="2" style="white-space: wrap;">在<b>{{ props.dirName || '根目录' }}</b>下新建文件夹</td></tr>
                <tr>
                    <td>显示<br/>名称</td>
                    <td><input v-model="creatingDirName" placeholder="必填"/></td>
                </tr>
                <tr>
                    <td>路径<br/>名称</td>
                    <td>
                        <div><button class="minor" @click="runAutoUrl">由名称自动生成</button></div>
                        <input v-model="creatingDirUrlPathName" placeholder="必填"/>
                    </td>
                </tr>
                <tr>
                    <td>作为<br/>快捷<br/>方式</td>
                    <td>
                        <div v-if="asDirId>0" class="asDirSelectedPath">
                            已设置：{{ asDirFullName }}
                        </div>
                        <Search :source="api.etc.quickSearch.fileDir" @done="asDirSearchDone"
                            :done-when-click-cand="true" :compact="true"></Search>
                    </td>
                </tr>
                <tr class="noneBackground">
                    <td colspan="2">
                        <button class="confirm" @click="create">确认</button>
                    </td>
                </tr>
            </tbody></table>
        </div>
    </div>
</template>

<style scoped>
.asDirSelectedPath{
    font-size: 12px;
    color:#888;
    margin-bottom: 5px;
    white-space: wrap; 
}
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