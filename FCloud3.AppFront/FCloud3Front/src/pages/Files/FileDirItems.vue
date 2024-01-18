<script setup lang="ts">
import { TakeContentResItem } from '../../models/files/fileDir';
import { fileSizeStr, getFileIconStyle, getFileExt} from '../../utils/fileUtils';

const props = defineProps<{
    items: Array<TakeContentResItem>|undefined
}>()
</script>

<template>
<div v-if="props.items" class="dirItems">
    <div class="item" v-for="item in props.items">
        <div class="iconName">
            <div class="icon" :style="getFileIconStyle(item.Name)">{{ getFileExt(item.Name) }}</div>
            <div class="name">
                <a :href="item.Url" target="_blank">{{ item.Name }}</a> 
            </div>
        </div>
        <div class="size">
            {{ fileSizeStr(item.ByteCount) }}
        </div>
    </div>
</div>
</template>

<style scoped>
.iconName{
    display: flex;
    align-items: center;
    gap:10px;
    flex-grow: 1;
}
.icon{
    width: 20px;
    height: 20px;
    line-height: 20px;
    text-align: center;
    border: 1px solid white;
    border-radius: 3px;
    font-size: 10px;
    color:white;
}
.name{
    flex-grow: 0;
    flex-shrink: 0;
    text-align: left;
}
.item{
    display: flex;
    flex-direction: row;
    justify-content:space-between;
    align-items: center;
    gap:10px;
    padding: 5px 5px 5px 0px;
}
</style>