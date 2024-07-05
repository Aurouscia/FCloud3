<script setup lang="ts">
import SwitchingTabs from '@/components/SwitchingTabs.vue';
import { grammarHelpsStandard, grammarHelpsExtended } from '../../utils/wikiSource/grammarHelp';
</script>

<template>
<div class="wikiView">
    <SwitchingTabs :texts="['标准md支持','特有扩展']">
        <div class="choose">
            <div v-for="i in grammarHelpsStandard" class="item">
                <div class="title">{{ i.title }}</div>
                <div class="desc" v-html="i.desc"></div>
                <div class="code">{{ i.code }}</div>
                <div class="demo" v-html="i.demo"></div>
            </div>
        </div>
        <div class="choose">
            <div v-for="i in grammarHelpsExtended" class="item">
                <div class="title">{{ i.title }}</div>
                <div class="desc" v-html="i.desc"></div>
                <div class="code">{{ i.code }}</div>
                <div class="demo" v-html="i.demo"></div>
            </div>
            <div class="welcome">欢迎提出建议和问题反馈</div>
        </div>
    </SwitchingTabs>
</div>
</template>

<style scoped lang="scss">
@import '@/styles/globalValues';

.welcome{
    margin: 10px 0px 10px 0px;
    font-size: 18px;
    text-align: center;
}
.choose{
    padding-top: 20px;
    height: calc(100vh - $topbar-height - 110px);
    overflow-y: scroll;
}
.title{
    font-size: 18px;
    font-weight: bold;
    margin-bottom: 2px;
    color: cornflowerblue;
}
.desc{
    color:#666
}
.code{
    background-color: #eee;
    padding: 10px;
    border-radius: 5px;
}
.demo{
    padding: 10px;
}
.item{
    border-bottom: 1px solid #ccc;
    margin-bottom: 10px;
    padding-bottom: 10px;
    display: flex;
    flex-direction: column;
    gap: 5px
}
.wikiView{
    margin-top: 20px;    
}
</style>
<style>
.hoverToDisplayDemo{
    color:black !important;
    background-color:black;
}
.hoverToDisplayDemo:hover {
    background-color: transparent;
}
</style>