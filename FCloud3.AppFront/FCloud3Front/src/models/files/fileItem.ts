export interface FileItem{
    Id:number,
    DisplayName:string,
    StorePathName:string,
    ByteCount:number,
    InDir:number,
    CreatorUserId:number
}
export interface FileItemDetail{
    ItemInfo:FileItem
    FullUrl:string
    DirPath:string[]
    DirFriendlyPath:string[]
}

export interface StagingFile{
    file:File,
    displayName:string,
    displayNameWithoutExt:string,
    storeName?:string,
    editing?:boolean,
    md5?:string
}
export type FileUploadDist = "upload"|"wikiFile"|"material"|"forum"|"test"

export interface FileUploadRequest
{
    ToSave:File
    DisplayName:string
    StorePath:string
    StoreName:string,
    Hash:string
}

export const fileUploadMaxSize = 15*1024*1024
