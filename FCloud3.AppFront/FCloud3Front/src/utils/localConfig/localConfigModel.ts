export interface LocalConfigModel{
    key:LocalConfigType
    version:string
}
export type LocalConfigType = "textSection"|"auth"|"wikiContentEdit"|"wikiCenteredHomePage"