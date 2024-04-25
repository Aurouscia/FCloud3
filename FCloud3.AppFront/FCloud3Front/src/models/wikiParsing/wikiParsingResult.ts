import { WikiParaTypes } from "../wiki/wikiParaTypes";

export interface WikiParsingResult {
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
    ParaType: WikiParaTypes;
    UnderlyingId: number;
    Bytes:number;
}

export interface ParserTitleTreeNode {
    Text: string
    Id: number
    Level: number
    Subs: ParserTitleTreeNode[]
}