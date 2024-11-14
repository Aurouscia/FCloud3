export interface WikiCenteredHomePage
{
    LatestWikis:Array<WikiWithAvt>
    RandomWikis:Array<WikiWithAvt>
    TopDirs:Array<Pair>
}

export interface Wiki
{
    Path:string
    Title:string
    TimeInfo?:string|null
}
export interface WikiWithAvt extends Wiki
{
    Avt:string
}
export interface FileDir
{
    Path:string
    Name:string
}
export interface Pair{
    WPath:string
    WTitle:string
    DPath:string
    DName:string
}