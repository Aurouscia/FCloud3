import { useRouter } from "vue-router";

export function useWikiParsingRoutesJump(){
    const router = useRouter();
    const jumpToViewWiki = (wikiPathName:string|undefined)=>{
        if(!wikiPathName){
            return;
        }
        if(router){
            router.push({name:"viewWiki",params:{wikiPathName}});
        }
    }
    const jumpToWikiTemplateList = () => {
        if(router){
            router.push({name:"wikiTemplateList"});
        }
    }
    const jumpToWikiTemplateEditor = (id:number) => {
        if(router){
            router.push({name:"wikiTemplateEditor",params:{id}});
        }
    }
    return { jumpToViewWiki, jumpToWikiTemplateEditor, jumpToWikiTemplateList }
}