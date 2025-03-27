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
    AsDir:number
}

export interface FileDirIndexResult{
    SubDirs:FileDirSubDir[],
    Items:FileDirItem[]
    Wikis:FileDirWiki[]
    ThisDirId:number,
    OwnerId:number,
    OwnerName:string,
    AsDirId:number,
    AsDirFriendlyPath?:string[]
    FriendlyPath:string[],
    TotalCount:number,
    PageCount:number,
    PageIdx:number
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