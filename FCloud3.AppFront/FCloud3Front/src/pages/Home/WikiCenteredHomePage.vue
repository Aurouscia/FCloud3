<script setup lang="ts">
import Loading from '@/components/Loading.vue';
import { WikiCenteredHomePage } from '@/models/etc/wikiCenteredHomePage';
import { injectApi, injectPop } from '@/provides';
import { onMounted, ref } from 'vue';
import { useWikiParsingRoutesJump } from '@/pages/WikiParsing/routes/routesJump';
import { useFilesRoutesJump } from '../Files/routes/routesJump';
import { useFeVersionChecker } from '@/utils/feVersionCheck/feVersionCheck';
import externalLinkIcon from '@/assets/externalLink.svg'
import WikiSelectedSwiperContainer from '../../components/Swiper/WikiSelectedSwiperContainer.vue';
import { sleep } from '@/utils/sleep';
import { readLocalConfig, saveLocalConfig } from '@/utils/localConfig/localConfig';
import { WikiCenteredHomePageLocalConfig, wikiCenteredHomePageLocalConfigDefault } from '@/utils/localConfig/models/wikiCenteredHomePage';

const api = injectApi();
const model = ref<WikiCenteredHomePage>();
const { jumpToViewWikiRoute } = useWikiParsingRoutesJump();
const { jumpToRootDirRoute } = useFilesRoutesJump();
const { checkAndPop } = useFeVersionChecker();
const pop = injectPop()

async function init(){
    const loadCount = getLoadCount()
    const resp = await api.etc.wikiCenteredHomePage.get(loadCount);
    if(resp){
        model.value = resp;
    }
}

function getLoadCount(): number{
    let loadCount = 10
    const cfg = readLocalConfig('wikiCenteredHomePage')
    if(cfg && 'latestWikiCount' in cfg && typeof cfg.latestWikiCount == 'number'){
        loadCount = cfg.latestWikiCount
    }
    return loadCount
}
function setLoadCount(){
    const res = window.prompt('请设置当前浏览器的“最近更新”条数') ?? ''
    const num = parseInt(res)
    if(!isNaN(num)){
        const cfg = readLocalConfig('wikiCenteredHomePage') ?? wikiCenteredHomePageLocalConfigDefault()
        const newCfg: WikiCenteredHomePageLocalConfig = {...cfg, latestWikiCount: num}
        saveLocalConfig(newCfg)
        handleRefresh()
        pop.value.show('设置成功', 'success')
    }
    else {
        pop.value.show('请输入数字', 'failed')
    }
}

const isRefreshing = ref(false)
async function handleRefresh(){
    if(isRefreshing.value)
        return
    isRefreshing.value = true
    const sleepProm = sleep(800) // 与旋转动画的时长保持一致
    const initProm = init()
    await Promise.all([sleepProm, initProm]) // 至少拖住sleep的时长，避免用户点击过频繁
    isRefreshing.value = false
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
                <div class="menuIcon" @click="setLoadCount"></div>
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
                <div class="refreshIcon" :class="{isRefreshing}" @click="handleRefresh"></div>
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
    .listItem, .listTitle{
        height: 37.5px;
        box-sizing: border-box;
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
        display: flex;
        align-items: center;
        gap: 4px;
        user-select: none;
        .menuIcon, .refreshIcon{
            width: 18px;
            height: 18px;
            mask-repeat: no-repeat;
            mask-size: contain;
            mask-position: center center;
            background-color: #aaa;
            cursor: pointer;
        }
        .menuIcon{
            mask-image: url('@/assets/menu.svg');
        }
        .refreshIcon {
            mask-image: url('@/assets/refresh.svg');
            mask-size: 80% 80%;
            will-change: transform;
            &.isRefreshing{
                animation: spin-once 800ms linear forwards; // 时长与sleep时长保持一致
                @keyframes spin-once {
                    to { transform: rotate(360deg); }
                }
            }
        }
        &:hover{
            text-decoration: none;
            background-color: transparent;
        }
    }
}
.wchp{
    display: flex;
    flex-direction: column;
    gap: 20px;
}
</style>