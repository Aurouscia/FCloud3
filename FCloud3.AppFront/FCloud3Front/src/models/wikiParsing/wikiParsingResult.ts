import { WikiParaTypes } from "../wiki/wikiParaTypes";

export interface WikiParsingResult {
    Title: string;
    UsedRules: string[];
    FootNotes: string[];
    SubTitles: ParserTitleTreeNode[];
    Paras: WikiParsingResultItem[];
}

export interface WikiParsingResultItem {
    Title?: string;
    TitleId: number
    Content?: string;
    ParaType: WikiParaTypes
}

export interface ParserTitleTreeNode {
    Text: string
    Id: number
    Level: number
    Subs: ParserTitleTreeNode[]
}