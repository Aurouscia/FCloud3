<script setup lang="ts">
import { useRouter } from 'vue-router';
import { TopbarModel, TopbarModelItem } from './topbarModel';
import { useMessageRoutesJump } from '@/pages/Message/routes/routesJump';
import { useNotifCountStore } from '@/utils/globalStores/notifCount';
import { storeToRefs } from 'pinia';

const { unreadNotifCount } = storeToRefs(useNotifCountStore())
const { jumpToNotifs } = useMessageRoutesJump();
const router = useRouter();
defineProps<{
    data: TopbarModel
}>();
function clickHandler(item: TopbarModelItem){
    if(item.Link)
        router.push(item.Link);
}
</script>
<template>
<div class="topbarBodyHorizontal">
    <div v-for="i in data.Items" class="topbarItem">
        <div class="topbarText" @click="clickHandler(i)">
            {{ i.Title }}
        </div>
        <div v-if="i.SubItems" class="topbarSubItemList">
            <div v-for="si in i.SubItems" @click="router.push(si.Link)">
                {{ si.Title }}
            </div>
        </div>
    </div>
    <div class="topbarItem">
        <div class="topbarText" @click="jumpToNotifs">消息</div>
        <div v-show="unreadNotifCount>0" class="notifExists"></div>
    </div>
</div>
</template>

<style scoped lang="scss">
.notifExists{
    position: absolute;
    top: 6px;
    right: 6px;
}
.topbarBodyHorizontal{
    display: flex;
    justify-content: left;
    align-items: center;
    user-select: none;
    border-left: 1px solid #aaa;
    padding-left: 15px;
}
.topbarText {
    cursor: pointer;
    font-size: 20px;
    color: #333;
    padding: 6px 10px 6px 10px;
}
.topbarItem{
    position: relative;
    border-radius: 4px;
    &:hover{
        background-color: rgb(213, 213, 213);
        .topbarSubItemList{
            display: block;
        }
        .topbarText{
            color: black;
        }
    }
}
.topbarSubItemList{
    position: absolute;
    white-space: nowrap;
    display: none;
    top: 38px;
    left: -3px;
    min-width: 100px;
    background-color: white;
    box-shadow: 0 0 5px 0 rgba(0,0,0,0.8);
    border-radius: 5px;
    padding: 5px;
    div{
        cursor: pointer;
        font-size: 18px;
        color: #888;
        border-radius: 4px;
        margin: 4px;
        padding: 10px;
        &:hover {
            color: #333;
            background-color: rgb(213, 213, 213);
        }
    }
}
</style>