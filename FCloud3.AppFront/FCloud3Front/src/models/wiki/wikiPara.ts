import { WikiParaType } from "./wikiParaType";

export interface WikiParaDisplay{
    ParaId:number,
    UnderlyingId:number,
    Title:string,
    NameOverride:string|null,
    Content:string,
    Order:number,
    TitleContainCount:number,
    Type:WikiParaType,
    Bytes:number
}

export interface WikiParaRendered extends WikiParaDisplay{
    posY?:number,
    isMoveing?:boolean,
    displayOrder?:number
}

export const wikiParaDisplayPlaceholder:WikiParaDisplay = {
    ParaId:0,
    UnderlyingId:0,
    Title:"",
    NameOverride:null,
    Content:"",
    Order:0,
    TitleContainCount:0,
    Type:WikiParaType.Text,
    Bytes:0
}

export const wikiParaDefaultFoldMark = "^"

export interface WikiParaRawContentRes
{
    ParaId:number
    ParaName:string|null
    ParaType:WikiParaType 
    ObjId:number
    OwnerId:number
    LastEdit:string
    Content:string
}