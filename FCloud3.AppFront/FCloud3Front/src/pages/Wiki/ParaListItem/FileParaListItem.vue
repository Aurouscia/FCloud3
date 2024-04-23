<script setup lang="ts">
import icon from '../../../assets/paraTypes/filePara.svg'
import { canDisplayAsImage } from '../../../utils/fileUtils';
import { WikiParaDisplay } from '../../../models/wiki/wikiPara'

const props = defineProps<{
    w:WikiParaDisplay
}>();
</script>

<template>
    <div class="paraListItem">
        <div class="title">
            <img class="icon" :src="icon">
            <div>{{props.w.Title}}</div>
        </div>
        <img class="fileImage" v-if="canDisplayAsImage(w.Content, w.Bytes)" :src="props.w.Content" />
        <div v-else class="fileLink">
            <a :href="props.w.Content">
                {{ props.w.Title }}<br /><span>{{ props.w.Content }}</span>
            </a>
        </div>
    </div>
</template>

<style scoped>
.title div{
    text-shadow:
        0px 3px 0px #eee,
        2px 2px 0px #eee,
        3px 0px 0px #eee,
        2px -2px 0px #eee,
        0px -3px 0px #eee,
        -2px -2px 0px #eee,
        -3px 0px 0px #eee,
        -2px 2px 0px #eee
}
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
    font-size: 18px;
}
.fileLink{
    word-wrap: break-word;
    word-break: break-all;
}
a{
    font-size: 16px;
}
</style>