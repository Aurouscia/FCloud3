<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { TakeContentResponse } from '../../models/files/TakeContentResponse';
import FileChildren from './fileChildren.vue'
import { useRouter } from 'vue-router';
import _ from 'lodash';

const router = useRouter();

function jumpToSubDir(name:string){
    router.replace({name:'files',params:{path: _.concat(props.path,name)}});
}

const props = defineProps<{
    dirId:number,
    path:string[]|string,
    fetchFrom:(dir:number)=>Promise<TakeContentResponse|undefined>
}>();
const data = ref<TakeContentResponse>();
onMounted(async()=>{
    data.value = await props.fetchFrom(props.dirId);
})
</script>

<template>
    <div class="fileChildren">
        <div v-for="item in data?.SubDirs">
            <div class="subdir">
                <div>
                    <div class="foldBtn" v-show="!item.showChildren" @click="item.showChildren = true" style="color:#999">▶
                    </div>
                    <div class="foldBtn" v-show="item.showChildren" @click="item.showChildren = false" style="color:black">▼
                    </div>
                    <div class="subdirName" @click="jumpToSubDir(item.Name)">{{ item.Name }}</div>
                </div>
                <div>
                </div>
            </div>
            <div class="detail" v-if="item.showChildren">
                <FileChildren :dir-id="item.Id" :path="_.concat(props.path, item.Name)" :fetch-from="props.fetchFrom">
                </FileChildren>
            </div>
        </div>
        <div v-if="data?.SubDirs.length == 0" class="emptyDir">
            空文件夹
        </div>
    </div>
</template>

<style scoped>
.subdirName:hover{
    text-decoration: underline;
    cursor: pointer;
}
.emptyDir{
    text-align: center;
    font-size: small;
}
.foldBtn{
    width: 20px;
    overflow: visible;
    cursor: pointer;
}
.fileChildren{
    padding-left: 15px;
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
}
</style>