import { IndexResult } from "@/components/Index"

export interface User{
    Id:number,
    Name:string,
    Pwd:string,
    AvatarSrc:string
}

export enum UserType{
    Tourist = 0,
    Member = 1,
    Admin = 8,
    SuperAdmin = 9
}

export interface UserIndexItem{
    Id:number,
    Name:string,
    LastOperation:string,
    Avatar:string
    Type:string,
    TypeColor:string
    TypeEnum: UserType
}
export function getUserIndexItemsFromIndexResult(r:IndexResult):Array<UserIndexItem>{
    const idx_Id = r.ColumnNames.indexOf("Id")
    const idx_Name = r.ColumnNames.indexOf("Name")
    const idx_LastOp = r.ColumnNames.indexOf("LastOperation")
    const idx_Avatar = r.ColumnNames.indexOf("Avatar")
    const idx_Type = r.ColumnNames.indexOf("Type")
    const res:UserIndexItem[] = []
    for (let rowIdx = 0; rowIdx < r.Data.length; rowIdx++) {
        const row = r.Data[rowIdx];
        if (row.length < 5) {
            continue;
        }
        const type = parseInt(row[idx_Type]) as UserType
        const typeDisplayInfo = userTypeText(type)
        res.push({
            Id: parseInt(row[idx_Id]),
            Name: row[idx_Name],
            LastOperation: row[idx_LastOp],
            Avatar: row[idx_Avatar],
            Type: typeDisplayInfo.type,
            TypeColor: typeDisplayInfo.color,
            TypeEnum: type
        })
    }
    return res;
}
export function userTypeText(type: UserType): { type: string, color: string } {
    if (type == UserType.Tourist) {
        return { type: "游客", color: "#888" }
    }else if(type == UserType.Member){
        return {type: "会员", color: "#099"}
    }else if(type == UserType.Admin){
        return {type: "管理", color: "#990"}
    }else if(type == UserType.SuperAdmin){
        return {type: "超管", color: "#930"}
    }else{
        return {type: "未知", color: "#aaa"}
    }
}