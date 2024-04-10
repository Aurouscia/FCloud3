<script setup lang="ts">
import { nextTick, onMounted, ref, watch } from 'vue';

const props = defineProps<{
    from:string,
    to:string,
    removed:[number,number][],
    added:[number,number][],
}>();

const from = ref<HTMLDivElement>();
const to = ref<HTMLDivElement>();

watch(props, async()=>{
    await refreshDisplay();
})
onMounted(async()=>{
    await refreshDisplay();
})

async function refreshDisplay(){
    await nextTick();
    let addedRanges:Range[] = props.added.map(([start, end])=>{
        const range = document.createRange();
        if(to.value){
            range.setStart(to.value.childNodes[0], start);
            range.setEnd(to.value.childNodes[0], end);   
        }
        return range;
    })
    let removedRanges:Range[] = props.removed.map(([start, end])=>{
        const range = document.createRange();
        if(from.value){
            range.setStart(from.value.childNodes[0], start);
            range.setEnd(from.value.childNodes[0], end);   
        }
        return range;
    })
    const addedHighlight = new Highlight(...addedRanges);
    const removedHighlight = new Highlight(...removedRanges);
    CSS.highlights.clear()
    CSS.highlights.set('added', addedHighlight);
    CSS.highlights.set('removed', removedHighlight);
}
</script>

<template>
<div class="diffContentDetail">
    <div class="from" ref="from">{{ $props.from }}</div>
    <div class="to" ref="to">{{ $props.to }}</div>
</div>
</template>

<style scoped>
.diffContentDetail{
    display: flex;
    gap:10px;
    flex-direction: row;
    justify-content: center;
    align-items: center;
    flex-grow: 1;
}
.from,.to{
    height: 100%;
    width: 50%;
    flex-grow: 1;
    overflow-x: hidden;
    overflow-y: scroll;
    white-space: pre-wrap;
    box-sizing: border-box;
    padding: 20px;
    background-color: #eee;
}
@media screen and ( max-width: 1000px ){
    .diffContentDetail{
        flex-direction: column;
    }
    .from,.to{
        width: 100%;
        height: 50%;
    }
}

::highlight(added) {
  background-color: green;
  color: white;
}
::highlight(removed) {
  background-color: red;
  color: white;
}
</style>