<script setup lang="ts">
import settingsImg from '@/assets/settings.svg'
</script>

<template>
    <div class="help">
        <div>
            1.文件和词条都存在于目录里(顶部栏-作品-目录)，要发布文件/编写词条，请先创建目录或联系目录的所有者获取授权，然后点击文件夹界面的
            <img :src="settingsImg"/>
            按钮即可新建词条/上传文件
        </div>
        <div>2.要合作编写词条，请先加入用户组，并进入编辑词条-权限设置，设置词条允许该用户组的成员编辑。在该页面也可以为用户单独添加授权</div>
        <div>3.要加入用户组，请先通过qq等方式联系用户组的负责人，负责人可以邀请用户进入，待其邀请后，在用户组列表点击同意即可</div>
        <div>4.暂未开放私聊功能，如果需要在站内联系其他用户，可以在其词条评论区下留言，该用户上线时能收到提醒</div>
    </div>
</template>

<style scoped>
.help div{
    text-indent: 15px;
    margin-bottom: 1em;
}
img{
    width: 1.2em;
    height: 1.2em;
    background-color: #666;
    vertical-align:middle;
}
</style>