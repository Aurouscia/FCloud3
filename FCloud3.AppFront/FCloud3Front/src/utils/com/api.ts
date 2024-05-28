import _ from 'lodash';
import { IndexQuery, IndexResult } from "@/components/Index/index";
import { User, UserType } from "@/models/identities/user";
import { TextSection, TextSectionPreviewResponse } from "@/models/textSection/textSection";
import { WikiParaDisplay } from "@/models/wiki/wikiPara";
import { WikiParaTypes } from "@/models/wiki/wikiParaTypes";
import { HttpClient } from "./httpClient";
import { IdentityInfo } from "../globalStores/identityInfo";
import { FileItemDetail, FileUploadRequest, StagingFile } from "@/models/files/fileItem";
import { FileDirIndexResult, PutInFileRequest, PutInThingsRequest, FileDirPutInResult, FileDirCreateRequest } from '@/models/files/fileDir';
import { FileDir } from "@/models/files/fileDir";
import { QuickSearchResult } from '@/models/sys/quickSearch';
import { UserGroup, UserGroupDetailResult, UserGroupListResult } from '@/models/identities/userGroup';
import { WikiItem } from '@/models/wiki/wikiItem';
import { FreeTable } from '@/models/table/freeTable';
import { AuthGrant, AuthGrantOn, AuthGrantViewModel } from '@/models/identities/authGrant';
import { WikiTemplate, WikiTemplateListItem, WikiTemplatePreviewResponse } from '@/models/wikiParsing/wikiTemplate';
import { WikiParsingResult } from '@/models/wikiParsing/wikiParsingResult';
import { WikiRulesCommonsResult } from '@/models/wikiParsing/wikiRulesCommonsResult';
import { WikiTitleContainAutoFillResult, WikiTitleContainGetAllRequest, WikiTitleContainListModel, WikiTitleContainSetAllRequest, WikiTitleContainType } from '@/models/wiki/wikiTitleContain';
import { DiffContentType } from '@/models/diff/DiffContentType';
import { DiffContentHistoryResult } from '@/models/diff/DiffContentHistory';
import { DiffContentDetailResult } from '@/models/diff/DiffContentDetail';
import { HeartbeatRequest } from '@/models/sys/heartbeat';
import { Comment, CommentTargetType, CommentViewResult } from '@/models/messages/comment';
import { NotifViewResult, NotificationGetRequest } from '@/models/messages/notification';
import { OpRecordGetRequest, OpRecordViewModel } from '@/models/messages/opRecord';
import { LatestWorkViewItem } from '@/models/etc/latestWork';
import { WikiRecommendModel } from '@/models/wikiParsing/wikiRecommend';


