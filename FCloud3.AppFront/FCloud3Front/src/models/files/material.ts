import { IndexResult } from "@/components/Index"

export const userDefaultAvatar = "/defaultAvatar.svg"

export interface MaterialIndexItem{
    Id:number
    Name:string
    Src:string
    Desc:string
    Time:string
}

export function getMaterialItemsFromIndexResult(r:IndexResult):Array<MaterialIndexItem>{
    const idx_Id = r.ColumnNames.indexOf("Id")
    const idx_Name = r.ColumnNames.indexOf("Name")
    const idx_Src = r.ColumnNames.indexOf("Src")
    const idx_Desc = r.ColumnNames.indexOf("Desc")
    const idx_Time = r.ColumnNames.indexOf("Time")
    const res:MaterialIndexItem[] = []
    for (let rowIdx = 0; rowIdx < r.Data.length; rowIdx++) {
        const row = r.Data[rowIdx];
        if (row.length < 5) {
            continue;
        }
        res.push({
            Id: parseInt(row[idx_Id]),
            Name: row[idx_Name],
            Src: row[idx_Src],
            Desc: row[idx_Desc],
            Time: row[idx_Time]
        })
    }
    return res;
}