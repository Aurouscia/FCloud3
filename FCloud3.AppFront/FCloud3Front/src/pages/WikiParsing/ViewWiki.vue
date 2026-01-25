<script setup lang="ts">
import { computed, nextTick, onMounted, onUnmounted, ref, useTemplateRef, watch } from 'vue';
import { injectApi, injectIdentityInfoProvider, injectPop, injectWikiViewScrollMemory } from '@/provides';
import { Api, fileDownloadLink } from '@/utils/com/api';
import { ParserTitleTreeNode, WikiParsingResult } from '@/models/wikiParsing/wikiParsingResult';
import { WikiDisplayInfo, wikiDisplayInfoDefault } from '@/models/wikiParsing/wikiDisplayInfo';
import { findNearestUnhiddenAnces, hiddenSubClassName, TitleClickFold } from '@/utils/wikiView/titleClickFold';
import { WikiLinkClick } from '@/utils/wikiView/wikiLinkClick';
import { useFootNoteJump } from '@/utils/wikiView/footNoteJump';
import Loading from '@/components/Loading.vue';
import TitleTree from '@/components/Wiki/TitleTree.vue';
import Comment from '@/components/Messages/Comment.vue';
import { CommentTargetType, cmtTitleId } from '@/models/messages/comment';
import { updateScript } from '@/utils/wikiView/dynamicScriptUpdate';
import { WikiParaType } from '@/models/wiki/wikiParaType';
import { useTextSectionRoutesJump } from '../TextSection/routes/routesJump';
import { useWikiRoutesJump } from '../Wiki/routes/routesJump';
import { useWikiParsingRoutesJump } from './routes/routesJump';
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
import { runPluginsByWiki } from '@/utils/plugins/runPluginsByWiki'
import { useMainDivDisplayStore } from '@/utils/globalStores/mainDivDisplay';
import _ from 'lodash';
import { stickyContainTableRestrict } from '@/utils/wikiView/stickyContainTableRestrict';
import { paraTitleHiddenClass } from '@/utils/wikiView/titleHidden';

const props = defineProps<{
    wikiPathName: string;
    viewCmt?: string|boolean;
}>()
watch(()=>props.wikiPathName,async(_newVal, oldVal)=>{
    wikiViewScrollMemory.save(oldVal, wikiViewArea.value)
    data.value = undefined;
    commentsLoaded.value = false;
    recommendsLoaded.value = false;
    clickFold?.dispose()
    await init(true);
})

const data = ref<WikiParsingResult>();
const stylesContent = ref<string>("");
const preScripts = useTemplateRef('preScripts')
const postScripts = useTemplateRef('postScripts')
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

