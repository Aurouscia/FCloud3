<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { TakeContentResult } from '../../models/files/fileDir';
import FileDirChild from './FileDirChild.vue'
import { useRouter } from 'vue-router';
import _ from 'lodash';
import FileDirItems from './FileDirItems.vue';

const router = useRouter();

function jumpToSubDir(name:string){
    var path =  _.concat(props.path,name);
    path = _.filter(path, x=>!!x)
    router.replace({name:'files',params:{path}});
}
function isEmptyDir(){
    return data.value?.SubDirs.length==0 && data.value?.Items.length==0;
}

const props = defineProps<{
    dirId:number,
    path:string[]|string,
    fetchFrom:(dir:number)=>Promise<TakeContentResult|undefined>
}>();
const data = ref<TakeContentResult>();
onMounted(async()=>{
    data.value = await props.fetchFrom(props.dirId);
})
</script>

<template>
    <div class="fileDirChild">
        <div v-for="subdir in data?.SubDirs">
            <div class="subdir">
                <div>
                    <div class="foldBtn" v-show="!subdir.showChildren" @click="subdir.showChildren = true" style="color:#999">▶
                    </div>
                    <div class="foldBtn" v-show="subdir.showChildren" @click="subdir.showChildren = false" style="color:black">▼
                    </div>
                    <div class="subdirName" @click="jumpToSubDir(subdir.Name)">{{ subdir.Name }}</div>
                </div>
                <div>
                </div>
            </div>
            <div class="detail" v-if="subdir.showChildren">
                <FileDirChild :dir-id="subdir.Id" :path="_.concat(props.path, subdir.Name)" :fetch-from="props.fetchFrom">
                </FileDirChild>
            </div>
        </div>
        <FileDirItems :items="data?.Items"></FileDirItems>
        <div v-if="isEmptyDir()" class="emptyDir">
            空文件夹
        </div>
    </div>
</template>

<style scoped>
.subdirName{
    font-weight: bold;
}
.subdirName:hover{
    text-decoration: underline;
    cursor: pointer;
}
.emptyDir{
    margin-top: 10px;
    text-align: left;
    color:#999;
    font-size: small;
}
.foldBtn{
    width: 20px;
    overflow: visible;
    cursor: pointer;
}
.fileDirChild{
    padding-left: 5px;
    position: relative;
}
.detail{
    display: flex;
    flex-direction: column;
    gap:5px;
    padding-bottom: 10px;
    border-left: 1px solid black;
    border-bottom: 1px solid black;
    margin-left: 11px;
    padding-left: 4px;
}
.subdir div{
    display: flex;
    flex-direction: row;
    justify-content: left;
    align-items: center;
    gap:5px
}
.subdir{
    display: flex;
    flex-direction: row;
    justify-content: left;
    gap:20px;
    align-items: center;
    padding: 4px;
}
</style>