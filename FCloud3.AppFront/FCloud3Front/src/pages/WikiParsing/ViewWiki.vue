<script setup lang="ts">
import { computed, nextTick, onMounted, onUnmounted, ref, watch } from 'vue';
import { injectApi, injectIdentityInfoProvider, injectMainDivStyle, injectWikiViewScrollMemory } from '@/provides';
import { Api, fileDownloadLink } from '@/utils/com/api';
import { WikiParsingResult } from '@/models/wikiParsing/wikiParsingResult';
import { WikiDisplayInfo, wikiDisplayInfoDefault } from '@/models/wikiParsing/wikiDisplayInfo';
import { TitleClickFold } from '@/utils/wikiView/titleClickFold';
import { WikiLinkClick } from '@/utils/wikiView/wikiLinkClick';
import { useFootNoteJump } from '@/utils/wikiView/footNoteJump';
import Loading from '@/components/Loading.vue';
import TitleTree from '@/components/Wiki/TitleTree.vue';
import Comment from '@/components/Messages/Comment.vue';
import { CommentTargetType, cmtTitleId } from '@/models/messages/comment';
import { updateScript } from '@/utils/wikiView/dynamicScriptUpdate';
import menuImg from '@/assets/menu.svg';
import { WikiParaType } from '@/models/wiki/wikiParaType';
import { useTextSectionRoutesJump } from '../TextSection/routes/routesJump';
import { useWikiRoutesJump } from '../Wiki/routes/routesJump';
import { useDiffRoutesJump } from '../Diff/routes/routesJump';
import { useTableRoutesJump } from '../Table/routes/routesJump';
import { useIdentityRoutesJump } from '@/pages/Identities/routes/routesJump';
import { diffContentTypeFromParaType } from '@/models/diff/diffContentTypes';
import { canDisplayAsImage, getFileType } from '@/utils/fileUtils';
import { useRouter } from 'vue-router';
import { SwipeListener } from '@/utils/eventListeners/swipeListener';
import { sleep } from '@/utils/sleep';
import { recoverTitle, setTitleTo } from '@/utils/titleSetter';
import Recommends from './Recommends.vue';
import { IdentityInfo } from '@/utils/globalStores/identityInfo';
import { UserType } from '@/models/identities/user';
import LongPress from '@/components/LongPress.vue';
import Footer from '@/components/Footer.vue';
import { ImageClickJump } from '@/utils/wikiView/imgClickJump';
import ImageFocusView from '@/components/ImageFocusView.vue';
import { userDefaultAvatar } from '@/models/files/material';
import { RouteRenderer } from '@/utils/plugins/routeRenderer'

const props = defineProps<{
    wikiPathName: string;
    viewCmt?: boolean;
}>()
watch(()=>props.wikiPathName,async(_newVal, oldVal)=>{
    wikiViewScrollMemory.save(oldVal, wikiViewArea.value)
    data.value = undefined;
    commentsLoaded.value = false;
    recommendsLoaded.value = false;
    await init(true);
})

const data = ref<WikiParsingResult>();
const stylesContent = ref<string>("");
const preScripts = ref<HTMLDivElement>();
const postScripts = ref<HTMLDivElement>();
const styles = computed(()=>`<style>${stylesContent.value}</style>`)
const displayInfo = ref<WikiDisplayInfo>()
const currentUser = ref<IdentityInfo>();
async function load(){
    setTitleTo("正在跳转")
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
    displayInfo.value = wikiDisplayInfoDefault;
    if(data.value){
        const info = await api.wikiParsing.wikiParsing.getWikiDisplayInfo(props.wikiPathName);
        if(info){
            displayInfo.value = info;
        }
        setTitleTo(data.value.Title)
    }
}

