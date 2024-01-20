import { IndexResult } from "../../components/Index";

export interface FileDir{
    Id:number,
    Name:string,
    Depth:number,
    
    CanEditInfo:boolean,
    CanPutFile:boolean
}

export interface FileDirIndexResult{
    SubDirs:IndexResult,//FileDirSubDir
    Items:IndexResult|undefined,//FileDirItem
    ThisDirId:number
}
export interface FileDirSubDir{
    Id:number,
    Name:string,
    Updated:string,
    OwnerId:number,
    OwnerName:string,
    ByteCount:number,
    FileNumber:number,

    showChildren?:boolean|undefined
}
export interface FileDirItem
{
    Id:number,
    Name:string,
    Updated:string
    ByteCount:number
    Url:string
}


export interface StagingFile{
    file:File,
    displayName:string,
    storeName?:string,
    editing?:boolean
}
export type FileUploadDist = "upload"|"wikiFile"|"material"|"forum"|"test"




export interface TakeContentResItem{
    Id:number,
    Name:string,
    Url:string,
    ByteCount:number
}
export interface TakeContentResSubDir{
    Id:number,
    Name:string,
    showChildren:boolean|undefined
}
export interface TakeContentResult {
    SubDirs:Array<TakeContentResSubDir>;
    Items:Array<TakeContentResItem>
}


export interface PutInFileRequest {
    DirPath:string[]
    FileItemId:number
}
export interface PutInThingsRequest {
    DirPath:string[]
    FileItemIds?:number[]
    FileDirIds?:number[]
}
export interface FileDirPutInResult{
    FileItemSuccess?:number[]
    FileDirSuccess?:number[]
    FailMsg?:string
}