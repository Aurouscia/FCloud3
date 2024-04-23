import { WikiParaTypes } from "./wikiParaTypes";

export interface WikiParaDisplay{
    ParaId:number,
    UnderlyingId:number,
    Title:string,
    Content:string,
    Order:number,
    Type:WikiParaTypes,
    Bytes:number
}

export interface WikiParaRendered extends WikiParaDisplay{
    posY?:number,
    isMoveing?:boolean,
    displayOrder?:number
}