export class Api{
    private httpClient: HttpClient;
    constructor(httpClient:HttpClient){
        this.httpClient = httpClient;
    }
    identites = {
        authen:{
            login: async(reqObj:{userName:string,password:string})=>{
                var res = await this.httpClient.request(
                    "/api/Auth/Login",
                    "postForm",
                    reqObj,
                    "已成功登录",
                    true);
                if(res.success){
                    return res.data["token"] as string;
                }
            },
            identityTest: async()=>{
                var res = await this.httpClient.request(
                    "/api/Auth/IdentityTest",
                    "get")
                if(res.success){
                    return res.data as IdentityInfo
                }
            },
        },
        user:{
            create: async(name:string, pwd:string)=>{
                var res = await this.httpClient.request(
                    "/api/User/Create",
                    "postForm",
                    {name, pwd},
                    "已成功注册",
                    true
                )
                return res.success
            },
            edit: async()=>{
                var res = await this.httpClient.request(
                    "/api/User/Edit",
                    "get")
                if(res.success){
                    return res.data as User
                }
            },
            editExe: async(user:User)=>{
                var res = await this.httpClient.request(
                    "/api/User/EditExe",
                    "postRaw",
                    user,
                    "修改成功",
                    true)
                if(res.success){
                    return true
                }
            },
            index: async(q:IndexQuery)=>{
                var res = await this.httpClient.request(
                    "/api/User/Index",
                    "postRaw",
                    q)
                if(res.success){
                    return res.data as IndexResult
                }
            },
            getInfoByName: async(name:string)=>{
                var res = await this.httpClient.request(
                    "/api/User/GetInfoByName",
                    "get",
                    {name:name}
                )
                if(res.success){
                    return res.data as User
                }
            },
            replaceAvatar:async(id:number, materialId:number)=>{
                const res = await this.httpClient.request(
                    "/api/User/ReplaceAvatar",
                    "postForm",
                    {
                        id, materialId
                    },
                    "成功替换头像",
                    true
                )
                return res.success
            },
            setUserType:async(id:number, type:UserType)=>{
                const res = await this.httpClient.request(
                    "/api/User/SetUserType",
                    "postForm",
                    {
                        id, type
                    },
                    "成功设置身份",
                    true
                )
                return res.success
            },
            getInfo: async(id:number)=>{
                var res = await this.httpClient.request(
                    "/api/User/GetInfo",
                    "get",
                    {id})
                if(res.success){
                    return res.data as User
                }
            },
        },
        userGroup:{
            create:async(name:string)=>{
                const resp = await this.httpClient.request(
                    "/api/UserGroup/Create",
                    "postForm",
                    {name},
                    "创建成功",
                    true
                )
                return resp.success;
            },
            getList:async(search?:string)=>{
                const resp = await this.httpClient.request(
                    "/api/UserGroup/GetList",
                    "get",
                    {search:search||null}
                )
                if(resp.success){
                    return resp.data as UserGroupListResult
                }
            },
            getDetail:async(id:number)=>{
                const resp = await this.httpClient.request(
                    "/api/UserGroup/GetDetail",
                    "get",
                    {id}
                )
                if(resp.success){
                    return resp.data as UserGroupDetailResult
                }
            },
            edit:async(id:number)=>{
                const resp = await this.httpClient.request(
                    "/api/UserGroup/Edit",
                    "get",
                    {id}
                )
                if(resp.success){
                    return resp.data as UserGroup
                }
            },
            editExe:async(data:UserGroup)=>{
                const resp = await this.httpClient.request(
                    "/api/UserGroup/EditExe",
                    "postRaw",
                    data,
                    "保存成功",
                    true
                )
                return resp.success;
            },
            addUserToGroup:async(userId:number,groupId:number)=>{
                const resp = await this.httpClient.request(
                    "/api/UserGroup/AddUserToGroup",
                    "get",
                    {userId,groupId},
                    "成功邀请",
                    true
                )
                return resp.success
            },
            answerInvitation:async(groupId:number,accept:boolean)=>{
                const resp = await this.httpClient.request(
                    "/api/UserGroup/AnswerInvitation",
                    "get",
                    {groupId,accept},
                    "操作成功",
                    true
                )
                return resp.success
            },
            removeUserFromGroup:async(userId:number,groupId:number)=>{
                const resp = await this.httpClient.request(
                    "/api/UserGroup/RemoveUserFromGroup",
                    "get",
                    {userId,groupId},
                    "操作成功",
                    true
                )
                return resp.success
            },
            leave:async(groupId:number)=>{
                const resp = await this.httpClient.request(
                    "/api/UserGroup/Leave",
                    "get",
                    {groupId},
                    "操作成功",
                    true
                )
                return resp.success
            },
            dissolve:async(id:number)=>{
                const resp = await this.httpClient.request(
                    "/api/UserGroup/Dissolve",
                    "get",
                    {id},
                    "操作成功",
                    true
                )
                return resp.success
            },
        },
        authGrant:{
            getList:async(on:AuthGrantOn, onId:number)=>{
                const resp = await this.httpClient.request(
                    "/api/AuthGrant/GetList",
                    "get",
                    {on,onId}
                )
                if(resp.success){
                    return resp.data as AuthGrantViewModel;
                }
            },
            setOrder:async(on:AuthGrantOn, onId:number, ids:number[])=>{
                const resp = await this.httpClient.request(
                    "/api/AuthGrant/SetOrder",
                    "postRaw",
                    {on,onId,ids},
                    "设置顺序成功",
                    true
                )
                return resp.success;
            },
            add:async(authGrant:AuthGrant)=>{
                const resp = await this.httpClient.request(
                    "/api/AuthGrant/Add",
                    "postRaw",
                    authGrant,
                    "新增成功",
                    true
                )
                return resp.success;
            },
            remove:async(id:number)=>{
                const resp = await this.httpClient.request(
                    "/api/AuthGrant/Remove",
                    "get",
                    {id},
                    "删除成功",
                    true
                )
                return resp.success;
            }
        }
    }
    wiki = {
        edit: async(urlPathName:string)=>{
            const res = await this.httpClient.request(
                "/api/WikiItem/Edit",
                "get",
                {urlPathName}
            )
            if(res.success){
                return res.data as WikiItem
            }
        },
        editExe: async(data:WikiItem)=>{
            const res = await this.httpClient.request(
                "/api/WikiItem/EditExe",
                "postRaw",
                data,
                "保存成功",
                true
            )
            return res.success
        },
        loadSimple: async(id:number)=>{
            const res = await this.httpClient.request(
                "/api/WikiItem/LoadSimple",
                "get",
                {id:id});
            if(res.success){
                return res.data as Array<WikiParaDisplay>
            }
        },
        insertPara:async(req:{id:number,afterOrder:number,type:WikiParaTypes}) => {
            const res = await this.httpClient.request(
                "/api/WikiItem/InsertPara",
                "postForm",
                req,
                "成功插入新段落",
                true)
            if(res.success){
                return res.data as Array<WikiParaDisplay>
            }
        },
        setParaOrders:async(req:{id:number,orderedParaIds:number[]})=>{
            const res = await this.httpClient.request(
                "/api/WikiItem/SetParaOrders",
                "postRaw",
                req,
                "成功修改顺序",
                true)
            if(res.success){
                return res.data as Array<WikiParaDisplay>
            }
        },
        removePara:async(req:{id:number,paraId:number})=>{
            const res = await this.httpClient.request(
                "/api/WikiItem/RemovePara",
                "postForm",
                req,
                "成功删除",
                true)
            if(res.success){
                return res.data as Array<WikiParaDisplay>
            }
        },
        index:async(req:IndexQuery)=>{
            const res = await this.httpClient.request(
                "/api/WikiItem/Index",
                "postRaw",
                req,
            )
            if(res.success){
                return res.data as IndexResult;
            }
        },
        createInDir:async(title:string,urlPathName:string,dirId:number)=>{
            const res = await this.httpClient.request(
                "/api/WikiItem/CreateInDir",
                "postForm",
                {title,urlPathName,dirId},
                "创建成功",
                true)
            if(res.success){
                return true;
            }
            return false;
        },
        removeFromDir:async(wikiId:number,dirId:number)=>{
            const res = await this.httpClient.request(
                "/api/WikiItem/RemoveFromDir",
                "postForm",
                {wikiId,dirId},
                "移出成功",
                true)
            if(res.success){
                return true;
            }
            return false;
        },
        para:{
            setFileParaFileId:async(paraId:number,fileId:number)=>{
                const resp = await this.httpClient.request(
                    "/api/WikiPara/SetFileParaFileId",
                    "get",
                    {paraId,fileId},
                    "成功为段落设置文件",
                    true
                )
                return resp.success
            },
            setInfo:async(paraId:number,nameOverride:string|null)=>{
                const resp = await this.httpClient.request(
                    "/api/WikiPara/SetInfo",
                    "postForm",
                    {paraId, nameOverride},
                    "成功设置段落名称",
                    true
                )
                return resp.success
            }
        },
        wikiTitleContain:{
            getAll:async(req:WikiTitleContainGetAllRequest)=>{
                const res = await this.httpClient.request(
                    "/api/WikiTitleContain/GetAll",
                    "postRaw",
                    req)
                if(res.success){
                    return res.data as WikiTitleContainListModel
                }
            },
            setAll:async(req:WikiTitleContainSetAllRequest)=>{
                const res = await this.httpClient.request(
                    "/api/WikiTitleContain/SetAll",
                    "postRaw",
                    req,
                    "设置成功",
                    true)
                return res.success
            },
            autoFill:async(objId:number, containType:WikiTitleContainType, content:string)=>{
                const res = await this.httpClient.request(
                    "/api/WikiTitleContain/AutoFill",
                    "postForm",
                    {objId, containType, content},
                    undefined,
                    true)
                if(res.success){
                    return res.data as WikiTitleContainAutoFillResult
                }
            }
        }
    }
    wikiParsing={
        wikiTemplate:{
            getList:async()=>{
                const res = await this.httpClient.request(
                    "/api/WikiTemplate/GetList",
                    "get"
                )
                if(res.success){
                    return res.data as WikiTemplateListItem[]
                }
            },
            add:async(name:string)=>{
                const res = await this.httpClient.request(
                    "/api/WikiTemplate/Add",
                    "postForm",
                    {name},
                    "成功添加")
                return res.success
            },
            edit:async(id:number)=>{
                const res = await this.httpClient.request(
                    "/api/WikiTemplate/Edit",
                    "get",
                    {id}
                )
                if(res.success){
                    return res.data as WikiTemplate
                }
            },
            editExe:async(data:WikiTemplate)=>{
                const res = await this.httpClient.request(
                    "/api/WikiTemplate/EditExe",
                    "postRaw",
                    data,
                    "保存成功"
                )
                return res.success
            },
            remove:async(id:number)=>{
                const res = await this.httpClient.request(
                    "/api/WikiTemplate/Remove",
                    "get",
                    {id},
                    "删除成功"
                )
                return res.success
            },
            preview:async(data:WikiTemplate)=>{
                const res = await this.httpClient.request(
                    "/api/WikiTemplate/Preview",
                    "postRaw",
                    data
                );
                if(res.success){
                    return res.data as WikiTemplatePreviewResponse
                }
            }
        },
        wikiParsing:{
            getParsedWiki: async(pathName:string)=>{
                const res = await this.httpClient.request(
                    "/api/WikiParsing/GetParsedWiki",
                    "getStream",{pathName})
                if(res.success){
                    return res.data as WikiParsingResult
                }
            },
            getRulesCommons: async(ruleNames:string[])=>{
                const res = await this.httpClient.request(
                    "/api/WikiParsing/GetRulesCommons",
                    "postRaw",
                    ruleNames)
                if(res.success){
                    return res.data as WikiRulesCommonsResult
                }
            },
            getRecommend: async(pathName:string)=>{
                const res = await this.httpClient.request(
                    "/api/WikiParsing/GetRecommends",
                    "get",{pathName})
                if(res.success){
                    return res.data as WikiRecommendModel
                }
            }
        }
    }
    textSection = {
        createForPara:async (req:{paraId:number}) => {
            const res = await this.httpClient.request(
                "/api/TextSection/CreateForPara",
                "postForm",
                req,undefined,true)
            if(res.success){
                return res.data as {CreatedId:number}
            }
        },
        edit:async(id:number)=>{
            const res = await this.httpClient.request(
                "/api/TextSection/Edit",
                "get"
                ,{id:id})
            if(res.success){
                return res.data as TextSection
            }
        },
        editExe:async(textSection:TextSection)=>{
            const res = await this.httpClient.request(
                "/api/TextSection/EditExe",
                "postRaw",
                textSection,
                "已保存修改",
                true)
            return res.success;
        },
        preview:async(textSecId:number,content:string)=>{
            const res = await this.httpClient.request(
                "/api/TextSection/Preview",
                "postForm",
                {id:textSecId,content:content});
            if(res.success){
                return res.data as TextSectionPreviewResponse;
            }
        }
    }
    table = {
        freeTable:{
            load:async(id:number)=>{
                const resp = await this.httpClient.request(
                    "/api/FreeTable/Load",
                    "get",
                    {id}
                )
                if(resp.success){
                    return resp.data as FreeTable
                }
            },
            createForPara:async (paraId:number) => {
                const resp = await this.httpClient.request(
                    "/api/FreeTable/CreateForPara",
                    "get",
                    {'paraId':paraId},
                    undefined,true
                )
                if(resp.success){
                    return resp.data as {CreatedId:number}
                }
            },
            saveInfo:async(id:number,title:string)=>{
                const resp = await this.httpClient.request(
                    "/api/FreeTable/SaveInfo",
                    "postForm",
                    {id,title},
                    "修改成功",
                    true
                );
                return resp.success;
            },
            saveContent:async(id:number,data:string)=>{
                const resp = await this.httpClient.request(
                    "/api/FreeTable/SaveContent",
                    "postForm",
                    {id,data},
                    undefined,
                    true
                )
                return resp
            }
        }
    }
    fileItem = {
        getDetail:async(id:number)=>{
            const resp = await this.httpClient.request(
                "/api/FileItem/GetDetail",
                "get",
                {id}
            );
            if(resp.success){
                return resp.data as FileItemDetail
            }
        },
        save:async(file: StagingFile, dist:string)=>{
            const req:FileUploadRequest = {
                ToSave:file.file,
                DisplayName:file.displayName,
                StorePath:dist,
                StoreName:file.storeName||"",
                Hash:file.md5||""
            } 
            const res = await this.httpClient.request(
                "/api/FileItem/Save",
                "postForm",
                req,
                `成功上传：${_.truncate(req.DisplayName,{length:8})}`,
                true);
            if(res.success){
                return res.data as {CreatedId:number};
            }
        },
        editInfo:async(id:number, name:string)=>{
            const res = await this.httpClient.request(
                "/api/FileItem/EditInfo",
                "postForm",
                {id,name},
                "修改成功",
                true
            )
            return res.success
        },
        deleteFile:async(id:number)=>{
            const res = await this.httpClient.request(
                "/api/FileItem/Delete",
                "postForm",
                {id},
                "删除成功",
                true
            )
            return res.success
        }
    }
    fileDir = {
        getPathById:async(id:number)=>{
            const res = await this.httpClient.request(
                "/api/FileDir/GetPathById",
                "get",
                {id}
            );
            if(res.success){
                return res.data as string[]
            }
        },
        index:async(q: IndexQuery,path: string[])=>{
            const res = await this.httpClient.request(
                "/api/FileDir/Index",
                "postRaw",
                {
                    Query:q,
                    Path:path
                },
            )
            if(res.success){
                return res.data as FileDirIndexResult;
            }
        },
        editDir:async(id:number)=>{
            const res = await this.httpClient.request(
                "/api/FileDir/Edit",
                "get",
                {
                    id:id
                },
            )
            if(res.success){
                return res.data as FileDir;
            }
        },
        editDirExe:async(req:FileDir)=>{
            const res = await this.httpClient.request(
                "/api/FileDir/EditExe",
                "postRaw",
                req,
                "编辑成功",
                true
            )
            if(res.success){
                return true;
            }
        },
        putInFile:async(dirId:number, fileItemId:number)=>{
            const reqData:PutInFileRequest={
                DirId:dirId,
                FileItemId:fileItemId
            }
            const res = await this.httpClient.request(
                "/api/FileDir/PutInFile",
                "postRaw",
                reqData,
                "成功将文件放入本文件夹",
                true
            )
            if(res.success){
                return true;
            }
        },
        putInThings:async(dirId:number, fileItemIds:number[], fileDirIds:number[], wikiItemIds:number[])=>{
            const reqNum = (fileItemIds.length||0) + (fileDirIds.length||0) + (wikiItemIds.length||0)
            if(!reqNum){return;}
            const reqData:PutInThingsRequest={
                DirId:dirId,
                FileItemIds:fileItemIds,
                FileDirIds:fileDirIds,
                WikiItemIds:wikiItemIds
            }
            const res = await this.httpClient.request(
                "/api/FileDir/PutInThings",
                "postRaw",
                reqData,
                undefined,
                true
            )
            if(res.success){
                const data = res.data as FileDirPutInResult;
                const num = (data.FileDirSuccess?.length||0) + (data.FileItemSuccess?.length||0) + (data.WikiItemSuccess?.length||0)
                if(num==reqNum){
                    this.httpClient.httpCallBack("ok",`操作成功`);
                }
                if(num<reqNum){
                    if(num>0){
                        this.httpClient.httpCallBack("ok",`${num}个操作成功`);
                    }
                    const failNum = reqNum - num;
                    if(failNum > 1)
                        this.httpClient.httpCallBack("warn",`${failNum}个操作失败`);
                    else if(failNum == 1)
                        this.httpClient.httpCallBack("err", data.FailMsg || "操作失败")
                }
                return data
            }
        },
        create:async(parentDir:number,name:string,urlPathName:string)=>{
            const reqData:FileDirCreateRequest = {
                ParentDir:parentDir,
                Name:name,
                UrlPathName:urlPathName
            };
            const res = await this.httpClient.request(
                "/api/FileDir/Create",
                "postRaw",
                reqData,
                "创建成功",
                true
            )
            if(res.success){
                return true;
            }
        },
        delete:async(dirId:number)=>{
            const resp = await this.httpClient.request(
                "/api/FileDir/Delete",
                "get",
                {dirId},
                "成功删除",
                true
            );
            if(resp.success){
                return true;
            }
        }
    }
    material = {
        Index: async(q:IndexQuery, onlyMine:boolean)=>{
            const resp = await this.httpClient.request(
                `/api/Material/Index?onlyMine=${onlyMine}`,
                "postRaw",
                q);
            if(resp.success){
                return resp.data as IndexResult
            }
        },
        Add: async(name:string, desc:string|undefined, content:File)=>{
            const resp = await this.httpClient.request(
                "/api/Material/Add",
                "postForm",
                {
                    name, desc,
                    content
                },
                "上传成功",
                true
            )
            return resp.data as number
        },
        EditContent: async(id:number, content:File)=>{
            const resp = await this.httpClient.request(
                "/api/Material/EditContent",
                "postForm",
                {
                    id, content
                },
                "更改成功",
                true
            )
            return resp.success
        },
        EditInfo: async(id:number, name:string, desc:string|undefined)=>{
            const resp = await this.httpClient.request(
                "/api/Material/EditInfo",
                "postForm",
                {
                    id,name,desc
                },
                "更改成功",
                true
            )
            return resp.success
        },
        Delete: async(id:number)=>{
            const resp = await this.httpClient.request(
                "/api/Material/Delete",
                "postForm",
                {id},
                "删除成功",
                true
            )
            return resp.success
        }
    }
    diffContent = {
        history: async(type:DiffContentType, objId:number)=>{
            const resp = await this.httpClient.request(
                "/api/DiffContent/History",
                "postForm",
                {
                    type, objId
                }
            )
            if(resp.success){
                return resp.data as DiffContentHistoryResult
            }
        },
        detail: async(type:DiffContentType, objId:number, diffId:number)=>{
            const resp = await this.httpClient.request(
                "/api/DiffContent/Detail",
                "postForm",
                {
                    type, objId, diffId
                }
            )
            if(resp.success){
                return resp.data as DiffContentDetailResult
            }
        }
    }
    messages = {
        comment:{
            create:async(cmt:Comment)=>{
                const resp = await this.httpClient.request(
                    "/api/Comment/Create",
                    "postRaw",
                    cmt,
                    "成功评论",
                    true
                )
                return resp.success
            },
            view:async(type:CommentTargetType, objId:number)=>{
                const resp = await this.httpClient.request(
                    "/api/Comment/View",
                    "get",
                    {type, objId}
                )
                if(resp.success){
                    return resp.data as CommentViewResult[]
                }
            }
        },
        notification:{
            get: async(skip: number)=>{
                const req: NotificationGetRequest = {
                    Skip: skip
                }
                const resp = await this.httpClient.request(
                    "/api/Notification/Get", "postRaw" ,req ,"" ,true
                )
                return resp.data as NotifViewResult
            },
            count: async()=>{
                const resp = await this.httpClient.request(
                    "/api/Notification/Count", "get"
                )
                return resp.data as number
            },
            markRead: async(id:number|'all')=>{
                if(id=='all')
                    id = -1;
                const resp = await this.httpClient.request(
                    "/api/Notification/MarkRead", "get", {id}
                )
                return resp.success
            }
        },
        opRecord:{
            get: async(skip:number, user?:number)=>{
                const req: OpRecordGetRequest = {
                    Skip:skip, User:user || -1
                }
                const resp = await this.httpClient.request(
                    "/api/OpRecord/Get", "postRaw", req, undefined, skip>0
                )
                if(resp.success)
                    return resp.data as Array<OpRecordViewModel>
            }
        }
    }
    utils = {
        quickSearch:{
            wikiItem:async(s:string)=>{
                const res = await this.httpClient.request(
                    "/api/QuickSearch/WikiItem",
                    "get",
                    {s}
                )
                if(res.success){
                    return res.data as QuickSearchResult;
                }
            },
            userName:async(s:string)=>{
                const res = await this.httpClient.request(
                    "/api/QuickSearch/UserName",
                    "get",
                    {s}
                )
                if(res.success){
                    return res.data as QuickSearchResult;
                }
            },
            userGroupName:async(s:string)=>{
                const res = await this.httpClient.request(
                    "/api/QuickSearch/UserGroupName",
                    "get",
                    {s}
                )
                if(res.success){
                    return res.data as QuickSearchResult;
                }
            },
            fileItem:async(s:string)=>{
                const res = await this.httpClient.request(
                    "/api/QuickSearch/FileItem",
                    "get",
                    {s}
                )
                if(res.success){
                    return res.data as QuickSearchResult;
                }
            },
            material:async(s:string)=>{
                const res = await this.httpClient.request(
                    "/api/QuickSearch/Material",
                    "get",
                    {s}
                )
                if(res.success){
                    return res.data as QuickSearchResult;
                }
            }
        },
        urlPathName:async(input:string)=>{
            const resp = await this.httpClient.request(
                "/api/Utils/UrlPathName",
                "get",
                {input}
            )
            if(resp.success){
                return resp.data.res as string;
            }
        },
        applyBeingMember:async()=>{
            const resp = await this.httpClient.request(
                "/api/Utils/ApplyBeingMember",
                "get"
            )
            if(resp.success){
                return resp.data.res as string
            }
        },
        heartbeat:async(req:HeartbeatRequest)=>{
            const resp = await this.httpClient.request(
                "/api/Heartbeat/Do",
                "get",
                req
            )
            return resp.success
        },
        latestWork:async(uid=-1)=>{
            const resp = await this.httpClient.request(
                "/api/LatestWork/Get", "get", {uid}
            )
            if(resp.success){
                return resp.data as LatestWorkViewItem[]
            }
        }
    }
}