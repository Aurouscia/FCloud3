export interface WikiCenteredHomePage
{
    LatestWikis:Array<Wiki>
    RandomWikis:Array<Wiki>
    TopDirs:Array<Pair>
}

export interface Wiki
{
    Path:string
    Title:string
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