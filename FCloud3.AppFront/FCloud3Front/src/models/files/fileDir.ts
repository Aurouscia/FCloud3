import { IndexResult } from "@/components/Index";

export interface FileDir{
    Id:number,
    Name:string,
    UrlPathName:string,
    Depth:number,
    
    CanEditInfo:boolean,
    CanPutThings:boolean,
    CanCreateSub:boolean
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
    OwnerId:number,
    OwnerName:string,
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

export function getSubDirsFromIndexResult(r?:IndexResult):Array<FileDirSubDir>{
    if(!r)
        return []
    return r.Data.map(r=>{return{
            Id: parseInt(r[0]),
            Name:r[1],
            UrlPathName:r[2],
            Updated:r[3],
            OwnerName:r[4],
            ByteCount:parseInt(r[5]),
            FileNumber:parseInt(r[6])
        }
    })
}
export function getFileItemsFromIndexResult(r?:IndexResult):Array<FileDirItem>{
    if(!r)
        return []
    return r.Data.map(r=>{
        return {
            Id:parseInt(r[0]),
            Name:r[1],
            Updated:r[2],
            OwnerName:r[3],
            ByteCount:parseInt(r[4]),
            Url:r[5]
        }
    })
}
export function getWikiItemsFromIndexResult(r?:IndexResult):Array<FileDirWiki>{
    if(!r)
        return []
    return r.Data.map(r=>{
        return{
            Id: parseInt(r[0]),
            Name:r[1],
            UrlPathName:r[2],
            Updated:r[3],
            OwnerName:r[4],
        }
    })
}


export interface PutInFileRequest {
    DirId:number
    FileItemId:number
}
export interface PutInThingsRequest {
    DirId:number
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