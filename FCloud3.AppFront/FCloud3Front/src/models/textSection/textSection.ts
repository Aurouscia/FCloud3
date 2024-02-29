export interface TextSection{
    Id:number,
    Title:string|null,
    Content:string|null
}
export interface TextSectionPreviewResponse{
    HtmlSource:string,
    PreScripts:string,
    PostScripts:string,
    Styles:string
}