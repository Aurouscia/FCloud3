import { DefineProps, computed } from 'vue'
import { WikiParaDisplay, wikiParaDefaultFoldMark } from '@/models/wiki/wikiPara';

export function useParaListItem(props:DefineProps<{ w: WikiParaDisplay }, never>){
    const mainname = computed<string>(()=>{
        const nov = props.w.NameOverride;
        const title = props.w.Title;
        let name = nov || title;
        if (name && name.startsWith(wikiParaDefaultFoldMark)) {
            return name.substring(1);
        } else {
            return name || '未命名段落';
        }
    })
    const subname = computed<string>(()=>{
        const nov = props.w.NameOverride;
        const title = props.w.Title;
        let name = nov || title;
        if (name && name.startsWith(wikiParaDefaultFoldMark)) {
            return "(默认折起)";
        } else {
            return "";
        }
    })
    return {
        props,
        mainname,
        subname
    }
}