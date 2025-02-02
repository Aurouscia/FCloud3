<script setup lang="ts">
import Search from '@/components/Search.vue';
import WikiCenteredHomePage from './WikiCenteredHomePage.vue';
import { injectApi } from '@/provides';
import { useWikiParsingRoutesJump } from '../WikiParsing/routes/routesJump';
import Footer from '@/components/Footer.vue'
import SideBar from '@/components/SideBar.vue';
import CreateWiki from '@/components/Wiki/CreateWiki.vue';
import { onMounted, ref } from 'vue';
import { guideInfo } from '@/utils/guideInfo';
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

const introPathName = ref<string|null|undefined>();
onMounted(async()=>{
    introPathName.value = guideInfo.siteIntro

    const resp = await api.etc.wikiTopBriefsOfDir.get(19615, 3)
    console.log(resp)
})
</script>

<template>
    <h1>欢迎</h1>
    <div class="welcome">
        {{ guideInfo.welcome }}
        <RouterLink v-if="introPathName" :to="jumpToViewWikiRoute(introPathName)">平台介绍</RouterLink>
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
    white-space: pre-wrap;
}
h1{
    text-align: center;
    border: none;
    padding: 0px;
    font-size: 22px;
}
</style>