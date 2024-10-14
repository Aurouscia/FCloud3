<script setup lang="ts">
import settingsImg from '@/assets/settings.svg'
import newDirImg from '@/assets/newDir.svg'
import { onMounted, onUnmounted } from 'vue';
import { setTitleTo, recoverTitle } from '@/utils/titleSetter';

onMounted(()=>{setTitleTo('平台使用帮助')})
onUnmounted(()=>{recoverTitle()})
</script>

<template>
    <h1>平台使用帮助</h1>
    <div class="help">
        <div>
            1.文件和词条都存在于目录里(顶部栏-作品-所有作品)，要<b>发布文件/编写词条</b>，请点击目录界面的
            <img :src="settingsImg"/>
            按钮即可新建词条/上传文件，注意你必须是目录的所有者或与所有者在同一用户组内
        </div>
        <div>
            2.要<b>创建自己的目录</b>，请点击根目录顶部的
            <img :src="newDirImg"/>
            按钮
        </div>
        <div>3.要<b>合作编写词条</b>，请先加入用户组(顶部栏-身份-用户组)，默认情况下同一组内成员可互相编辑作品和目录</div>
        <div>4.要<b>加入用户组</b>，请先通过qq等方式联系用户组的组长，组长可以邀请用户进入，待其邀请后，在用户组列表点击同意即可</div>
        <div>5.暂未开放私聊功能，如果需要在站内<b>联系其他用户</b>，可以在其词条评论区下留言，该用户上线时能收到提醒</div>
        <div>6.遇到问题请随时咨询管理员</div>
    </div>
</template>

<style scoped>
.help div{
    text-indent: 15px;
    margin-bottom: 1em;
    color: #666;
}
.help div b{
    color: black;
}
img{
    width: 1.2em;
    height: 1.2em;
    background-color: #666;
    vertical-align:middle;
    border: 3px solid #666;
    border-radius: 5px;
}
</style>