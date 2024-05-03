<script setup lang="ts">
import { ref } from 'vue';
import { WikiRecommendModel } from '../../models/wikiParsing/wikiRecommend';
import { injectApi } from '../../provides';
import Loading from '../../components/Loading.vue';
import { jumpToDirFromId } from '../Files/routes';
import { jumpToViewWiki } from './routes';

const props = defineProps<{
    pathName:string
}>()
const api = injectApi();
const model = ref<WikiRecommendModel>()
api.wikiParsing.wikiParsing.getRecommend(props.pathName).then(x=>model.value=x)
</script>

<template>
    <div v-if="model" class="recs">
        <div class="list dirs">
            <div v-for="d in model.Dirs" @click="jumpToDirFromId(d.Id)">
                {{ d.Name }}
            </div>
        </div>
        <div class="list">
            <div v-for="w in model.Wikis" @click="jumpToViewWiki(w.UrlPathName)">
                {{ w.Title }}
            </div>
        </div>
    </div>
    <Loading v-else></Loading>
</template>

<style scoped lang="scss">
.list{
    display: flex;
    gap: 10px;
    flex-wrap: wrap;
    padding: 5px 0px 5px 0px;
    justify-content: center;
}
.list div{
    padding: 6px;
    background-color: #e8e8e8;
    white-space: nowrap;
    cursor: pointer;
    &:hover{
        background-color: #ccc;
    }
}
.dirs div{
    background-color: #333;
    color: white;
    &:hover{
        background-color: #666;
    }
}
.recs{
    margin: 20px 0px 20px 0px;
    padding: 10px;
    background-color: #f8f8f8;
}
</style>