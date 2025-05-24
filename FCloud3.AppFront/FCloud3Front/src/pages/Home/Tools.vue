<script setup lang="ts">
import { recoverTitle, setTitleTo } from '@/utils/titleSetter';
import { onMounted, onUnmounted } from 'vue';

onMounted(()=>{
    setTitleTo('实用工具')
})
onUnmounted(()=>{
    recoverTitle()
})
</script>

<template>
<h1>实用工具</h1>
<div class="tools">
    <div class="toolItem">
        <h2>线路图画布</h2>
        <div class="author">作者 Au</div>
        <div class="intro">支持PC/手机/平板的线路图绘制软件，门槛低易上手，开源免费，轻松创作自己的线路图，<i>目前在持续开发中</i></div>
        <div class="links">
            <a href="http://aarc.jowei19.com" target="_blank">
                <button class="ok">进入使用</button></a>
            <a href="https://gitee.com/au114514/aarc" target="_blank">项目源码</a>
        </div>
        <div class="note">账户与本站不互通，单独注册</div>
        <img src="http://static.wiki.jowei19.com/wikiFile/4kkbjxh5.png"/>
    </div>
    <div class="toolItem">
        <h2>轨交棋（网页版）</h2>
        <div class="author">作者 Au</div>
        <div class="intro">
            一款在线游戏，玩家在地铁图上移动并互相对抗，占领更多车站得分，支持2-8人联机对战，
            <i>是mmc(SlinkierApple13)及其同学创造的游戏玩法的网页实现</i></div>
        <div class="links">
            <a href="http://railchess.jowei19.com" target="_blank">
                <button class="ok">进入游玩</button></a>
            <a href="https://gitee.com/au114514/mmc-rail-chess" target="_blank">项目源码</a>
        </div>
        <div class="note">账户与本站不互通，单独注册</div>
    </div>
    <div class="toolItem">
        <h2>地形图生成器</h2>
        <div class="author">作者 Au</div>
        <div class="intro">使用随机算法生成富有轮廓细节的地形图，可用于获取灵感或直接作为创作底图</div>
        <div class="links">
            <a href="http://mapgen.jowei19.com" target="_blank">
                <button class="ok">进入使用</button></a>
            <a href="https://gitee.com/au114514/mapgen" target="_blank">项目源码</a>
        </div>
    </div>
    <div class="toolItem">
        <h2>图片转换器</h2>
        <div class="author">作者 ImageResizer团队</div>
        <div class="intro">在线对各种图片进行格式转换、裁剪、尺寸调整、压缩等操作</div>
        <div class="links">
            <a href="https://imageresizer.com/" target="_blank">
                <button class="ok">进入使用</button>
            </a>
        </div>
    </div>
</div>
</template>

<style scoped lang="scss">
.tools{
    display: flex;
    flex-direction: column;
    align-items: stretch
}
.toolItem{
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 10px;
    border-bottom: 1px solid gray;
    padding-bottom: 20px;
    margin-bottom: 20px;
    h2{
        font-size: 20px;
    }
    .author{
        font-size: 14px;
        color: gray
    }
    .intro{
        color: #333;
        text-indent: 2em;
        i{
            color: gray;
        }
    }
    img{
        object-fit: contain;
        width: 90%;
        max-height: 300px;
        border-radius: 10px;
    }
    .links{
        display: flex;
        justify-content: center;
        align-items: center;
        gap: 30px;
    }
    a{
        color: cornflowerblue;
        font-weight: bold;
    }
    .note{
        font-size: 14px;
        color: plum
    }
}
</style>