const titles = ref<InstanceType<typeof TitleTree>>();
const subTitles = ref<HTMLDivElement>();
let isActiveMoving = false;
function titleElementId(id:number):string|undefined{
    if(id>0)
        return 't_'+id;
}
function getIdFromElementId(ele:HTMLElement):number{
    return parseInt(ele.id.substring(2));
}
function moveToTitle(titleId:number){
    const title = document.getElementById(titleElementId(titleId)||"??");
    //console.log(title)
    if(title){
        isActiveMoving = true;
        wikiViewArea.value?.scrollTo({top: title.offsetTop, behavior: 'smooth'})
        window.setTimeout(()=>{
            isActiveMoving = false;
        }, 1000)
    }
}


let lastScrollTime = 0;
const commentsLoaded = ref(false);
const recommendsLoaded = ref(false);
function viewAreaScrollHandler(enforce?:boolean){
    if(!enforce)
        if(Date.now() - lastScrollTime < 50){return;}
    const sh = wikiViewArea.value!.scrollHeight;
    const st = wikiViewArea.value!.scrollTop;
    const oh = wikiViewArea.value!.offsetHeight;
    if(sh - st < oh+1600){
        recommendsLoaded.value = true
    }
    if(sh - st < oh+1200){
        commentsLoaded.value = true
    }

    lastScrollTime = Date.now();
    let currentTitleIdx = titlesInContent.findIndex(t=>
        t.offsetTop > st + 80);
    if(currentTitleIdx == -1){
        return
    }
    if(currentTitleIdx != 0){
        currentTitleIdx -= 1;
    }
    let currentTitle = titlesInContent[currentTitleIdx];
    const titleInCatalogOffsetTop = titles.value?.highlight(getIdFromElementId(currentTitle));
    if(titleInCatalogOffsetTop && !isActiveMoving){
        subTitles.value?.scrollTo({top: titleInCatalogOffsetTop - 50, behavior: 'smooth'});
    }
}

function enterEdit(type:WikiParaType, underlyingId:number){
    if(type == WikiParaType.Text && underlyingId){
        jumpToTextSectionEdit(underlyingId)
    }else if(type == WikiParaType.Table && underlyingId){
        jumpToFreeTableEdit(underlyingId)
    }
}

async function toggleSealed(){
    if(!displayInfo.value){
        return;
    }
    const setTo = !displayInfo.value.Sealed;
    const s = await api.wiki.wikiItem.setSealed(displayInfo.value.WikiId, setTo);
    if(s){
        displayInfo.value.Sealed = setTo;
    }
}

