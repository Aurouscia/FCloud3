export interface WikiTemplate {
    Id: number;
    Name?: string;
    Source?: string;
    PreScripts?: string;
    PostScripts?: string;
    Styles?: string;
    Demo?: string;
    IsSingleUse: boolean;
    CreatorUserId: number;
    Created: Date;
    Updated: Date;
    Deleted: boolean;
}

export interface WikiTemplateListItem {
    Id: number;
    Name?: string;
    Updated?: string;
    CreatorUserId: number;
    CreatorName?: string;
}

export interface WikiTemplatePreviewResponse{
    HtmlSource:string,
    PreScripts:string,
    PostScripts:string,
    Styles:string
}