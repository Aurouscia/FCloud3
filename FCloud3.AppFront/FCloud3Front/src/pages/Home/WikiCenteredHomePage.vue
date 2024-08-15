<script setup lang="ts">
import Loading from '@/components/Loading.vue';
import { WikiCenteredHomePage } from '@/models/etc/wikiCenteredHomePage';
import { injectApi } from '@/provides';
import { onMounted, ref } from 'vue';
import { useWikiParsingRoutesJump } from '@/pages/WikiParsing/routes/routesJump';
import { useFilesRoutesJump } from '../Files/routes/routesJump';
import { useFeVersionChecker } from '@/utils/feVersionCheck';

const api = injectApi();
const model = ref<WikiCenteredHomePage>();
const { jumpToViewWikiRoute } = useWikiParsingRoutesJump();
const { jumpToRootDirRoute } = useFilesRoutesJump();
const { checkAndPop } = useFeVersionChecker();
async function init(){
    const resp = await api.etc.wikiCenteredHomePage.get();
    if(resp){
        model.value = resp;
    }
}
onMounted(async()=>{
    await init();
    checkAndPop();
})
</script>

<template>
<div class="wchp" v-if="model">
    <div class="upper">
        <div class="list">
            <div class="listTitle">
                最近更新
            </div>
            <div v-for="w in model.LatestWikis" :key="w.Path" class="listItem">
                <img :src="w.Avt">
                <RouterLink :to="jumpToViewWikiRoute(w.Path)">{{ w.Title }}</RouterLink>
            </div>
        </div>
        <div class="list">
            <div class="listTitle">
                随机看看
            </div>
            <div v-for="w in model.RandomWikis" :key="w.Path" class="listItem">
                <img :src="w.Avt">
                <RouterLink :to="jumpToViewWikiRoute(w.Path)">{{ w.Title }}</RouterLink>
            </div>
        </div>
    </div>    
    <div class="lower">
        <div class="list">
            <div class="twinListRow">
                <div class="listTitle">
                    根文件夹
                </div>
                <div class="listTitle">
                    其最新更新
                </div>
            </div>
            <div v-for="p in model.TopDirs" class="twinListRow">
                <div class="listItem">
                    <RouterLink :to="jumpToRootDirRoute(p.DPath)">{{ p.DName }}</RouterLink>
                </div>
                <div class="listItem">
                    <RouterLink :to="jumpToViewWikiRoute(p.WPath)">{{ p.WTitle }}</RouterLink>
                </div>
            </div>
        </div>
    </div>
</div>
<Loading v-else></Loading>
</template>

<style scoped lang="scss">
.upper{
    display: flex;
    justify-content: space-between;
    .list{
        width: 50%
    }
}
.twinListRow{
    display: flex;
    div{
        width: 50%;
        padding: 7px;
    }
}
.list{
    display: flex;
    flex-direction: column;
    .listItem,.listTitle{
        font-weight: bold;
        border-bottom: 1px solid #ccc;
        transition: 0.5s;
        padding: 7px;
        white-space: nowrap;
        overflow: hidden;
        text-overflow: ellipsis;
        &:hover{
            font-weight: bold;
            background-color: #eee;
        }
        img{
            vertical-align: middle;
            display: inline-block;
            height: 20px;
            width: 20px;
            margin-right: 3px;
            border-radius: 200px;
        }
        a{
            color: #000;
            vertical-align: middle;
        }
    }
    .listTitle{
        color: #aaa;
        font-size: 18px;
        &:hover{
            text-decoration: none;
        }
    }
}
.wchp{
    display: flex;
    flex-direction: column;
    gap: 20px;
}
</style>