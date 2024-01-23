import { IndexResult } from "../../components/Index";

export interface FileDir{
    Id:number,
    Name:string,
    Depth:number,
    
    CanEditInfo:boolean,
    CanPutFile:boolean,
    CanPutWiki:boolean
}

export interface FileDirIndexResult{
    SubDirs:IndexResult,//FileDirSubDir
    Items:IndexResult|undefined,//FileDirItem
    Wikis:IndexResult|undefined,//FileDirWiki
    ThisDirId:number
}
export interface FileDirSubDir{
    Id:number,
    Name:string,
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
    Updated:string,
    OwnerName:string,
}

// export interface TakeContentResItem{
//     Id:number,
//     Name:string,
//     Url:string,
//     ByteCount:number
// }
// export interface TakeContentResSubDir{
//     Id:number,
//     Name:string,
//     showChildren:boolean|undefined
// }
// export interface TakeContentResult {
//     SubDirs:Array<TakeContentResSubDir>;
//     Items:Array<TakeContentResItem>
// }


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