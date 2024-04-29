<script setup lang="ts">
import { computed, ref } from 'vue';
import { CommentViewResult, Comment, CommentTargetType } from '../../models/messages/comment';
import { injectApi, injectPop, injectUserInfo } from '../../provides';
import { truncate } from 'lodash'; 
import { rateColor, rateText } from './rateTextColor';
import { IdentityInfo } from '../../utils/userInfo';

const props = defineProps<{
    c:CommentViewResult,
    siblings:CommentViewResult[]
    objType:CommentTargetType,
    objId:number
}>()
const emits = defineEmits<{
    (e:'needLoad'):void
}>()
interface ReplyInfo{
    userId:number,
    userName:string,
    cmtId:number,
    cmtBrief:string
}
const replyInfo = computed<ReplyInfo|undefined>(()=>{
    if(props.c.Replying == 0){return undefined;}
    const replying = props.siblings.find(x=>x.Id == props.c.Replying);
    if(!replying){return undefined}
    return{
        userId: replying.UserId,
        userName: replying.UserName,
        cmtId: replying.Id,
        cmtBrief: truncate(replying.Content || "", {length: 15})
    }
})
const needFold = props.c.Replies.some(r=>r.Replying != 0);
const foldOpened = ref(false);
const iden = ref<IdentityInfo>();
injectUserInfo().getIdentityInfo().then(x => iden.value = x)

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
            <div v-if="c.Rate>0" class="rate" :style="{backgroundColor: rateColor(c.Rate)}">{{ c.Rate }}/10 {{ rateText(c.Rate) }}</div>
        </div>
        <div class="replyInfo" v-if="replyInfo">回复: {{ replyInfo.userName }} "{{ replyInfo.cmtBrief }}"</div>
        <div class="content">
            {{ c.Content }}
        </div>
        <div class="ops">
            <div class="btn" v-if="needFold" @click="foldOpened = !foldOpened">查看{{c.Replies.length}}条回复</div>
            <div class="btn" @click="wantWrite=!wantWrite">回复ta</div>
            <div v-if="wantWrite" class="replyWrite">
                <input v-model="writingContent" spellcheck="false" @keydown="inputKeyDown" :placeholder="'回复 '+c.UserName"/>
                <div>
                <button class="ok" @click="send">发送</button>
                <button class="cancel" @click="wantWrite=false">取消</button></div>
            </div>
        </div>
        <div class="replies" v-show="!needFold || foldOpened">
            <CommentItem v-for="r in c.Replies" :c="r" :siblings="c.Replies" :objType="objType" :objId="objId" @needLoad="emits('needLoad')"></CommentItem>
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
    white-space: nowrap;
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
    white-space: nowrap;
}
.ops{
    display: flex;
    flex-wrap: wrap;
    justify-content: flex-start;
    align-items: center;
    position: relative;
    gap: 10px;
    .btn{
        font-size: 14px;
        color: #666;
        cursor: pointer;
        user-select: none;
        &:hover{
            color:#000
        }
    }
}
.replyWrite{
    display: flex;
    align-items: center;
    flex-wrap: wrap;
    justify-content: center;
    input{
        height: 28px;
    }
}
.replies{
    margin-left: 30px;
}
.content{
    margin: 5px;
    font-weight: bold;
}
.replyInfo{
    font-size: 14px;
    color: #666;
    text-decoration: underline;
    margin: 3px 0px 3px 0px;
    white-space: nowrap;
    overflow-x: hidden;
    text-overflow: ellipsis;
}
.commentItem{
    margin: 10px 0px 0px 0px;
    padding: 10px;
    background-color: rgba(0,0,0,0.05)
}
</style>