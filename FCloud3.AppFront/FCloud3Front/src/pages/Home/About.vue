<script setup lang="ts">

</script>

<template>
<h1>关于软件</h1>
<div class="about">
    <div class="mark">
        <img src="/fcloud.svg"/>
        <div class="name">
            FCloud3
            <div class="cat">内容管理系统</div>
        </div>
    </div>
    <div>
        本项目以<a href="https://www.apache.org/licenses/LICENSE-2.0.html" target="_blank">Apache-2.0</a>开源协议提供<br/>
        任何人有权下载、使用、修改、再分发本项目的源代码和可执行文件，且无需保持开源。
    </div>
    <div>
        <a href="https://gitee.com/au114514/fcloud3" target="_blank">获取源码/反馈问题/提供建议</a>
    </div>
    <div>
        源码贡献者：Au<br/>issue贡献者：滨蜀、旋头(按拼音排序)
    </div>
    <div>
        技术支持(qq群)：798877093
    </div>
</div>
</template>

<style scoped lang="scss">
.mark{
    background-color: #f8f8f8;
    padding: 10px;
    border-radius: 10px;
    .name{
        color:black;
        font-size: 20px;
        margin-top: 4px;
        font-weight: bold;
        .cat{
            font-size: 16px;
            font-weight: normal;
        }
    }
}
img{
    width: 80px;
    height: 80px;
}
.about{
    color: #666;
    display: flex;
    flex-direction: column;
    justify-content: center;
    align-items: center;
    gap:20px;
    &>*{
        text-align: center;
    }
}
a{
    text-decoration: underline;
}
</style>