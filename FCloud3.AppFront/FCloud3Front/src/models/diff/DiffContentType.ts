import { WikiParaTypes } from "../wiki/wikiParaTypes";

export enum DiffContentType
{
    None = 0,
    TextSection = 1,
    FreeTable = 2
}

export function diffContentTypeFromStr(s:string){
    switch(s){
        case "TextSection":
            return DiffContentType.TextSection;
        case "FreeTable":
            return DiffContentType.FreeTable;
        default:
            return DiffContentType.None;
    }
}

export function diffContentTypeToStr(t:DiffContentType){
    switch(t){
        case DiffContentType.TextSection:
            return "TextSection";
        case DiffContentType.FreeTable:
            return "FreeTable";
        default:
            return "None";
    }
}

export function diffContentTypeFromParaType(t:WikiParaTypes){
    switch(t){
        case WikiParaTypes.Text:
            return DiffContentType.TextSection;
        case WikiParaTypes.Table:
            return DiffContentType.FreeTable;
        default:
            return DiffContentType.None;
    }
}