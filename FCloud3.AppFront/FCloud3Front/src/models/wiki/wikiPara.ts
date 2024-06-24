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

export const wikiParaDefaultFoldMark = "^"