import { wikiParaType } from "../wiki/wikiParaTypes";

export interface WikiParsingResult {
    Title: string;
    UsedRules: string[];
    FootNotes: string[];
    SubTitles: ParserTitleTreeNode[];
    Paras: WikiParsingResultItem[];
}

export interface WikiParsingResultItem {
    Title?: string;
    Content?: string;
    ParaType: keyof typeof wikiParaType
}

export interface ParserTitleTreeNode {
    Text: string
    Id: number
    Level: number
    Subs: ParserTitleTreeNode[]
}