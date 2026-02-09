export interface OpRecordViewModel
{
    Id: number
    Content: string
    OpType: OpRecordOpType
    TargetType: OpRecordTargetType
    TargetObjId: number
    OtherObjId: number
    UserId: number
    UserName: number
    Time: string
}
export enum OpRecordOpType
{
    None = 0,
    Create = 10,
    Edit = 20,
    EditImportant = 21,
    Remove = 30,
}
export enum OpRecordTargetType
{
    None = 0,
    WikiItem = 10,
    WikiPara = 11,
    TextSection = 20,
    FreeTable = 21,
    FileDir = 30,
    FileItem = 31,
    UserGroup = 40,
    User = 41
}

export interface OpRecordGetRequest{
    Skip: number
    User: number
}

export function OpTypeReadable(t:OpRecordOpType){
    if(t == OpRecordOpType.Create)
        return "创建"
    if(t == OpRecordOpType.Edit)
        return "编辑"
    if(t == OpRecordOpType.EditImportant)
        return "变动"
    if(t == OpRecordOpType.Remove)
        return "删除"
}
export function OpTypeColor(t:OpRecordOpType){
    if(t == OpRecordOpType.Create)
        return "green"
    if(t == OpRecordOpType.Edit)
        return "cornflowerblue"
    if(t == OpRecordOpType.EditImportant)
        return "fuchsia"
    if(t == OpRecordOpType.Remove)
        return "red"
}
export function TargetTypeReadable(t:OpRecordTargetType){
    if(t == OpRecordTargetType.WikiItem)
        return "词条"
    if(t == OpRecordTargetType.WikiPara)
        return "段落"
    if(t == OpRecordTargetType.TextSection)
        return "文本段"
    if(t == OpRecordTargetType.FreeTable)
        return "表格"
    if(t == OpRecordTargetType.FileDir)
        return "目录"
    if(t == OpRecordTargetType.FileItem)
        return "文件"
    if(t == OpRecordTargetType.UserGroup)
        return "用户组"
    if(t == OpRecordTargetType.User)
        return "用户"
}