<script setup lang="ts">
import icon from '@/assets/paraTypes/textPara.svg'
import { WikiParaDisplay } from '@/models/wiki/wikiPara';
import { useParaListItem } from './paraListItemTitle';

const props = defineProps<{
    w:WikiParaDisplay
}>();
const {mainname, subname} = useParaListItem(props);
</script>

<template>
    <div class="paraListItem">
        <div class="title">
            <img class="icon" :src="icon">
            <div class="mainName">{{ mainname }}</div>
            <span>{{ subname }}</span>
        </div>
        <div class="content">{{ props.w.Content }}</div>
    </div>
</template>

<style scoped>

</style>