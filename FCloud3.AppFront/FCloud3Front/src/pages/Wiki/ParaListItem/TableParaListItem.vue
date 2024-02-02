<script setup lang="ts">
import icon from '../../../assets/paraTypes/tablePara.svg'
import SimpleTable from '../../../components/SimpleTable.vue';
import { WikiPara } from '../../../models/wiki/wikiPara';
import './style.css'

const props = defineProps<{
    w:WikiPara
}>();
</script>

<template>
    <div class="paraListItem">
        <div class="title">
            <img class="icon" :src="icon">
            <div>{{ props.w.Title }}</div>
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
table{
    table-layout: fixed;
    width:calc(100%);
}
</style>