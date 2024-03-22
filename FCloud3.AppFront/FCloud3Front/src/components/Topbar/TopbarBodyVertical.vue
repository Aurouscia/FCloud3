<script setup lang="ts">
import { useRouter } from 'vue-router';
import { TopbarModel, TopbarModelItem } from './topbarModel';
import { ref } from 'vue';
import foldImg from '../../assets/fold.svg';

const router = useRouter();
const props = defineProps<{
    data: TopbarModel
}>();
function clickHandler(item: TopbarModelItem){
    if(item.SubItems){
        item.IsActive = !item.IsActive;
        return;
    }
    if(item.Link){
        router.push(item.Link);
        folded.value = true;
    }
}

const folded = ref<boolean>(true);
function toggleFold(){
    folded.value = !folded.value;
    if(!folded.value){
        props.data.Items.forEach(i=>{
            i.IsActive = false;
        });
    }
}
defineExpose({
    toggleFold
});
</script>

<template>
<div class="topbarBodyVertical" :class="{folded}">
    <div v-for="i in data.Items" class="topbarItem">
        <div class="topbarText" @click="clickHandler(i)">
            <span>{{ i.Title }}</span>
            <img v-show="i.SubItems" :src="foldImg" :class="{activeSubFoldImg:i.IsActive}"/>
        </div>
        <div v-if="i.SubItems && i.IsActive" class="topbarSubItemList">
            <div v-for="si in i.SubItems" @click="router.push(si.Link);folded=true">
                {{ si.Title }}
            </div>
        </div>
    </div>
</div>
</template>

<style scoped lang="scss">
$topbar-body-vert-width: 180px;
.topbarBodyVertical{
    position: fixed;
    top: 0px;
    right: 0px;
    height: 100vh;
    width: $topbar-body-vert-width;
    transition: 0.5s;
    box-shadow: 0px 0px 12px 0px black;
    background-color: white;
    z-index: 900;
    padding-top: 60px;
    display: flex;
    flex-direction: column;
    justify-content: top;
    align-items: left;
    user-select: none;
}
.topbarBodyVertical.folded{
    right: - $topbar-body-vert-width;
    box-shadow: 0px 0px 0px 0px black;
}
.topbarText{
    display: flex;
    justify-content: space-between;
    align-items: center;
    img{
        width: 14px;
        height: 14px;
        transform: rotate(90deg);
        object-fit: contain;
        transition: 0.2s;
    }
    img.activeSubFoldImg{
        transform: rotate(0deg);
    }
}
.topbarItem{
    padding: 10px;
    font-size: 20px;
    border-bottom: 1px solid #ddd;
    color: #333;
    cursor: pointer;
    &:hover{
        background-color: #eee;
        color: black;
    }
}
.topbarSubItemList{
    font-size: 16px;
    margin:10px 0px 0px 8px;
    div{
        border-top: 1px solid #ddd;
        padding: 8px;
        color: #666;
        &:hover{
            background-color: #ccc;
            color: black;
        }
    }
}
</style>