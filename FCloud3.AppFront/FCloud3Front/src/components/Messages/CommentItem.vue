<script setup lang="ts">
import { ref } from 'vue';
import { CommentViewResult, Comment, CommentTargetType } from '../../models/messages/comment';
import { injectApi, injectPop } from '../../provides';

const props = defineProps<{
    c:CommentViewResult,
    objType:CommentTargetType,
    objId:number
}>()
const emits = defineEmits<{
    (e:'needLoad'):void
}>()
const wantWrite = ref<boolean>(false);
const writingContent = ref<string>();
const api = injectApi();
const pop = injectPop();

async function send(){
    if(!writingContent.value){
        pop.value.show("回复不能为空","failed")
    }
    const cmt:Comment={
        Id:0,
        Content:writingContent.value||'',
        TargetType:props.objType,
        TargetObjId:props.objId,
        ReplyingTo:props.c.Id,
        Rate:0
    }
    const resp = await api.messages.comment.create(cmt)
    if(resp){
        wantWrite.value = false;
        writingContent.value = undefined;
        emits('needLoad')
    }
}

async function inputKeyDown(e:KeyboardEvent){
    if(e.key=="Enter"){
        await send();
    }
}
</script>

<template>
    <div class="commentItem">
        <div class="info">
            <img :src="c.UserAvtSrc" class="smallAvatar"/>
            <div class="uname">{{ c.UserName }}</div>
            <div class="time">{{ c.Time }}</div>
            <div v-if="c.Rate>0" class="rate">{{ c.Rate }}/10</div>
        </div>
        <div class="content">
            {{ c.Content }}
        </div>
        <div class="ops">
            <div class="btn" @click="wantWrite=!wantWrite">回复ta</div>
            <div v-if="wantWrite" class="replyWrite">
                <input v-model="writingContent" spellcheck="false" @keydown="inputKeyDown"/>
                <button class="ok" @click="send">发送</button>
                <button class="cancel" @click="wantWrite=false">取消</button>
            </div>
        </div>
        <div class="replies">
            <CommentItem v-for="r in c.Replies" :c="r" :objType="objType" :objId="objId" @needLoad="emits('needLoad')"></CommentItem>
        </div>
    </div>
</template>

<style scoped lang="scss">
.info{
    display: flex;
    justify-content: flex-start;
    align-items: baseline;
    font-size: 18px;
    gap: 5px;
}
.uname{
    color: #666;
}
.time{
    color: #aaa;
    font-size: 14px;
}
.rate{
    background-color: black;
    color:white;
    padding: 3px;
    text-align: center;
    border-radius: 5px;
}
.ops{
    display: flex;
    justify-content: flex-start;
    align-items: center;
    position: relative;
    height: 24px;
    .btn{
        font-size: 14px;
        color: #aaa;
        text-decoration: underline;
        cursor: pointer;
        user-select: none;
        &:hover{
            color:#666
        }
    }
}
.replyWrite{
    display: flex;
    align-items: center;
    input{
        height: 28px;
    }
}
.replies{
    margin-left: 30px;
}
.content{
    margin: 5px;
}
.commentItem{
    margin: 10px 0px 0px 0px;
    padding: 10px;
    background-color: rgba(0,0,0,0.05)
}
</style>