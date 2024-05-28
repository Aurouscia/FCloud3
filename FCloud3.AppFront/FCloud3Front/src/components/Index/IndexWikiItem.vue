<script setup lang="ts">
import { IndexQuery, IndexResult } from './index.ts';
import { Api } from '@/utils/com/api';
import Index, { IndexColumn } from './Index.vue';
import { inject, onMounted, ref } from 'vue';

interface WikiItemIndexItem{
    Id:number,
    Title:string,
    Update:string
}
const columns:IndexColumn[] = 
[
    {name:'Id',alias:'号儿',canSearch:true,canSetOrder:false},
    {name:'Title',alias:'标题',canSearch:true,canSetOrder:true},
    {name:'Update',alias:'上次更新',canSearch:false,canSetOrder:true},
]
const data = ref<WikiItemIndexItem[]>();
function render(i:IndexResult){
    data.value = [];
    i.Data.forEach(r=>{
        data.value?.push({
            Id: parseInt(r[0]),
            Title:r[1],
            Update:r[2]
        })
    })
}

var api:Api;
const ok = ref<boolean>(false);
const q = ref<IndexQuery>({
    Page:1,PageSize:5,
})
onMounted(()=>{
    api = inject('api') as Api;
    ok.value = true;
})
</script>

<template>
    <Index v-if="ok" :columns="columns" :fetch-index="api.wiki.index" :q-init="q" 
        @reload-data="render" :hide-head="true" :hide-page="true">
        <tr v-for="item in data">
            <td>
                {{ item.Id }}
            </td>
            <td>
                {{ item.Title }}
            </td>
            <td>
                {{ item.Update }}
            </td>
        </tr>
    </Index>
</template>

<style scoped>

</style>