const titles = useTemplateRef('titles')
const subTitles = useTemplateRef('subTitles')
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
    if(title){
        let top = title.offsetTop
        if(title.classList.contains(hiddenSubClassName)){
            const ances = findNearestUnhiddenAnces(title)
            if(ances){
                pop.value.show(`请展开“${ances.text}”以查看内容`,'info')
                top = (ances.ances as HTMLElement).offsetTop
            }
        }
        isActiveMoving = true;
        wikiViewArea.value?.scrollTo({top:top - 10, behavior: 'smooth'})
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
    const sh = wikiViewArea.value?.scrollHeight;
    const st = wikiViewArea.value?.scrollTop;
    const oh = wikiViewArea.value?.offsetHeight;
    if(typeof sh != "number" || typeof st != "number" || typeof oh != "number"){
        return;
    }
    if(sh - st < oh+1600){
        recommendsLoaded.value = true
    }
    if(sh - st < oh+1200){
        commentsLoaded.value = true
    }

    lastScrollTime = Date.now();
    let currentTitleIdx = _.findLastIndex(titlesInContent, t=>
        !t.classList.contains(hiddenSubClassName) &&
        t.offsetTop < st + 30); //30是玄学数字，未搞清楚作用机理
    if(currentTitleIdx == -1){
        return
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
const wikiViewArea = useTemplateRef('wikiViewArea')
let titlesInContent:HTMLElement[] 
const router = useRouter();
const { jumpToDiffContentHistoryRoute, jumpToDiffContentHistoryForWikiRoute } = useDiffRoutesJump();
const { jumpToWikiEdit, jumpToWikiContentEdit, jumpToViewParaRawContentRoute } = useWikiRoutesJump();
const { jumpToViewWikiRoute } = useWikiParsingRoutesJump()
const { jumpToFreeTableEdit } = useTableRoutesJump();
const { jumpToTextSectionEdit } = useTextSectionRoutesJump();
const { jumpToUserCenter, jumpToUserGroup } = useIdentityRoutesJump();
const mainDivDisplayStore = useMainDivDisplayStore()
onMounted(async()=>{
    mainDivDisplayStore.restrictContentMaxWidth = false;
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
function subtitlesClean(){
    //标题可能因为各种原因消失（解析器转换，插件操作）需要清理
    if(!data.value)
        return;
    //移除所有隐藏的标题(仅段落大标题，有“titleHiddenClass”的)
    data.value.SubTitles = data.value.SubTitles.filter(t=>!paraTitleHiddenClass(t.Text))
    //移除所有找不到对应id元素的目录项
    data.value.SubTitles = filterSubtitleByDomExistence(data.value.SubTitles)
    //移除所有找不到对应id目录项的“滚动目标”
    titlesInContent = titlesInContent.filter(t=>{
        if(!t)
            return;
        const id = getIdFromElementId(t)
        return subtitlesContainId(data.value?.SubTitles || [], id)
    })
}
function filterSubtitleByDomExistence(filterTarget:ParserTitleTreeNode[]){
    filterTarget = filterTarget.filter(x=>{
        const titleEleId = titleElementId(x.Id)
        if(!titleEleId)
            return false
        return !!document.getElementById(titleEleId)
    })
    filterTarget.forEach(t=>{
        if(t.Subs)
            t.Subs = filterSubtitleByDomExistence(t.Subs)
    })
    return filterTarget
}
function subtitlesContainId(searchIn:ParserTitleTreeNode[], id:number){
    if(searchIn.some(x=>x.Id==id))
        return true;
    for(const node of searchIn){
        if(!node.Subs)
            continue
        if(subtitlesContainId(node.Subs, id))
            return true;
    }
    return false;
}

const timeViewMode = ref<'update'|'create'>('update')
function toggleTimeViewMode(){
    timeViewMode.value = timeViewMode.value == 'create' ? 'update' : 'create'
}

const focusImg = ref<string>();
const focusImgDesc = ref<string>();
const wikiViewScrollMemory = injectWikiViewScrollMemory()
const pop = injectPop()

async function init(changedPathName?:boolean){
    currentUser.value = await iden.getIdentityInfo();
    if(data.value){
        data.value.Paras = []
    }
    await load();

    listenFootNoteJump();
    footNoteJumpCallBack.value = (top)=>{
        wikiViewArea.value?.scrollTo({top: top-150, behavior: 'smooth'})
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

    wikiLinkClick = new WikiLinkClick(
        (wikiPathName) => router.resolve({name:'viewWiki', params:{wikiPathName}}).href,
        (pathName, _name) => {
            pop.value.show(`${pathName} 暂不存在`, 'failed')
        }
    );
    wikiLinkClick.listen(wikiViewArea.value);

    if(props.viewCmt){
        moveToTitle(cmtTitleId)
        router.replace(jumpToViewWikiRoute(props.wikiPathName)) // 去除路径中的viewCmt标记（确保其一次性）
    }else if(changedPathName){
        wikiViewScrollMemory.read(props.wikiPathName, wikiViewArea.value)
        viewAreaScrollHandler(true)
    }

    await runPluginsByWiki(data.value?.Paras.map(x=>x.Content))
    stickyContainTableRestrict()
    subtitlesClean()
    wikiLinkClick.listen(wikiViewArea.value);//再次转化链接，因为插件可能添加了新的段落
}
onUnmounted(()=>{
    mainDivDisplayStore.resetToDefault()
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
                <span class="ownerName" @click="jumpToUserCenter(displayInfo?.UserName||'??')">{{ displayInfo.UserName }}</span>
                <div class="updateTime">
                    <span v-if="timeViewMode=='update'">更新于 {{ data.Update }}</span>
                    <span v-else>创建于 {{ displayInfo.Created }}</span>
                    <span class="timeSwitchMark" @click="toggleTimeViewMode">⇄</span>
                </div>
                <div class="groupLabels">
                    <div v-for="label in displayInfo.UserGroupLabels" @click="jumpToUserGroup(label.Id)">
                        {{ label.Name }}
                    </div>
                </div>
            </div>
            <div class="btns">
                <div>
                    <RouterLink :to="jumpToDiffContentHistoryForWikiRoute(wikiPathName)" target="_blank"><button class="minor">历史</button></RouterLink>
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
                <h1 :id="titleElementId(p.TitleId)" :class="[paraTitleHiddenClass(p.Title)]">
                    <span v-html="p.Title" class="paraTitleText"></span>
                    <div class="paraTitleSep"></div>
                    <div v-if="p.ParaType == WikiParaType.Table && p.IsFromFile" class="editBtn">
                        <a :href="fileDownloadLink(p.UnderlyingId)">下载</a>
                    </div>
                    <RouterLink v-if="p.HistoryViewable" class="editBtn" :to="jumpToViewParaRawContentRoute(p.ParaId)" target="_blank">源码</RouterLink>
                    <RouterLink v-if="p.HistoryViewable" class="editBtn" :to="jumpToDiffContentHistoryRoute(diffContentTypeFromParaType(p.ParaType),p.UnderlyingId)" target="_blank">历史</RouterLink>
                    <div v-if="p.Editable && displayInfo.CurrentUserAccess" class="editBtn" @click="enterEdit(p.ParaType,p.UnderlyingId)">编辑</div>
                </h1>
                <div class="indent" v-html="p.Content">
                </div>
            </div>
            <div v-if="p.ParaType==WikiParaType.File && p.Content">
                <div v-if="canDisplayAsImage(p.Content, p.Bytes)" class="imgPara">
                    <img :src="p.Content" :alt="p.Title" loading="lazy"/>
                    <div>{{ p.Title }}</div>
                </div>
                <div v-else-if="getFileType(p.Content)=='audio'">
                    <audio :src="p.Content" controls loading="lazy" ></audio>
                </div>
                <div v-else-if="getFileType(p.Content)=='video'">
                    <video :src="p.Content" controls loading="lazy" ></video>
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
        <h1 :id="titleElementId(cmtTitleId)">评论区<div class="paraTitleSep"></div></h1>
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
        <div></div>
    </div>

    <ImageFocusView v-if="focusImg" :img-src="focusImg" :desc="focusImgDesc" 
        :close="()=>{focusImg=undefined;focusImgDesc=undefined}">
    </ImageFocusView>
</div>
</template>

<style scoped lang="scss">
@use '@/styles/globalValues';

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
    height: globalValues.$body-height;
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
    margin-right: 20px;
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
    div{
        width: 100%;
        height: 100%;
        mask-image: url('@/assets/menu.svg');
        mask-repeat: no-repeat;
        mask-size: contain;
        background-color: black;
    }
}
.cover{
    z-index: 100;
    display: none;
}
.wikiView{
    // max-width: 900px;
    position: relative;
    height:100%;
    flex-grow: 1;
    overflow-y: auto;
    overflow-x: hidden;

    margin-right: -220px;
    padding-right: 220px;
    padding-left: 10px;
}
.wikiView.noCopy .para{
    user-select: none;
}
.wikiView>*{
    max-width: 1100px;
    margin-left: auto;
    margin-right: auto;
}

.info{
    display: flex;
    justify-content: space-between;
    align-items: center;
    .owner{
        font-size: 16px;
        color: #666;
        white-space: nowrap;
        overflow: hidden;
        text-overflow: ellipsis;
        img{
            margin: 0px 5px 0px 5px;
            vertical-align: bottom;
        }
        .ownerName{
            cursor: pointer;
            &:hover{
                text-decoration: underline;
            }
        }
    }
    .updateTime{
        margin-top: 6px;
        .timeSwitchMark{
            cursor: pointer;
            user-select: none;
            color: #aaa;
            font-size: 14px;
            margin-left: 5px;
            &:hover{
                text-decoration: underline;
            }
        }
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
        top: globalValues.$topbar-height;
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