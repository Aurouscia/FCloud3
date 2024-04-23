<script setup lang="ts">
import icon from '../../../assets/paraTypes/textPara.svg'
import { WikiParaDisplay } from '../../../models/wiki/wikiPara'

const props = defineProps<{
    w:WikiParaDisplay
}>();
</script>

<template>
    <div class="paraListItem">
        <div class="title">
            <img class="icon" :src="icon">
            {{ props.w.Title }}
        </div>
        <div class="content">{{ props.w.Content }}</div>
    </div>
</template>

<style scoped>

</style>