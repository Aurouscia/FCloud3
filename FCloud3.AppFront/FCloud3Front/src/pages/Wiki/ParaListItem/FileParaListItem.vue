<script setup lang="ts">
import icon from '@/assets/paraTypes/filePara.svg'
import { canDisplayAsImage, isImageFile } from '@/utils/fileUtils';
import { WikiParaDisplay } from '@/models/wiki/wikiPara'
import { useParaListItem } from './paraListItemTitle';
import { imgClickJumpRequireClassName } from '@/utils/wikiView/imgClickJump';

const props = defineProps<{
    w:WikiParaDisplay,
    noTitle?:boolean
}>();
const {mainname, subname} = useParaListItem(props);
</script>

<template>
    <div class="paraListItem" :class="{noTitle}">
        <div v-if="!noTitle" class="title">
            <img class="icon" :src="icon">
            <div class="mainName">{{ mainname }}</div>
            <span>{{ subname }}</span>
        </div>
        <img class="fileImage" :class="[imgClickJumpRequireClassName]" v-if="canDisplayAsImage(w.Content, w.Bytes)" :src="props.w.Content" />
        <div v-else class="fileLink">
            <a :href="props.w.Content">
                {{ props.w.Title }}<br /><span>{{ props.w.Content }}</span><br/>
                <span v-if="isImageFile(w.Content)" class="noDisplayImage">(图片过大，不显示仅提供下载)</span>
            </a>
        </div>
    </div>
</template>

<style scoped>
.fileImage{
    width: 100%;
    height: 100px;
    object-fit: contain;
    position: absolute;
    top:5px;bottom:5px;
    left:0px;right:-40px
}
.paraListItem{
    text-align: center;
}
.paraListItem.noTitle{
    height: 100px;
    display: flex;
    justify-content: center;
    align-items: center;
    font-size: 18px;
}
.fileLink{
    word-wrap: break-word;
    word-break: break-all;
}
a{
    font-size: 16px;
}
.noDisplayImage{
    font-size: 14px;
}
</style>