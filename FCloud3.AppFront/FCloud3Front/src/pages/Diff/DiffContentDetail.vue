<script setup lang="ts">
import { DiffContentType } from '../../models/diff/DiffContentType';

const props = defineProps<{
    type:DiffContentType,
    objId:number
}>();
</script>

<template>
<div class="diffContentDetail">
    <div class="from"></div>
    <div class="to"></div>
</div>
</template>

<style scoped>
.diffContentDetail{
    display: flex;
    flex-direction: row;
    justify-content: center;
    align-items: center;
}
.from,.to{
    height: 100%;
    flex-grow: 1;
    overflow-x: hidden;
    overflow-y: scroll;
}
@media screen and ( max-width: 800px ){
    .diffContentDetail{
        flex-direction: column;
    }
    .from,.to{
        width: 100%;
    }
}
</style>