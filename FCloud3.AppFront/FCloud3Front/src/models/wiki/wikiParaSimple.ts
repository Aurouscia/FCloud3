import { wikiParaType } from "./wikiParaTypes";

export interface wikiParaSimple{
    Id:number,
    Title:string,
    Content:string,
    Order:number,
    Type:keyof typeof wikiParaType,

    posY?:number,
    isMoveing?:boolean,
    displayOrder:number
}