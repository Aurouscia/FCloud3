<script setup lang="ts">
import { computed, nextTick, onMounted, onUnmounted, ref, watch } from 'vue';
import { injectApi, injectIdentityInfoProvider } from '@/provides';
import { Api } from '@/utils/com/api';
import { WikiParsingResult } from '@/models/wikiParsing/wikiParsingResult';
import { TitleClickFold } from '@/utils/wikiView/titleClickFold';
import { WikiLinkClick } from '@/utils/wikiView/wikiLinkClick';
import { useFootNoteJump } from '@/utils/wikiView/footNoteJump';
import Loading from '@/components/Loading.vue';
import TitleTree from '@/components/Wiki/TitleTree.vue';
import Comment from '@/components/Messages/Comment.vue';
import { CommentTargetType } from '@/models/messages/comment';
import { updateScript } from '@/utils/wikiView/dynamicScriptUpdate';
import menuImg from '@/assets/menu.svg';
import { WikiParaTypes } from '@/models/wiki/wikiParaTypes';
import { useTextSectionRoutesJump } from '../TextSection/routes/routesJump';
import { useWikiRoutesJump } from '../Wiki/routes/routesJump';
import { useDiffRoutesJump } from '../Diff/routes/routesJump';
import { useTableRoutesJump } from '../Table/routes/routesJump';
import { useIdentityRoutesJump } from '@/pages/Identities/routes/routesJump';
import { diffContentTypeFromParaType } from '@/models/diff/DiffContentType';
import { canDisplayAsImage } from '@/utils/fileUtils';
import { useRouter } from 'vue-router';
import { SwipeListener } from '@/utils/eventListeners/swipeListener';
import { sleep } from '@/utils/sleep';
import { recoverTitle, setTitleTo } from '@/utils/titleSetter';
import Recommends from './Recommends.vue';
import { IdentityInfo } from '@/utils/globalStores/identityInfo';
import { UserType } from '@/models/identities/user';
import LongPress from '@/components/LongPress.vue';

const props = defineProps<{
    wikiPathName: string;
}>()
watch(()=>props.wikiPathName,async()=>{
    data.value = undefined;
    commentsLoaded.value = false;
    recommendsLoaded.value = false;
    await init();
})

const data = ref<WikiParsingResult>();
const stylesContent = ref<string>("");
const preScripts = ref<HTMLDivElement>();
const postScripts = ref<HTMLDivElement>();
const styles = computed(()=>`<style>${stylesContent.value}</style>`)
const wikiId = ref<number>(0);
const authorName = ref<string>("")
const authorAvtSrc = ref<string>("")
const sealed = ref(false)
const currentUser = ref<IdentityInfo>();
async function load(){
    data.value = await api.wikiParsing.wikiParsing.getParsedWiki(props.wikiPathName);
    stylesContent.value = data.value?.Styles || "";
    await sleep(10)
    if(preScripts.value){
        updateScript(preScripts.value, data.value?.PreScripts || "");
    }
    await sleep(10)
    if(postScripts.value){
        updateScript(postScripts.value, data.value?.PostScripts || "", "module");
    }
    authorName.value = "";
    authorAvtSrc.value = "";
    sealed.value = false;
    wikiId.value = 0;
    if(data.value){
        const info = await api.wikiParsing.wikiParsing.getWikiDisplayInfo(props.wikiPathName);
        if(info){
            authorName.value = info.UserName;
            authorAvtSrc.value = info.UserAvtSrc;
            sealed.value = info.Sealed;
            wikiId.value = info.WikiId;
        }
    }
}

const titles = ref<InstanceType<typeof TitleTree>>();
const subTitles = ref<HTMLDivElement>();
function titleElementId(id:number):string|undefined{
    if(id>0)
        return 't_'+id;
}
function getIdFromElementId(ele:HTMLElement):number{
    return parseInt(ele.id.substring(2));
}
function moveToTitle(titleId:number){
    const title = document.getElementById(titleElementId(titleId)||"??");
    console.log(title)
    if(title){
        wikiViewArea.value?.scrollTo({top: title.offsetTop, behavior: 'smooth'})
    }
}


let lastScrollTime = 0;
const commentsLoaded = ref(false);
const recommendsLoaded = ref(false);
function viewAreaScrollHandler(){
    const sh = wikiViewArea.value!.scrollHeight;
    const st = wikiViewArea.value!.scrollTop;
    if(sh - st < 2000){
        recommendsLoaded.value = true
    }

    if(Date.now() - lastScrollTime < 50){return;}
    lastScrollTime = Date.now();
    let currentTitleIdx = titlesInContent.findIndex(t=>
        t.offsetTop > st - 20);
    if(currentTitleIdx == -1){
        return
    }
    if(currentTitleIdx == titlesInContent.length-1){
        commentsLoaded.value = true
    }
    let currentTitle = titlesInContent[currentTitleIdx];
    const titleInCatalogOffsetTop = titles.value?.highlight(getIdFromElementId(currentTitle));
    if(titleInCatalogOffsetTop){
        subTitles.value?.scrollTo({top: titleInCatalogOffsetTop - 50, behavior: 'smooth'});
    }
}

