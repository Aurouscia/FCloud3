export interface WikiTopBriefOfDirResponse
{
    Items: WikiTopBriefOfDirItem[]
}

export interface WikiTopBriefOfDirItem {
    Id: number
    PathName?:string
    Title?: string;
    Time?: string;
    OwnerName?: string;
    Brief?: string;
}