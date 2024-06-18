<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { injectApi } from '../provides';
import { Api } from '../utils/com/api';

const show = ref<boolean>(false);
const way = ref<string|undefined>();
let api:Api;
function setShow(s:boolean){
    show.value = s;
    if(s){
        getWay()
    }
}
defineExpose({setShow})

async function getWay(){
    way.value = await api.etc.utils.applyBeingMember();
}
onMounted(()=>{
    api = injectApi();
})
</script>

<template>
    <div v-if="show" class="needMemberWarning fixFill">
        <div class="background fixFill"></div>
        <div class="panel">
            <h2>无身份</h2>
            <div class="mainInfo">您需要申请正式成员身份<br/>才能进行编辑性质操作</div>
            <div>{{ way }}</div>
            <button @click="setShow(false)">知道了</button>
        </div>
    </div>
</template>

<style scoped>
.mainInfo{
    color: #999
}
.needMemberWarning{
    z-index: 30000;
    display: flex;
    flex-direction: column;
    justify-content: center;
    align-items: center;
}
.background{
    background-color: black;
    opacity: 0.2;
}
.panel{
    border-radius: 10px;
    box-shadow: 0 0 10px 0 black;
    height: 200px;
    width: 300px;
    background-color: white;
    z-index: 30001;
    display: flex;
    flex-direction: column;
    justify-content: space-around;
    align-items: center;
    text-align: center;
}
</style>