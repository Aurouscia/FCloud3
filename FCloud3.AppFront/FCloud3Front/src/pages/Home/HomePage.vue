<script setup lang="ts">
import Search from '@/components/Search.vue';
import WikiCenteredHomePage from './WikiCenteredHomePage.vue';
import { injectApi } from '@/provides';
import { useWikiParsingRoutesJump } from '../WikiParsing/routes/routesJump';
import Footer from '@/components/Footer.vue'
import SideBar from '@/components/SideBar.vue';
import CreateWiki from '@/components/Wiki/CreateWiki.vue';
import { onMounted, ref, useTemplateRef } from 'vue';
import { guideInfo } from '@/utils/guideInfo';
import { RouterLink } from 'vue-router';

const api = injectApi();
const { jumpToViewWiki, jumpToViewWikiRoute } = useWikiParsingRoutesJump();
const creatingWiki = ref(false);
const createWikiSidebar = useTemplateRef('createWikiSidebar')
function toggleSidebar(){
    if(creatingWiki.value){
        createWikiSidebar.value?.fold();
    }else{
        createWikiSidebar.value?.extend();
    }
}

const introPathName = ref<string|null|undefined>();
const mainRepoOnly = ref<boolean>(false)
onMounted(async()=>{
    introPathName.value = guideInfo.siteIntro
    mainRepoOnly.value = guideInfo.mainRepoOnly
})
</script>

<template>
    <h1>欢迎</h1>
    <div class="welcome">
        {{ guideInfo.welcome }}
        <RouterLink v-if="introPathName" :to="jumpToViewWikiRoute(introPathName)">平台介绍</RouterLink>
        <template v-if="introPathName && mainRepoOnly">
            -
        </template>
        <template v-if="mainRepoOnly">
            <a href="http://wiki.jowei19.com/#/w/mermaid-latex-code" style="color:green">最新高级功能</a>
            - 
            <a href="/#/Fork" style="background: linear-gradient(90deg, cornflowerblue, #4ecdc4);background-clip: text;color: transparent;font-weight: bold;">搭建自有私服</a>
            <p style="font-size: 12px;">标题默认隐藏的目录行为变更（请参考“语法指南”）</p>
            <p style="font-size: 12px;">嵌入网络资源的新参数“根据屏幕宽度决定是否显示”（请参考“语法指南”）</p>
            <p style="font-size: 12px;">新插件“表格浮动”已上线（请参考“插件列表”）</p>
        </template>
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