<script setup lang="ts">
import { onMounted, onUnmounted, ref } from 'vue';
import { NotifViewItem, NotifType } from '@/models/messages/notification';
import { injectApi, injectNotifCountProvider } from '@/provides';
import Loading from '@/components/Loading.vue';
import { recoverTitle, setTitleTo } from '@/utils/titleSetter';
import { useNotifCountStore } from '@/utils/globalStores/notifCount';
import { useIdentityInfoStore } from '@/utils/globalStores/identityInfo';
import { useWikiParsingRoutesJump } from '../WikiParsing/routes/routesJump';
import { useIdentityRoutesJump } from '../Identities/routes/routesJump';

const api = injectApi();
const notifs = ref<NotifViewItem[]>([])
const loaded = ref(false);
const totalCount = ref(0);
const notifCountStore = useNotifCountStore();
const notifCountProvider = injectNotifCountProvider();
const {jumpToViewWiki} = useWikiParsingRoutesJump();
const {jumpToUserGroup} = useIdentityRoutesJump();
const idenStore = useIdentityInfoStore();

async function load(){
    if(idenStore.iden.Id <= 0)
        return;
    const res = await api.messages.notification.get(notifs.value.length);
    if(res){
        totalCount.value = res.TotalCount;
        notifs.value.push(...res.Items);
        const unreadCount = res.Items.filter(x=>!x.Read).length;
        notifCountProvider.activeOverride(unreadCount);
    }
}
async function markRead(id:number|"all") {
    const res = await api.messages.notification.markRead(id);
    if(res){
        if(id == 'all')
        {
            notifs.value.forEach(n=>n.Read=true);
            notifCountStore.readAll()
        }
        else{
            const target = notifs.value.find(x=>x.Id==id);
            if(target){
                target.Read = true;
                notifCountStore.readOne();
            }
        }
    }
}
async function jumpToWiki(id:number){
    const info = await api.wiki.wikiItem.getInfoById(id);
    if(info){
        jumpToViewWiki(info.UrlPathName);
    }
}

onMounted(async()=>{
    setTitleTo('通知消息')
    await load();
    loaded.value = true;
})
onUnmounted(()=>{
    recoverTitle()
})
</script>

<template>
    <h1>
        通知消息
        <button v-if="notifs.some(x=>!x.Read)" @click="markRead('all')">全部已读</button>
    </h1>
    <div v-if="loaded" class="notifs">
        <div v-for="n in notifs" class="n">
            <div v-if="n.Type == NotifType.CommentWiki">
                <span class="s">{{ n.SName }}</span>
                评论了你的词条
                <span class="wikiTitle" @click="jumpToWiki(n.P1)">{{ n.P1T }}</span>
                <span class="cmt">"{{ n.P2T }}"</span>
            </div>
            <div v-else-if="n.Type == NotifType.CommentWikiReply">
                <span class="s">{{ n.SName }}</span>
                回复了你在
                <span class="wikiTitle" @click="jumpToWiki(n.P1)">{{ n.P1T }}</span>
                下的评论
                <span class="cmt">"{{ n.P2T }}"</span>
            </div>
            <div v-else-if="n.Type == NotifType.UserGroupInvite">
                <span class="s">{{ n.SName }}</span>
                邀请你加入用户组
                <span class="wikiTitle" @click="jumpToUserGroup(n.P1)">{{ n.P1T }}</span>
            </div>
            <div class="right">
                {{ n.Time }}
                <div v-if="!n.Read" class="unread" @click="markRead(n.Id)">未读</div>
                <div v-else class="read">已读</div>
            </div>
        </div>
    </div>
    <Loading v-else></Loading>
    <div v-if="loaded && notifs.length == 0" class="empty">暂无通知</div>
    <div v-if="totalCount>notifs.length" class="more" @click="load">加载更多</div>
</template>

<style scoped lang="scss">
.cmt{
    color: black;
    font-style: italic;
}
.wikiTitle{
    color: olivedrab;
    font-weight: bold;
    cursor: pointer;
    &:hover{
        text-decoration: underline;
    }
}
.s{
    color: black;
    font-weight: bold;
}
.n{
    color: #888;
    background-color: #eee;
    margin-bottom: 10px;
    padding: 10px;
    &:hover{
        background-color: #e3e3e3;
    }
    display: flex;
    justify-content: space-between;
    align-items: center;
    gap: 10px
}
@media screen and (max-width: 600px) {
    .n{
        flex-direction: column;
        align-items: flex-start;
    }
}
.empty{
    text-align: center;
    color: #888;
}
.more{
    text-align: center;
    color:cornflowerblue;
    text-decoration: underline;
    cursor: pointer;
    &:hover{
        font-weight: bold;
    }
}
.right{
    display: flex;
    gap:10px;
    white-space: nowrap;
    .unread{
        color: plum;
        &:hover{
            text-decoration: underline;
            cursor: pointer;
        }
    }
    .read{
        color:#aaa
    }
}
h1{
    display: flex;
    height: 34px;
    justify-content: space-between;
    button{
        height: 30px;
        font-size: 16px;
    }
}
</style>