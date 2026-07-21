export function paraTitleHiddenClass(paraTitle?:string){
    paraTitle = paraTitle?.trim()
    if(paraTitle?.startsWith("///")){
        return "paraTitleHiddenKeepingMargin"
    }
    else if(paraTitle?.startsWith("//")){
        return "paraTitleHidden"
    }
    return undefined
}