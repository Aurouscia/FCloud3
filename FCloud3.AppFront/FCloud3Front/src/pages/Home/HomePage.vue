<script setup lang="ts">
import Search from '@/components/Search.vue';
import WikiCenteredHomePage from './WikiCenteredHomePage.vue';
import { injectApi } from '@/provides';
import { useWikiParsingRoutesJump } from '../WikiParsing/routes/routesJump';
import Footer from '@/components/Footer.vue'

const api = injectApi();
const { jumpToViewWiki } = useWikiParsingRoutesJump();
</script>

<template>
    <h1>欢迎</h1>
    <div class="welcome">
        这里是我们的架空世界地图库和WIKI<br/>
        请随意浏览或者上传自己的作品<br/>
        绘图器和线网数据在搬迁中，请耐心等待，需使用绘图器请：<a class="lagacy" href="http://lagacy.wiki.jowei19.com">返回旧版</a>
    </div>
    <div class="search">
        <Search :source="api.utils.quickSearch.wikiItem" @done="(_v,_i,u)=>jumpToViewWiki(u)" :placeholder="'搜索站内词条'"></Search>
    </div>
    <WikiCenteredHomePage>
    </WikiCenteredHomePage>
    <Footer></Footer>
</template>

<style scoped>
.lagacy{
    color: cornflowerblue;
}
.search{
    overflow: visible !important;
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