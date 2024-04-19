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
    DirPath:string[]
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
