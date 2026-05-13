<script setup lang="ts">
import { ref, watch } from 'vue';
import { WikiRecommendModel } from '@/models/wikiParsing/wikiRecommend';
import { injectApi } from '@/provides';
import Loading from '@/components/Loading.vue';
import { useFilesRoutesJump } from '../Files/routes/routesJump';
import { useWikiParsingRoutesJump } from './routes/routesJump';

const props = defineProps<{
    pathName:string
}>()
const emit = defineEmits<{
    (e: 'hasContent', value: boolean): void
}>()
const { jumpToDirFromId } = useFilesRoutesJump();
const { jumpToViewWiki } = useWikiParsingRoutesJump();
const api = injectApi();
const model = ref<WikiRecommendModel>()
api.wikiParsing.wikiParsing.getRecommend(props.pathName).then(x=>model.value=x)
const dirsWithWiki = ref<WikiRecommendModel['Dirs']>([])
const dirsWithoutWiki = ref<WikiRecommendModel['Dirs']>([])
watch(model, (m) => {
    if (m) {
        dirsWithWiki.value = m.Dirs.filter(d => d.Wikis.length > 0)
        dirsWithoutWiki.value = m.Dirs.filter(d => d.Wikis.length === 0)
        emit('hasContent', m.Dirs.length > 0)
    }
})
</script>

<template>
    <template v-if="model">
        <div v-if="dirsWithWiki.length > 0" class="recs">
            <div v-for="d in dirsWithWiki" :key="d.Id" class="dirBox">
                <div class="dirTitle" @click="jumpToDirFromId(d.Id)">
                    <div class="dirTitleText">
                        <div class="dirName">{{ d.Name }}</div>
                        <div class="dirCount">另有{{ d.TotalWikiCount }}个词条</div>
                    </div>
                    <svg class="dirArrow" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                        <polyline points="9 18 15 12 9 6"></polyline>
                    </svg>
                </div>
                <div class="wikiList">
                    <div v-for="w in d.Wikis" :key="w.UrlPathName" @click="jumpToViewWiki(w.UrlPathName)">
                        {{ w.Title }}
                    </div>
                </div>
            </div>
        </div>
        <div v-if="dirsWithoutWiki.length > 0" class="recs recsEmpty">
            <div v-for="d in dirsWithoutWiki" :key="d.Id" class="dirBox dirBoxEmpty" @click="jumpToDirFromId(d.Id)">
                <div class="dirTitleText">
                    <div class="dirName">{{ d.Name }}</div>
                    <div class="dirCount">暂无其他词条</div>
                </div>
                <svg class="dirArrow" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                    <polyline points="9 18 15 12 9 6"></polyline>
                </svg>
            </div>
        </div>
    </template>
    <Loading v-else></Loading>
</template>

<style scoped lang="scss">
.recs{
    margin: 20px 0px;
    display: flex;
    flex-wrap: wrap;
    gap: 15px;
    justify-content: center;
}
.dirBox{
    background-color: #fff;
    border: 1px solid #e0e0e0;
    border-radius: 8px;
    overflow: hidden;
    min-width: 200px;
    max-width: 400px;
    flex-basis: 10px;
    flex-grow: 1;
    box-shadow: 0 1px 3px rgba(0,0,0,0.06);
}
.dirTitle{
    padding: 10px 12px;
    cursor: pointer;
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 8px;
    border-bottom: 1px solid #f0f0f0;
    &:hover{
        background-color: #f8f8f8;
    }
}
.dirTitleText{
    min-width: 0;
    flex: 1;
}
.dirName{
    font-weight: bold;
    color: cornflowerblue;
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
}
.dirCount{
    font-size: 12px;
    color: #888;
    margin-top: 2px;
}
.dirArrow{
    width: 18px;
    height: 18px;
    flex-shrink: 0;
    color: cornflowerblue;
    opacity: 0.7;
}
.wikiList{
    padding: 8px;
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(140px, 1fr));
    gap: 4px;
}
.wikiList div{
    padding: 6px 8px;
    background-color: #f5f5f5;
    border-radius: 4px;
    cursor: pointer;
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
    color: #333;
    &:hover{
        background-color: #e8e8e8;
    }
}
.dirBoxEmpty{
    padding: 10px 12px;
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 8px;
    cursor: pointer;
    &:hover{
        background-color: #f8f8f8;
    }
}
</style>
