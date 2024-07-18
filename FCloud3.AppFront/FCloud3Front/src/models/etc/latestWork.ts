export interface LatestWorkViewItem
{
    Type: LatestWorkType
    Title: string
    JumpParam: string
    ObjId: number
    UserId: number
    UserName: string
    Time: string
}
export enum LatestWorkType
{
    None = 0,
    Wiki = 1,
    File = 2
}