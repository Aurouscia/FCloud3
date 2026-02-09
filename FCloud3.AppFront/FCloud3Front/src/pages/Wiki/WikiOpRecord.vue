<script lang="ts" setup>
import OpRecord from '@/components/Messages/OpRecord.vue';
import { injectApi } from '@/provides';
import { recoverTitle, setTitleTo } from '@/utils/titleSetter';
import { computed, onMounted, onUnmounted, ref, watch } from 'vue';

const props = defineProps<{
    wikiId: string
}>()
const wikiIdNum = computed(()=>Number(props.wikiId))

const api = injectApi()
const wikiTitle = ref<string>()
watch(()=>wikiIdNum.value, (id)=>{
    if(id){
        api.wiki.wikiItem
            .getInfoById(wikiIdNum.value)
            .then(v=>{
                if(v){
                    wikiTitle.value = v.Title
                }
            })
    }
},{immediate: true})

onMounted(()=>{
    setTitleTo('操作记录')
})
onUnmounted(()=>{
    recoverTitle()
})
</script>

<template>
<h1>操作记录<span class="wikiTitle">({{ wikiTitle }})</span></h1>
<div v-if="wikiIdNum">
    <OpRecord :wiki-id="wikiIdNum"></OpRecord>
</div>
<div v-else>
    请从正确入口进入
</div>
</template>

<style lang="scss" scoped>
.wikiTitle{
    font-size: 0.8em;
    letter-spacing: unset;
}
</style>