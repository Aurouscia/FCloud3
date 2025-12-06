export function paraTitleHiddenClass(paraTitle?:string){
    paraTitle = paraTitle?.trim()
    if(paraTitle === "//"){
        return "paraTitleHidden"
    }
    else if(paraTitle === "///"){
        return "paraTitleHiddenKeepingMargin"
    }
    return undefined
}