import _ from 'lodash';
import { IndexQuery, IndexResult } from "../components/Index/index";
import { User } from "../models/identities/user";
import { TextSection, TextSectionPreviewResponse } from "../models/textSection/textSection";
import { WikiPara } from "../models/wiki/wikiPara";
import { wikiParaType } from "../models/wiki/wikiParaTypes";
import { HttpClient } from "./httpClient";
import { IdentityInfo } from "./userInfo";
import { StagingFile } from "../models/files/fileItem";
import { FileDirIndexResult, PutInFileRequest, PutInThingsRequest, FileDirPutInResult } from '../models/files/fileDir';
import { FileDir } from "../models/files/fileDir";
import {QuickSearchResult} from '../models/sys/quickSearch';

export class Api{
    private httpClient: HttpClient;
    constructor(httpClient:HttpClient){
        this.httpClient = httpClient;
    }
    identites = {
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
    }
    wiki = {
        create: undefined,
        edit:undefined,
        editExe:undefined,
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
        createInDir:async(title:string,dirId:number)=>{
            const res = await this.httpClient.request(
                "/api/WikiItem/CreateInDir",
                "postForm",
                {title,dirId},
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
                "编辑成功")
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
    fileItem = {
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
        }
    }
    quickSearch = {
        wikiItem:async(s:string)=>{
            const res = await this.httpClient.request(
                "/api/QuickSearch/WikiItem",
                "get",
                {s}
            )
            if(res.success){
                return res.data as QuickSearchResult;
            }
        }
    }
}