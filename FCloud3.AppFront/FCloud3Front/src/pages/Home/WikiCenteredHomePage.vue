<script setup lang="ts">
import Loading from '@/components/Loading.vue';
import { WikiCenteredHomePage } from '@/models/etc/wikiCenteredHomePage';
import { injectApi } from '@/provides';
import { onMounted, ref } from 'vue';
import { useWikiParsingRoutesJump } from '@/pages/WikiParsing/routes/routesJump';
import { useFilesRoutesJump } from '../Files/routes/routesJump';
import { useFeVersionChecker } from '@/utils/feVersionCheck/feVersionCheck';
import externalLinkIcon from '@/assets/externalLink.svg'
import WikiSelectedSwiperContainer from '../../components/Swiper/WikiSelectedSwiperContainer.vue';

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
function isExternal(path:string){
    return path.startsWith('http://') || path.startsWith('https://')
}
onMounted(async()=>{
    checkAndPop();
    await init();
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
                <img :src="w.Avt" class="avt">
                <a v-if="isExternal(w.Path)" :href="w.Path" target="_blank">
                    {{ w.Title }}
                    <img class="externalLinkIcon" :src="externalLinkIcon"/>
                </a>
                <RouterLink v-else :to="jumpToViewWikiRoute(w.Path)">{{ w.Title }}</RouterLink>
                <span class="tInfo" v-if="w.TimeInfo">{{ w.TimeInfo }}</span>
            </div>
        </div>
        <div class="list">
            <div class="listTitle">
                随机看看
            </div>
            <div v-for="w in model.RandomWikis" :key="w.Path" class="listItem">
                <img :src="w.Avt" class="avt">
                <RouterLink :to="jumpToViewWikiRoute(w.Path)">{{ w.Title }}</RouterLink>
            </div>
        </div>
    </div>
    <WikiSelectedSwiperContainer></WikiSelectedSwiperContainer>
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
        img.avt{
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
        .tInfo{
            color: gray;
            margin-left: 6px;
            font-size: 13px;
            font-weight: normal;
            vertical-align: bottom;
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