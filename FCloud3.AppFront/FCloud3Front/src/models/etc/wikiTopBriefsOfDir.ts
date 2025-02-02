export interface WikiTopBriefOfDirResponse
{
    Items: WikiTopBriefOfDirItem[]
}

export interface WikiTopBriefOfDirItem {
    Title?: string;
    Time?: string;
    OwnerName?: string;
    Brief?: string;
}