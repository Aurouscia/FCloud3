<script setup lang="ts">
import icon from '../../../assets/paraTypes/tablePara.svg'
import SimpleTable from '../../../components/SimpleTable.vue';
import { WikiParaDisplay } from '../../../models/wiki/wikiPara';
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
            {{ mainname }}
            <span>{{ subname }}</span>
        </div>
        <div class="content">
            <SimpleTable :json="w.Content"></SimpleTable>
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
.content{
    display: flex;
    align-items: center;
    justify-content: center;
    margin-top: -25px;
    height: 90px;
}
</style>