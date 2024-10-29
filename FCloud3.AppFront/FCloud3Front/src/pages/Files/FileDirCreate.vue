<script setup lang="ts">
import { inject, onMounted } from 'vue';
import { Api } from '@/utils/com/api';
import { useUrlPathNameConverter } from '@/utils/urlPathName';

const props = defineProps<{
    dirId:number,
    dirName:string
}>();

const {name:creatingDirName, converted:creatingDirUrlPathName, run:runAutoUrl} = useUrlPathNameConverter();

async function create() {
    if(!creatingDirName.value||!creatingDirUrlPathName.value){return;}
    const res = await api.files.fileDir.create(props.dirId,creatingDirName.value,creatingDirUrlPathName.value);
    if(res){
        emit('created',creatingDirUrlPathName.value);
        creatingDirName.value = "";
        creatingDirUrlPathName.value = "";
    }
}

const emit = defineEmits<{
    (e:'created',urlPathName:string):void
}>()

var api:Api
onMounted(()=>{
    api = inject('api') as Api;
})
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