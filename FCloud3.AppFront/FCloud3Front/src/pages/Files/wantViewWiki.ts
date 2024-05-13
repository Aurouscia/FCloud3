let wantViewWiki:string|undefined;

export function setWantViewWiki(w:string){
    wantViewWiki = w;
}
export function getWantViewWiki(){
    const w = wantViewWiki;
    wantViewWiki = undefined;
    return w;
}