function enterEdit(type:WikiParaTypes, underlyingId:number){
    if(type == WikiParaTypes.Text && underlyingId){
        jumpToTextSectionEdit(underlyingId)
    }else if(type == WikiParaTypes.Table && underlyingId){
        jumpToFreeTableEdit(underlyingId)
    }
}

async function toggleSealed(){
    const setTo = !sealed.value;
    const s = await api.wiki.setSealed(wikiId.value, setTo);
    if(s){
        sealed.value = setTo;
    }
}

const api:Api = injectApi();
const iden = injectIdentityInfoProvider();
let clickFold:TitleClickFold;
let wikiLinkClick:WikiLinkClick;
const {listenFootNoteJump,disposeFootNoteJump,footNoteJumpCallBack} = useFootNoteJump();
const wikiViewArea = ref<HTMLDivElement>();
let titlesInContent:HTMLElement[] 
const router = useRouter();
const { jumpToDiffContentHistory } = useDiffRoutesJump();
const { jumpToWikiEdit } = useWikiRoutesJump();
const { jumpToFreeTableEdit } = useTableRoutesJump();
const { jumpToTextSectionEdit } = useTextSectionRoutesJump();
const { jumpToUserCenter } = useIdentityRoutesJump();
onMounted(async()=>{
    await init();
    if(data.value?.Title)
        setTitleTo(data.value?.Title)
})

const subtitlesFolded = ref<boolean>(true);
let swl:SwipeListener|undefined;
function toggleSubtitlesSidebarFolded(force:"fold"|"extend"|"toggle"= "toggle"){
    if(force=="toggle"){
        subtitlesFolded.value = !subtitlesFolded.value;
    }
    else if(force=="fold"){
        subtitlesFolded.value = true;
    }
    else if(force=="extend"){
        subtitlesFolded.value = false;
    }
    if(!subtitlesFolded.value){
        swl = new SwipeListener((n)=>{
            if(n=="right"){
                toggleSubtitlesSidebarFolded('fold');
            }
        },"hor",100)
        swl.startListen()
    }
    else{
        swl?.stopListen()
        swl = undefined;
    }
}

async function init(){
    currentUser.value = await iden.getIdentityInfo();
    if(data.value){
        data.value.Paras = []
    }
    await load();

    listenFootNoteJump();
    footNoteJumpCallBack.value = (top)=>{
        wikiViewArea.value?.scrollTo({top: top, behavior: 'smooth'})
    };

    await nextTick();
    clickFold = new TitleClickFold();
    titlesInContent = clickFold.listen(wikiViewArea.value);
    const commentTitle = document.getElementById("t_666666666");
    if(commentTitle)
        titlesInContent.push(commentTitle)

    viewAreaScrollHandler();
    wikiViewArea.value?.addEventListener('scroll',viewAreaScrollHandler);

    wikiLinkClick = new WikiLinkClick(pathName => {
        router.push(`/w/${pathName}`)
    });
    wikiLinkClick.listen(wikiViewArea.value);
}
onUnmounted(()=>{
    clickFold.dispose();
    disposeFootNoteJump();
    swl?.stopListen();
    recoverTitle();
})
</script>

