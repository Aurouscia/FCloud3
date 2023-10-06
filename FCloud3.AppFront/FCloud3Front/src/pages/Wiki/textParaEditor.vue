<script setup lang="ts">
import {ref, watch, onMounted, inject} from 'vue'

const props = defineProps<{id:string}>()
const previewOn = ref<boolean>(true);

const content = ref<string>();
const contentLines = ref<Array<string>>([]);
watch(content,(newVal,_)=>{
    if(newVal){
        contentLines.value = newVal.split('\n');
    }
    else{
        contentLines.value = [];
    }
})
onMounted(()=>{
    const hideTopbar = inject('hideTopbar') as ()=>void;
    hideTopbar();
})
</script>

<template>
<div class="topbar">
    <div>
        <button @click="previewOn=!previewOn">
            {{ previewOn?"预览开":"预览关" }}
        </button>
    </div>
    <div>
        <button @click="()=>{}">
            保存
        </button>
    </div>
</div>
<div class="background">
    <div class="preview" v-show="previewOn">
        <div v-for="l in contentLines">
            {{ l }}
        </div>
    </div>
    <div class="write" :class="{writeNoPreview:!previewOn}">
        <textarea v-model="content" placeholder="请输入内容">
            {{ props.id }}
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