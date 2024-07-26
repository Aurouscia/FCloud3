export interface DiffContentDetailResult{
    Items: DiffContentStepDisplay[]
}

export interface DiffContentStepDisplay
{
    Id: number
    Hidden:boolean
    From: DiffDisplayFrag[]
    To: DiffDisplayFrag[]
}
export interface DiffDisplayFrag
{
    Text: string
    High: [number,number][]
}