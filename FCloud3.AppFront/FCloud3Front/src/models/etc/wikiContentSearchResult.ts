import { DiffDisplayFrag } from "../diff/diffContentDetail";

export interface WikiContentSearchResult
{
    WikiItems:WikiContentSearchResultWikiItem[];
}
export interface WikiContentSearchResultWikiItem
{
    WikiUrlPathName:string 
    WikiTitle:string 
    Paras:Array<WikiContentSearchResultWikiPara>
}
export interface WikiContentSearchResultWikiPara
{
    ParaTitle:string
    Took:Array<DiffDisplayFrag>
}