export interface DiffContentDetailResult{
    Items: DiffContentStepDisplay[]
}

export interface DiffContentStepDisplay
{
    Id: number
    From: DiffDisplayFrag[]
    To: DiffDisplayFrag[]
}
export interface DiffDisplayFrag
{
    Text: string
    High: [number,number][]
}