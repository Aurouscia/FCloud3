export interface WikiTopBriefOfDirRequest
{
    DirId :number
    Skip :number
    Take :number
    TakeBriefAt :number
    TakeKvPairAt :number
}

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
    KvPairs?: { [key: string]: string }
}