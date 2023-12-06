export interface TextSection{
    Id:number,
    Title:string,
    Content:string
}
export interface TextSectionPreviewResponse{
    HtmlSource:string,
    PreScripts:string,
    PostScripts:string,
    Styles:string
}