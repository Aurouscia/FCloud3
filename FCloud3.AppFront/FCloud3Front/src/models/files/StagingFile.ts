export interface StagingFile{
    file:File,
    displayName:string,
    storeName?:string,
    editing?:boolean
}

export type FileUploadDist = "upload"|"wikiFile"|"material"|"forum"|"test"