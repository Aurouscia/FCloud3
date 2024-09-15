<script setup lang="ts">
import Search from '@/components/Search.vue';
import WikiCenteredHomePage from './WikiCenteredHomePage.vue';
import { injectApi } from '@/provides';
import { useWikiParsingRoutesJump } from '../WikiParsing/routes/routesJump';
import Footer from '@/components/Footer.vue'
import SideBar from '@/components/SideBar.vue';
import CreateWiki from '@/components/Wiki/CreateWiki.vue';
import { onMounted, ref } from 'vue';
import { useGuideInfoStore } from '@/utils/globalStores/guideInfo';
import { RouterLink } from 'vue-router';

const api = injectApi();
const { jumpToViewWiki, jumpToViewWikiRoute } = useWikiParsingRoutesJump();
const creatingWiki = ref(false);
const createWikiSidebar = ref<InstanceType<typeof SideBar>>();
function toggleSidebar(){
    if(creatingWiki.value){
        createWikiSidebar.value?.fold();
    }else{
        createWikiSidebar.value?.extend();
    }
}

const { getGuideOf } = useGuideInfoStore();
const introPathName = ref<string|null|undefined>();
onMounted(async()=>{
    introPathName.value = await getGuideOf('intro')
})
</script>

<template>
    <h1>欢迎</h1>
    <div class="welcome">
        这里是我们的架空世界地图库和WIKI<br/>
        请随意浏览或者上传自己的作品 
        <RouterLink v-if="introPathName" :to="jumpToViewWikiRoute(introPathName)">平台介绍</RouterLink><br/>
        绘图器和线网数据在搬迁中，请耐心等待，需使用绘图器请：<a href="http://lagacy.wiki.jowei19.com">返回旧版</a>
    </div>
    <div class="search">
        <Search :source="api.etc.quickSearch.wikiItem" @done="(_v,_i,u)=>jumpToViewWiki(u)" :placeholder="'搜索站内词条'"></Search>
        <button @click="toggleSidebar">创建新词条</button>
    </div>
    <SideBar ref="createWikiSidebar">
        <CreateWiki></CreateWiki>
    </SideBar>
    <WikiCenteredHomePage>
    </WikiCenteredHomePage>
    <Footer></Footer>
</template>

<style scoped>
a{
    color: cornflowerblue;
}
.search{
    overflow: visible;
    display: flex;
    justify-content: center;
    flex-wrap: wrap;
    align-items: center;
    gap: 10px;
}
.welcome{
    text-align: center;
    margin: 10px;
    color: #666;
}
h1{
    text-align: center;
    border: none;
    padding: 0px;
    font-size: 22px;
}
</style>