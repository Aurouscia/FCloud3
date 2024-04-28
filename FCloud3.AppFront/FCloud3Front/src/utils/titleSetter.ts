export function setTitleTo(title:string){
    document.title = defaultTitle + " - " + title;
}
export function recoverTitle(){
    document.title = defaultTitle;
}

export const defaultTitle = import.meta.env.VITE_TITLE;