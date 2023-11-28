<script setup lang="ts">
import {ref, Ref, onMounted, inject} from 'vue'
import Pop from '../../components/Pop.vue';
import {TextSection} from '../../models/textSection/textSection'
import { Api } from '../../utils/api';

const props = defineProps<{id:string}>()
var textSecId:number = Number(props.id);
const previewOn = ref<boolean>(true);
var api:Api
var pop:Ref<InstanceType<typeof Pop>>;

const data = ref<TextSection>({
    Id:textSecId,
    Content:"",
    Title:""
});

async function replaceTitle() {
    const val = data.value.Title;
    if(!val || val==""){
        pop.value.show("段落标题不可为空","failed");
        return;
    }
    api.textSection.editExe(data.value,pop.value.show)
}

async function init(){
    const resp = await api.textSection.edit(textSecId,pop.value.show);
    if(resp){
        data.value = resp;}
}
onMounted(async()=>{
    pop = inject('pop') as Ref<InstanceType<typeof Pop>>;
    api = inject('api') as Api;
    const hideTopbar = inject('hideTopbar') as ()=>void;
    hideTopbar();

    await init();
})
</script>

<template>
<div class="topbar">
    <div>
        <button @click="previewOn=!previewOn">
            {{ previewOn?"预览开":"预览关" }}
        </button>
        <input v-model="data.Title" placeholder="请输入段落标题" @blur="replaceTitle"/>
    </div>
    <div>
        <button @click="()=>{}">
            保存
        </button>
    </div>
</div>
<div v-if="data" class="background">
    <div class="preview" v-show="previewOn">
        {{ data.Content }}
    </div>
    <div class="write" :class="{writeNoPreview:!previewOn}">
        <textarea v-model="data.Content" placeholder="请输入内容">
        </textarea>
    </div>
</div>
</template>

<style scoped> 
    .background{
        position: fixed;
        width: 100vw;
        top: 50px;
        left: 0px;
        height: calc(100vh - 50px);

        display: flex;
        flex-wrap: wrap;
        justify-content:space-between;
        overflow: hidden;
    }
    .preview{
        width: 50vw;
        height: 100%;
        font-size: 18px;
        overflow-y: scroll;
        word-break: break-all;
    }
    .write{
        width: 50vw;
        height: 100%;
    }
    .write textarea{
        width: 100%;
        height: 100%;
        resize:none;
        border: none !important;
        font-size: 18px;
        font-family:unset;
        overflow-y: scroll;
        word-break: break-all;
    }
    .write textarea:focus{
        border:none
    }
    .writeNoPreview{
        width: 100vw;
    }
    @media screen and (max-width:800px) {
        .preview{
            width: 100vw;
            height: 50%;
        }
        .write{
            width: 100vw;
            height: 50%;
        }
        .writeNoPreview{
            height: 100%;
        }
    }
    .topbar{
        position: fixed;
        top:0px;
        left:0px;
        width: 100vw;
        height: 50px;
        background-color: #ccc;

        display: flex;
        align-items: center;
        justify-content: space-between;
    }
    .topbar > div{
        display: flex;
        padding: 0px 10px 0px 10px;
    }
</style>