<template>
<div class="wikiViewFrame">
    <div v-if="data && currentUser" class="wikiView" ref="wikiViewArea">
        <div class="invisible" v-html="styles"></div>
        <div class="invisible" ref="preScripts"></div>
        <div class="masterTitle">
            {{data.Title}}
        </div>
        <div class="info">
            <div class="owner">
                所有者<img :src="authorAvtSrc" class="smallAvatar"/>
                <span @click="jumpToUserCenter(authorName)">{{ authorName }}</span><br/>
                更新于 {{ data.Update }}
            </div>
            <div class="btns">
                <button @click="jumpToWikiEdit(wikiPathName)">编辑词条</button>
                <LongPress v-if="currentUser.Type >= UserType.Admin" :reached="toggleSealed">
                    {{ sealed ? '解除隐藏': '隐藏词条'}}
                </LongPress>
            </div>
        </div>
        <div v-if="sealed" class="sealed">该词条已被隐藏</div>
        <div v-for="p in data.Paras">
            <div v-if="p.ParaType==WikiParaTypes.Text || p.ParaType==WikiParaTypes.Table">
                <h1 :id="titleElementId(p.TitleId)">
                    <span v-html="p.Title"></span>
                    <div class="h1Sep"></div>
                    <div v-if="p.HistoryViewable" class="editBtn" @click="jumpToDiffContentHistory(diffContentTypeFromParaType(p.ParaType),p.UnderlyingId)">历史</div>
                    <div v-if="p.Editable" class="editBtn" @click="enterEdit(p.ParaType,p.UnderlyingId)">编辑</div>
                </h1>
                <div class="indent" v-html="p.Content">
                </div>
            </div>
            <div v-if="p.ParaType==WikiParaTypes.File && p.Content">
                <div v-if="canDisplayAsImage(p.Content, p.Bytes)" class="imgPara">
                    <a :href="p.Content" target="_blank">
                        <img :src="p.Content"/>
                    </a>
                    <div>{{ p.Title }}</div>
                </div>
                <div v-else class="filePara">
                    <span class="fileHint">点击下载文件：</span>
                    <a :href="p.Content" target="_blank">{{ p.Title }}</a>
                </div>
            </div>
        </div>
        <div class="refbodies" v-if="data.FootNotes.length>0">
            <div v-for="f in data.FootNotes" v-html="f">
            </div>
        </div>
        <div class="invisible" ref="postScripts"></div>

        <Recommends v-if="recommendsLoaded" :path-name="wikiPathName"></Recommends>
        <h1 id="t_666666666">评论区<div class="h1Sep"></div></h1>
        <div class="comments" :class="{commentsNotLoaded: !commentsLoaded}">
            <Comment v-if="commentsLoaded && data" :obj-id="data?.Id" :type="CommentTargetType.Wiki"></Comment>
            <div v-else style="text-align: center;color:gray">(请继续上滑加载评论区)</div>
        </div>
        
    </div>
    <div class="wikiView" v-else>
        <Loading></Loading>
    </div>

    <div class="cover" :class="{folded:subtitlesFolded}" @click="toggleSubtitlesSidebarFolded('fold')">

    </div>
    <div class="subTitles" :class="{folded:subtitlesFolded}" ref="subTitles">
        <TitleTree v-if="data" :title-tree="data?.SubTitles" 
        :isMaster="true" @click-title="moveToTitle" ref="titles"></TitleTree>
        <Loading v-else></Loading>
    </div>
    <div class="subTitlesFoldBtn" @click="()=>toggleSubtitlesSidebarFolded()">
        <img :src="menuImg" alt="目录">
    </div>
</div>
</template>

<style scoped lang="scss">
@import '@/styles/globalValues';

.wikiViewFrame{
    height: $body-height;
    display: flex;
    gap:20px;
}
.subTitles{
    width: 180px;
    height:100%;
    overflow-y: auto;
    overflow-x: hidden;
    flex-shrink: 0;
    position: relative;
    transition: 0.3s;
    background-color: white;
    box-sizing: border-box;
    padding-top: 10px;
    margin-right: 40px;
}
.subTitlesFoldBtn{
    position: fixed;
    bottom: 15px;
    right: 15px;
    width: 25px;
    height: 25px;
    background-color: white;
    border-radius: 5px;
    cursor: pointer;
    text-align: center;
    display: none;
    box-shadow: 0px 0px 3px 0px black;
    img{
        object-fit: contain;
    }
}
.cover{
    display: none;
}
.wikiView{
    max-width: 900px;
    position: relative;
    height:100%;
    flex-grow: 1;
    overflow-y: scroll;
    overflow-x: hidden;

    margin-right: -220px;
    padding-right: 220px;
}

.info{
    display: flex;
    justify-content: space-between;
    align-items: center;
    .owner{
        font-size: 16px;
        color: #666;
        img{
            margin: 0px 5px 10px 5px
        }
        span{
            cursor: pointer;
            &:hover{
                text-decoration: underline;
            }
        }
        white-space: nowrap;
        overflow: hidden;
        text-overflow: ellipsis;
    }
}
.btns{
    display: flex;
    flex-direction: column;
    gap: 2px;
}
.sealed{
    color:red;
    font-weight: bold;
    margin-top: 5px;
    text-align: center;
}

@media screen and (max-width: 700px){
    .wikiView{
        margin-right: 0px;
        padding-right: 0px;
    }
    .subTitles{
        position: fixed;
        right: 0px;
        top: 0px;
        padding-top: calc($topbar-height + 10px);
        box-shadow: 0px 0px 12px 0px black;
        margin-right: 0px;
    }
    .subTitles.folded{
        right: -180px;
        box-shadow: none;
    }
    .subTitlesFoldBtn{
        display: block;
    }

    .cover{
        display: block;
        position: fixed;
        left: 0px;
        right: 0px;
        bottom: 0px;
        top: 0px;
    }
    .cover.folded{
        display: none;
    }
}

.comments{
    margin-top: 30px;
    margin-bottom: 40px;
}
.commentsNotLoaded{
    margin-bottom: 100vh;
}
</style>