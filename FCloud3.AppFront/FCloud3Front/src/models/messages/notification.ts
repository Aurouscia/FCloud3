export interface NotifViewResult{
    Items:NotifViewItem[],
    TotalCount:number
}

export interface NotifViewItem{
    Id: number
    Read: boolean
    Time: string
    Type: NotifType
    SId: number
    SName: string
    P1: number
    P1T: string
    P2: number
    P2T: string
}

export enum NotifType
{
    None = 0,
    CommentWiki = 10,
    CommentWikiReply = 11,
}

export interface NotificationGetRequest
{
    Skip: number
}