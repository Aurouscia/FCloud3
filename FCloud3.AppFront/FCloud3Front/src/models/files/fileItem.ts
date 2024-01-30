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
    storeName?:string,
    editing?:boolean
}
export type FileUploadDist = "upload"|"wikiFile"|"material"|"forum"|"test"
