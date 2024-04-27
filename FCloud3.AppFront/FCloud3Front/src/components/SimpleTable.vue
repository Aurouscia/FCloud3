<script setup lang="ts">
import _ from 'lodash'
import { onMounted, ref, watch } from 'vue';

const props = withDefaults(defineProps<{
    cells?:string[][]
    json?:string
}>(),{
    cells:()=>[["",""],["",""]]
});

watch(props,()=>{
    refresh();
})
onMounted(()=>{
    refresh();
})

const cells = ref<string[][]>([]);
function refresh(){
    cells.value = props.cells;
    if(props.json){
        try{
            var jsonData = JSON.parse(props.json) as string[][];
            if(_.isArray(jsonData)){
                if(_.every(jsonData, _.isArray)){
                    cells.value = jsonData;
                }
            }
        }
        catch{}
    }
}
</script>

<template>
<table class="simpleTable">
    <tr v-for="r in cells">
        <td v-for="c in r">{{ c || '　'}}</td>
    </tr>
</table>
</template>

<style scoped>
    td{
        background-color: white;
    }
</style>