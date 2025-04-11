import { WikiParaType } from "../wiki/wikiParaType";

export interface WikiParsingResult {
    Id:number
    Update:string
    OwnerId:number
    Title: string;
    UsedRules: string[];
    FootNotes: string[];
    SubTitles: ParserTitleTreeNode[];
    Paras: WikiParsingResultItem[];
    Styles: string
    PreScripts: string
    PostScripts: string
}

export interface WikiParsingResultItem {
    Title?: string;
    TitleId: number
    Content?: string;
    ParaId: number;
    ParaType: WikiParaType;
    IsFromFile: boolean;
    UnderlyingId: number;
    Bytes:number;
    Editable: boolean;
    HistoryViewable: boolean;
}

export interface ParserTitleTreeNode {
    Text: string
    Id: number
    Level: number
    Subs?: ParserTitleTreeNode[]
}