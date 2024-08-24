<script setup lang="ts">
import { AuTable, CellShown } from '@aurouscia/au-table-editor';
import { ref } from 'vue';

const props = defineProps<{
    data:string
}>()
const cells = ref<CellShown[][]>([])
function convert(){
    const data = JSON.parse(props.data)
    const table = new AuTable(data)
    cells.value = table.output()
}
convert()
</script>

<template>
<table class="simpleTable">
    <tr v-for="r in cells">
        <td v-for="c in r" :colspan="c.colspan" :rowspan="c.rowspan" :style="{color:c.textColor, backgroundColor:c.bgColor}">
            {{ c.content || '　'}}
        </td>
    </tr>
</table>
</template>

<style scoped lang="scss">
td{
    white-space: pre-wrap;
}
</style>