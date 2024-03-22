<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { getTopbarModel } from './topbarModel';
import itemsImg from '../../assets/items.svg';
import { TopbarModel } from './topbarModel';
import TopbarBodyHorizontal from './TopbarBodyHorizontal.vue';
import TopbarBodyVertical from './TopbarBodyVertical.vue';

const topbarModel = ref<TopbarModel>();
onMounted(async()=>{
    topbarModel.value = await getTopbarModel();
})

const topbarBodyVert = ref<InstanceType<typeof TopbarBodyVertical>>();
function toggleFold(){
    topbarBodyVert.value?.toggleFold();
}
</script>

<template>
<div class="topbarParent">
    <div class="logo">
        <img src="/fcloud.svg" alt="fcloudLogo" />
        <div class="logoText">
            <div class="logoText1">
                FicCloud
            </div>
            <div class="logoText2">
                架空世界云
            </div>
        </div>
    </div>
    <div v-if="topbarModel" class="topbarHori">
        <TopbarBodyHorizontal :data="topbarModel"></TopbarBodyHorizontal>
    </div>
    <div class="foldBtn">
        <img :src="itemsImg" @click="toggleFold"/>
    </div>
</div>
<div v-if="topbarModel" class="topbarVert">
    <TopbarBodyVertical :data="topbarModel" ref="topbarBodyVert"></TopbarBodyVertical>
</div>
</template>

<style scoped lang="scss">
.logo{
    width: 130px;
    height: 42px;
    position: absolute;
    left: 10px;
    display: flex;
    align-items: center;
    gap:5px;
    img{
        object-fit: contain;
        height: 40px;
        width: 40px;
    }
    .logoText{
        display: flex;
        flex-direction: column;
        justify-content: center;
        text-align: center;
        gap: 0px;
        color: #333;
        .logoText1{
            font-size: 18px;
        }
        .logoText2{
            font-size: 10px;
            letter-spacing: 4px;
        }
    }
}

.topbarParent{
    position: fixed;
    top: 0px;
    left: 0px;
    right: 0px;
    height: var(--top-bar-height);
    z-index: 1000;
    display: flex;
    justify-content: left;
    align-items: center;
    box-shadow: 0px 0px 10px 0px rgba(0,0,0,0.7);
    background-color: white;
}

.foldBtn{
    position: absolute;
    right: 10px;
    height: 25px;
    cursor: pointer;
    img{
        width: 25px;
        height: 25px;
        object-fit: contain;
    }
}

.topbarHori{
    display: none;
    margin-left: 140px;
}
@media screen and (min-width: 500px){
    .foldBtn{
        display: none;
    }
    .topbarHori{
        display: block;
    }
    .topbarVert{
        display: none;
    }
}
</style>