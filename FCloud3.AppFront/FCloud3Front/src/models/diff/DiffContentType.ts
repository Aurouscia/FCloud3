import { WikiParaTypes } from "../wiki/wikiParaTypes";

export enum DiffContentType
{
    None = 0,
    TextSection = 1,
    FreeTable = 2
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