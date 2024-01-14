import { wikiParaType } from "./wikiParaTypes";

export interface WikiPara{
    ParaId:number,
    UnderlyingId:number,
    Title:string,
    Content:string,
    Order:number,
    Type:keyof typeof wikiParaType,
}

export interface WikiParaRendered extends WikiPara{
    posY?:number,
    isMoveing?:boolean,
    displayOrder?:number
}