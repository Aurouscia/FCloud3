<script setup lang="ts">
import { computed, nextTick, onMounted, onUnmounted, ref, watch } from 'vue';
import { injectApi } from '../../provides';
import { Api } from '../../utils/api';
import { WikiParsingResult } from '../../models/wikiParsing/wikiParsingResult';
import { TitleClickFold } from '../../utils/wikiView/titleClickFold';
import { WikiLinkClick } from '../../utils/wikiView/wikiLinkClick';
import { useFootNoteJump } from '../../utils/wikiView/footNoteJump';
import Loading from '../../components/Loading.vue';
import TitleTree from '../../components/Wiki/TitleTree.vue';
import Comment from '../../components/Messages/Comment.vue';
import { CommentTargetType } from '../../models/messages/comment';
import { updateScript } from '../../utils/wikiView/dynamicScriptUpdate';
import menuImg from '../../assets/menu.svg';
import { WikiParaTypes } from '../../models/wiki/wikiParaTypes';
import { jumpToTextSectionEdit } from '../TextSection/routes';
import { jumpToFreeTableEdit } from '../Table/routes';
import { jumpToWikiEdit } from '../Wiki/routes';
import { jumpToDiffContentHistory } from '../Diff/routes';
import { jumpToUserCenter } from '../Identities/routes';
import { diffContentTypeFromParaType } from '../../models/diff/DiffContentType';
import { canDisplayAsImage } from '../../utils/fileUtils';
import { useRouter } from 'vue-router';
import { SwipeListener } from '../../utils/swipeListener';
import { sleep } from '../../utils/sleep';
import { recoverTitle, setTitleTo } from '../../utils/titleSetter';
import Recommends from './Recommends.vue';

const props = defineProps<{
    wikiPathName: string;
}>()
watch(()=>props.wikiPathName,async()=>{
    data.value = undefined;
    await init();
})

const data = ref<WikiParsingResult>();
const stylesContent = ref<string>("");
const preScripts = ref<HTMLDivElement>();
const postScripts = ref<HTMLDivElement>();
const styles = computed(()=>`<style>${stylesContent.value}</style>`)
const userName = ref<string>("")
const userAvtSrc = ref<string>("")
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
    userName.value = "";
    userAvtSrc.value = "";
    if(data.value){
        const user = await api.identites.user.getInfo(data.value?.OwnerId)
        if(user){
            userName.value = user.Name;
            userAvtSrc.value = user.AvatarSrc;
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
function viewAreaScrollHandler(){
    if(Date.now() - lastScrollTime < 50){return;}
    lastScrollTime = Date.now();
    let currentTitleIdx = titlesInContent.findIndex(t=>
        t.offsetTop > wikiViewArea.value!.scrollTop - 20);
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

let api:Api;
let clickFold:TitleClickFold;
let wikiLinkClick:WikiLinkClick;
const {listenFootNoteJump,disposeFootNoteJump,footNoteJumpCallBack} = useFootNoteJump();
const wikiViewArea = ref<HTMLDivElement>();
let titlesInContent:HTMLElement[] 
const router = useRouter();
onMounted(async()=>{
    api = injectApi();
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
    <div v-if="data" class="wikiView" ref="wikiViewArea">
        <div class="invisible" v-html="styles"></div>
        <div class="invisible" ref="preScripts"></div>
        <div class="masterTitle">
            {{data.Title}}
        </div>
        <div class="info">
            <div class="owner">
                所有者<img :src="userAvtSrc" class="smallAvatar"/>
                <span @click="jumpToUserCenter(userName)">{{ userName }}</span><br/>
                更新于 {{ data.Update }}
            </div>
            <button @click="jumpToWikiEdit(wikiPathName)">编辑本词条</button>
        </div>
        <div v-for="p in data.Paras">
            <div v-if="p.ParaType==WikiParaTypes.Text || p.ParaType==WikiParaTypes.Table">
                <h1 :id="titleElementId(p.TitleId)">
                    {{ p.Title }}
                    <div class="h1Sep"></div>
                    <div class="editBtn" @click="jumpToDiffContentHistory(diffContentTypeFromParaType(p.ParaType),p.UnderlyingId)">历史</div>
                    <div class="editBtn" @click="enterEdit(p.ParaType,p.UnderlyingId)">编辑</div>
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

        <Recommends :path-name="wikiPathName"></Recommends>
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
.wikiViewFrame{
    height:calc(96vh - var(--main-div-margin-top));
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
    }
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
        padding-top: var(--main-div-margin-top);
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