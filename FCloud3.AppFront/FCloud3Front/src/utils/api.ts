import _ from 'lodash';
import { IndexQuery, IndexResult } from "../components/Index/index";
import { User } from "../models/identities/user";
import { TextSection, TextSectionPreviewResponse } from "../models/textSection/textSection";
import { WikiPara } from "../models/wiki/wikiPara";
import { wikiParaType } from "../models/wiki/wikiParaTypes";
import { HttpClient } from "./httpClient";
import { IdentityInfo } from "./userInfo";
import { FileItemDetail, StagingFile } from "../models/files/fileItem";
import { FileDirIndexResult, PutInFileRequest, PutInThingsRequest, FileDirPutInResult, FileDirCreateRequest } from '../models/files/fileDir';
import { FileDir } from "../models/files/fileDir";
import { QuickSearchResult } from '../models/sys/quickSearch';
import { UserGroup, UserGroupDetailResult, UserGroupListResult } from '../models/identities/userGroup';
import { WikiItem } from '../models/wiki/wikiItem';
import { FreeTable } from '../models/table/freeTable';
import { AuthGrant, AuthGrantOnText, AuthGrantViewModel, authGrantOn } from '../models/identities/authGrant';


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
                    "已成功登录");
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
                    "修改成功")
                if(res.success){
                    return true
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
            }
        },
        userGroup:{
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
                    "保存成功"
                )
                return resp.success;
            },
            addUserToGroup:async(userId:number,groupId:number)=>{
                const resp = await this.httpClient.request(
                    "/api/UserGroup/AddUserToGroup",
                    "get",
                    {userId,groupId},
                    "成功邀请"
                )
                return resp.success
            },
            answerInvitation:async(groupId:number,accept:boolean)=>{
                const resp = await this.httpClient.request(
                    "/api/UserGroup/AnswerInvitation",
                    "get",
                    {groupId,accept},
                    "操作成功"
                )
                return resp.success
            },
            removeUserFromGroup:async(userId:number,groupId:number)=>{
                const resp = await this.httpClient.request(
                    "/api/UserGroup/RemoveUserFromGroup",
                    "get",
                    {userId,groupId},
                    "操作成功"
                )
                return resp.success
            },
            leave:async(groupId:number)=>{
                const resp = await this.httpClient.request(
                    "/api/UserGroup/Leave",
                    "get",
                    {groupId},
                    "操作成功"
                )
                return resp.success
            },
            dissolve:async(id:number)=>{
                const resp = await this.httpClient.request(
                    "/api/UserGroup/Dissolve",
                    "get",
                    {id},
                    "操作成功"
                )
                return resp.success
            },
        },
        authGrant:{
            getList:async(onType:AuthGrantOnText, onId:number)=>{
                const on = authGrantOn(onType);
                const resp = await this.httpClient.request(
                    "/api/AuthGrant/GetList",
                    "get",
                    {on,onId}
                )
                if(resp.success){
                    return resp.data as AuthGrantViewModel[];
                }
            },
            setOrder:async(onType:AuthGrantOnText, onId:number, ids:number[])=>{
                const on = authGrantOn(onType);
                const resp = await this.httpClient.request(
                    "/api/AuthGrant/SetOrder",
                    "postRaw",
                    {on,onId,ids},
                    "设置顺序成功"
                )
                return resp.success;
            },
            add:async(authGrant:AuthGrant)=>{
                const resp = await this.httpClient.request(
                    "/api/AuthGrant/Add",
                    "postRaw",
                    authGrant,
                    "新增成功"
                )
                return resp.success;
            },
            remove:async(id:number)=>{
                const resp = await this.httpClient.request(
                    "/api/AuthGrant/Remove",
                    "get",
                    {id},
                    "删除成功"
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
                "保存成功"
            )
            return res.success
        },
        loadSimple: async(id:number)=>{
            const res = await this.httpClient.request(
                "/api/WikiItem/LoadSimple",
                "get",
                {id:id});
            if(res.success){
                return res.data as Array<WikiPara>
            }
        },
        insertPara:async(req:{id:number,afterOrder:number,type:keyof typeof wikiParaType}) => {
            const res = await this.httpClient.request(
                "/api/WikiItem/InsertPara",
                "postForm",
                req,
                "成功插入新段落")
            if(res.success){
                return res.data as Array<WikiPara>
            }
        },
        setParaOrders:async(req:{id:number,orderedParaIds:number[]})=>{
            const res = await this.httpClient.request(
                "/api/WikiItem/SetParaOrders",
                "postRaw",
                req,
                "成功修改顺序")
            if(res.success){
                return res.data as Array<WikiPara>
            }
        },
        removePara:async(req:{id:number,paraId:number})=>{
            const res = await this.httpClient.request(
                "/api/WikiItem/RemovePara",
                "postForm",
                req,
                "成功删除")
            if(res.success){
                return res.data as Array<WikiPara>
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
                "创建成功")
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
                "移出成功")
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
                    "成功为段落设置文件"
                )
                return resp.success
            }
        }
    }
    textSection = {
        createForPara:async (req:{paraId:number}) => {
            const res = await this.httpClient.request(
                "/api/TextSection/CreateForPara",
                "postForm",
                req)
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
                "已保存修改")
            if(res.success){
                return res.data as boolean;
            }
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
                    {'paraId':paraId}
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
                    "修改成功"
                );
                return resp.success;
            },
            saveContent:async(id:number,data:string)=>{
                const resp = await this.httpClient.request(
                    "/api/FreeTable/SaveContent",
                    "postForm",
                    {id,data}
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
        save:async(req: StagingFile, dist:string)=>{
            const res = await this.httpClient.request(
                "/api/FileItem/Save",
                "postForm",
                {
                    ToSave:req.file,
                    DisplayName:req.displayName,
                    StoreName:req.storeName,
                    StorePath:dist
                },`成功上传：${_.truncate(req.displayName,{length:8})}`);
            if(res.success){
                return res.data as {CreatedId:number};
            }
        },
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
                "编辑成功"
            )
            if(res.success){
                return true;
            }
        },
        putInFile:async(dirPath:string[], fileItemId:number)=>{
            const reqData:PutInFileRequest={
                DirPath:dirPath,
                FileItemId:fileItemId
            }
            const res = await this.httpClient.request(
                "/api/FileDir/PutInFile",
                "postRaw",
                reqData,
                "成功将文件放入本文件夹"
            )
            if(res.success){
                return true;
            }
        },
        putInThings:async(dirPath:string[], fileItemIds:number[], fileDirIds:number[], wikiItemIds:number[])=>{
            const reqNum = (fileItemIds.length||0) + (fileDirIds.length||0) + (wikiItemIds.length||0)
            if(!reqNum){return;}
            const reqData:PutInThingsRequest={
                DirPath:dirPath,
                FileItemIds:fileItemIds,
                FileDirIds:fileDirIds,
                WikiItemIds:wikiItemIds
            }
            const res = await this.httpClient.request(
                "/api/FileDir/PutInThings",
                "postRaw",
                reqData
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
                    this.httpClient.httpCallBack("warn",`${reqNum-num}个操作失败`);
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
                "创建成功"
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
                "成功删除"
            );
            if(resp.success){
                return true;
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
        }
    }
}