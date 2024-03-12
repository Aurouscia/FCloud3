import { WikiParaTypes } from "./wikiParaTypes";

export interface WikiPara{
    ParaId:number,
    UnderlyingId:number,
    Title:string,
    Content:string,
    Order:number,
    Type:WikiParaTypes,
}

export interface WikiParaRendered extends WikiPara{
    posY?:number,
    isMoveing?:boolean,
    displayOrder?:number
}