const api:Api = injectApi();
const iden = injectIdentityInfoProvider();
let clickFold:TitleClickFold|undefined;
let wikiLinkClick:WikiLinkClick|undefined;
let imgClickJump:ImageClickJump;
const {listenFootNoteJump,disposeFootNoteJump,footNoteJumpCallBack} = useFootNoteJump();
const wikiViewArea = ref<HTMLDivElement>();
let titlesInContent:HTMLElement[] 
const router = useRouter();
const { jumpToDiffContentHistory, jumpToDiffContentHistoryForWiki } = useDiffRoutesJump();
const { jumpToWikiEdit, jumpToWikiContentEdit } = useWikiRoutesJump();
const { jumpToFreeTableEdit } = useTableRoutesJump();
const { jumpToTextSectionEdit } = useTextSectionRoutesJump();
const { jumpToUserCenter, jumpToUserGroup } = useIdentityRoutesJump();
const  mainDivStyle = injectMainDivStyle();
onMounted(async()=>{
    mainDivStyle.value.maxWidth = '1300px'
    mainDivStyle.value.padding = '0px'
    mainDivStyle.value.width = '100%'
    await init();
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

const focusImg = ref<string>();
const focusImgDesc = ref<string>();
const imgFocusViewElement = ref<InstanceType<typeof ImageFocusView>>();
const wikiViewScrollMemory = injectWikiViewScrollMemory()
const routeRendererContainer = ref<HTMLDivElement>()

async function init(changedPathName?:boolean){
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
    imgClickJump = new ImageClickJump((src, alt)=>{
        focusImg.value = src;
        focusImgDesc.value = alt
    });
    imgClickJump.listen(wikiViewArea.value);

    const commentTitle = document.getElementById(titleElementId(cmtTitleId) || "??");
    if(commentTitle)
        titlesInContent.push(commentTitle)

    viewAreaScrollHandler();
    wikiViewArea.value?.addEventListener('scroll', _e=>viewAreaScrollHandler(false));

    wikiLinkClick = new WikiLinkClick(pathName => {
        router.push(`/w/${pathName}`)
    });
    wikiLinkClick.listen(wikiViewArea.value);

    if(props.viewCmt){
        moveToTitle(cmtTitleId)
    }else if(changedPathName){
        wikiViewScrollMemory.read(props.wikiPathName, wikiViewArea.value)
        viewAreaScrollHandler(true)
    }

    if(routeRendererContainer.value){
        const rr = new RouteRenderer(routeRendererContainer.value)
        rr.run()
    }
}
onUnmounted(()=>{
    mainDivStyle.value = {}
    clickFold?.dispose();
    imgClickJump?.dispose();
    disposeFootNoteJump();
    swl?.stopListen();
    recoverTitle();
})
</script>

<template>
<div class="wikiViewFrame">
    <div v-if="data && currentUser && displayInfo" class="wikiView"
        :class="{noCopy:data.OwnerId !== currentUser.Id}" ref="wikiViewArea">
        <div class="invisible" v-html="styles"></div>
        <div class="invisible" ref="preScripts"></div>
        <div class="masterTitle">
            {{data.Title}}
        </div>
        <div class="info" v-if="displayInfo">
            <div class="owner">
                所有者<img :src="displayInfo.UserAvtSrc || userDefaultAvatar" :alt="displayInfo.UserName+' 头像'" class="smallAvatar"/>
                <span @click="jumpToUserCenter(displayInfo?.UserName||'??')">{{ displayInfo.UserName }}</span>
                <div class="updateTime">更新于 {{ data.Update }}</div>
                <div class="groupLabels">
                    <div v-for="label in displayInfo.UserGroupLabels" @click="jumpToUserGroup(label.Id)">
                        {{ label.Name }}
                    </div>
                </div>
            </div>
            <div class="btns">
                <div>
                    <button @click="jumpToDiffContentHistoryForWiki(wikiPathName)" class="minor">历史</button>
                    <button v-if="displayInfo.CurrentUserAccess" @click="jumpToWikiEdit(wikiPathName)">设置</button>
                    <button v-if="displayInfo.CurrentUserAccess" @click="jumpToWikiContentEdit(wikiPathName)">编辑</button>
                </div>
                
                <LongPress v-if="currentUser.Type >= UserType.Admin" :reached="toggleSealed">
                    {{ displayInfo.Sealed ? '解除隐藏': '隐藏词条'}}
                </LongPress>
            </div>
        </div>
        <div v-if="displayInfo.Sealed" class="sealed">该词条已被隐藏</div>
        <div v-for="p in data.Paras" class="para">
            <div v-if="p.ParaType==WikiParaType.Text || p.ParaType==WikiParaType.Table">
                <h1 :id="titleElementId(p.TitleId)">
                    <span v-html="p.Title"></span>
                    <div class="h1Sep"></div>
                    <div v-if="p.ParaType == WikiParaType.Table && p.IsFromFile" class="editBtn">
                        <a :href="fileDownloadLink(p.UnderlyingId)">下载</a>
                    </div>
                    <div v-if="p.HistoryViewable" class="editBtn" @click="jumpToDiffContentHistory(diffContentTypeFromParaType(p.ParaType),p.UnderlyingId)">历史</div>
                    <div v-if="p.Editable && displayInfo.CurrentUserAccess" class="editBtn" @click="enterEdit(p.ParaType,p.UnderlyingId)">编辑</div>
                </h1>
                <div class="indent" v-html="p.Content">
                </div>
            </div>
            <div v-if="p.ParaType==WikiParaType.File && p.Content">
                <div v-if="canDisplayAsImage(p.Content, p.Bytes)" class="imgPara">
                    <img :src="p.Content" :alt="p.Title"/>
                    <div>{{ p.Title }}</div>
                </div>
                <div v-else-if="getFileType(p.Content)=='audio'">
                    <audio :src="p.Content" controls></audio>
                </div>
                <div v-else-if="getFileType(p.Content)=='video'">
                    <video :src="p.Content" controls></video>
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
        <div style="color:gray;text-align: center;margin-top: 20px;font-size: 14px;">
            词条作者不另外说明的情况下保留所有权利，未经作者允许请勿转载、使用、改编
        </div>
        <Recommends v-if="recommendsLoaded" :path-name="wikiPathName"></Recommends>
        <h1 :id="titleElementId(cmtTitleId)">评论区<div class="h1Sep"></div></h1>
        <div class="comments" :class="{commentsNotLoaded: !commentsLoaded}">
            <Comment v-if="commentsLoaded && data" :obj-id="data?.Id" :type="CommentTargetType.Wiki"></Comment>
            <div v-else style="text-align: center;color:gray">(请继续上滑加载评论区)</div>
        </div>
        <Footer></Footer>
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

    <ImageFocusView v-if="focusImg" :img-src="focusImg" :desc="focusImgDesc" 
        :close="()=>{focusImg=undefined;focusImgDesc=undefined}" ref="imgFocusViewElement">
    </ImageFocusView>

    <div ref="routeRendererContainer" style="display: none;"></div>
</div>
</template>

<style scoped lang="scss">
@import '@/styles/globalValues';
.groupLabels{
    display: flex;
    flex-wrap: wrap;
    gap: 3px;
    margin-top: 6px;
    div{
        white-space: nowrap;
        padding: 2px 4px 2px 4px;
        border-radius: 1000px;
        font-size: 14px;
        background-color: rgb(72, 180, 26);
        color: white;
        cursor: pointer;
        &:hover{
            background-color: green;
        }
    }
}
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
    box-sizing: border-box;
    padding: 20px 0px 20px 0px;
    margin-right: 40px;
}
.subTitlesFoldBtn{
    position: fixed;
    bottom: 15px;
    right: 15px;
    width: 30px;
    height: 30px;
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
    // max-width: 900px;
    position: relative;
    height:100%;
    flex-grow: 1;
    overflow-y: scroll;
    overflow-x: hidden;

    margin-right: -220px;
    padding-right: 220px;
    padding-left: 10px;
}
.wikiView.noCopy .para{
    user-select: none;
}

.info{
    display: flex;
    justify-content: space-between;
    align-items: center;
    .owner,.owner>span{
        font-size: 16px;
        color: #666;
        white-space: nowrap;
        overflow: hidden;
        text-overflow: ellipsis;
        img{
            margin: 0px 5px 0px 5px;
            vertical-align: bottom;
        }
        span{
            cursor: pointer;
            &:hover{
                text-decoration: underline;
            }
        }
    }
    .updateTime{
        margin-top: 6px;
    }
}
.btns{
    display: flex;
    flex-direction: column;
    gap: 2px;
    flex-shrink: 0;
}
.sealed{
    color:red;
    font-weight: bold;
    margin-top: 5px;
    text-align: center;
}

@media screen and (max-width: 1000px){
    .wikiView{
        margin-right: 0px;
        padding-right: 10px;
    }
    .subTitles{
        position: fixed;
        right: 0px;
        bottom: 0px;
        height: unset;
        top: $topbar-height;
        border-top: 1px solid #ddd;
        padding-top: 20px;
        box-shadow: 0px 0px 12px 0px black;
        margin-right: 0px;
        z-index: 950;
        background-color: white;
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
        background-color: black;
        opacity: 0.4;
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