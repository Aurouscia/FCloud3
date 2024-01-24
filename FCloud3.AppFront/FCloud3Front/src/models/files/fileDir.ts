import { IndexResult } from "../../components/Index";

export interface FileDir{
    Id:number,
    Name:string,
    UrlPathName:string,
    Depth:number,
    
    CanEditInfo:boolean,
    CanPutFile:boolean,
    CanPutWiki:boolean
}

export interface FileDirCreateRequest{
    ParentDir:number,
    Name:string,
    UrlPathName:string,
}

export interface FileDirIndexResult{
    SubDirs:IndexResult,//FileDirSubDir
    Items:IndexResult|undefined,//FileDirItem
    Wikis:IndexResult|undefined,//FileDirWiki
    ThisDirId:number,
    FriendlyPath:string[]
}
export interface FileDirSubDir{
    Id:number,
    Name:string,
    UrlPathName:string,
    Updated:string,
    OwnerName:string,
    ByteCount:number,
    FileNumber:number,
}
export interface FileDirItem
{
    Id:number,
    Name:string,
    Updated:string,
    OwnerName:string,
    ByteCount:number,
    Url:string
}
export interface FileDirWiki
{
    Id:number,
    Name:string,
    UrlPathName:string,
    Updated:string,
    OwnerName:string,
}



export interface PutInFileRequest {
    DirPath:string[]
    FileItemId:number
}
export interface PutInThingsRequest {
    DirPath:string[]
    FileItemIds?:number[]
    FileDirIds?:number[]
    WikiItemIds?:number[]
}
export interface FileDirPutInResult{
    FileItemSuccess?:number[]
    FileDirSuccess?:number[]
    WikiItemSuccess?:number[]
    FailMsg?:string
}