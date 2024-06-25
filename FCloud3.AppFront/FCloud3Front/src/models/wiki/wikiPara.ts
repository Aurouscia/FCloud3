import { WikiParaType } from "./wikiParaType";

export interface WikiParaDisplay{
    ParaId:number,
    UnderlyingId:number,
    Title:string,
    NameOverride:string|null,
    Content:string,
    Order:number,
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
    Type:WikiParaType.Text,
    Bytes:0
}

export const wikiParaDefaultFoldMark = "^"