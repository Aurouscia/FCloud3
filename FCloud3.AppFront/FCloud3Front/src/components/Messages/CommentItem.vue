<script setup lang="ts">
import { computed, ref } from 'vue';
import { CommentViewResult, Comment, CommentTargetType } from '@/models/messages/comment';
import { injectApi, injectPop } from '@/provides';
import { truncate } from 'lodash'; 
import { rateColor, rateText } from './rateTextColor';
import { useIdentityRoutesJump } from '@/pages/Identities/routes/routesJump';
import { useIdentityInfoStore } from '@/utils/globalStores/identityInfo';
import { storeToRefs } from 'pinia';
import { UserType } from '@/models/identities/user';

const props = defineProps<{
    c:CommentViewResult,
    siblings:CommentViewResult[]
    objType:CommentTargetType,
    objId:number
}>()
const { jumpToUserCenter } = useIdentityRoutesJump();
const { iden } = storeToRefs(useIdentityInfoStore())

const canDelete = computed<boolean>(()=>{
    if(iden.value.Type >= UserType.Admin){
        return true;
    }
    if(props.c.UserId == iden.value.Id){
        return true;
    }
    return false
})
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

const wantWrite = ref<boolean>(false);
const writingContent = ref<string>();
const api = injectApi();
const pop = injectPop();

async function send(){
    if(!writingContent.value){
        pop.value.show("回复不能为空","failed")
        return;
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

async function hide() {
    const s = await api.messages.comment.hide(props.c.Id);
    if(s){
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
            <div class="sender">
                <img :src="c.UserAvtSrc" class="smallAvatar"/>
                <div class="uname" @click="jumpToUserCenter(c.UserName)">{{ c.UserName }}</div>
                <div class="time">{{ c.Time }}</div>
            </div>
            <div v-if="!c.Hidden && c.Rate>0 && c.Rate<11" class="rate" :style="{backgroundColor: rateColor(c.Rate)}">{{ c.Rate }}/10 {{ rateText(c.Rate) }}</div>
        </div>
        <div class="replyInfo" v-if="replyInfo">回复: {{ replyInfo.userName }} "{{ replyInfo.cmtBrief }}"</div>
        <div class="content" :class="{hidden:c.Hidden}">
            {{ c.Content }}
        </div>
        <div class="ops">
            <div class="btn" v-if="needFold" @click="foldOpened = !foldOpened">查看{{c.Replies.length}}条回复</div>
            <div class="btn" @click="wantWrite=!wantWrite">回复ta</div>
            <div class="btn" v-if="canDelete && !c.Hidden" @click="hide">删除</div>
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
    align-items: center;
    font-size: 18px;
    gap: 10px;
    .sender{
        display: flex;
        justify-content: flex-start;
        gap:5px;
        flex-wrap: wrap;
        align-items:end;
        div{
            margin-bottom: 3px;
        }
    }
}
.uname{
    color: #666;
    white-space: nowrap;
    cursor: pointer;
    &:hover{
        text-decoration: underline;
    }
}
.time{
    color: #aaa;
    font-size: 14px;
    margin-top: -4px;
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
        transition: 0.3s;
        font-size: 14px;
        color: #888;
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
.content.hidden{
    color: #888;
    font-style: italic;
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