import { sleep } from '../../utils/sleep';
import { ref } from "vue";

const clickableTagName = "A";
const refIdStarts = "ref_";
const refentryIdStarts = "refentry_";

export function useFootNoteJump(){
    function listenFootNoteJump(){
        window.addEventListener("click",clickHandler)
    }
    function disposeFootNoteJump(){
        window.removeEventListener("click",clickHandler)
    }
    const footNoteJumpCallBack = ref<(top:number)=>void>();
    function clickHandler(e:MouseEvent){
        if(!footNoteJumpCallBack.value){return;}
        const ele = e.target as HTMLElement;
        const tagName = ele.tagName;
        if(tagName != clickableTagName || !ele.id){
            return;
        }
        let search:string;
        if(ele.id.startsWith(refentryIdStarts)){
            search = refIdStarts + ele.id.substring(refentryIdStarts.length);
        }else if(ele.id.startsWith(refIdStarts)){
            search = refentryIdStarts + ele.id.substring(refIdStarts.length);
        }else{
            return;
        }
        const target = document.querySelector("#"+search) as HTMLElement;
        if(target && target.offsetTop){
            footNoteJumpCallBack.value(target.offsetTop);
            window.setTimeout(async()=>{
                target.style.transition = "0s";
                for(const _a in [1,2,3]){
                    target.style.color = "white";
                    target.style.backgroundColor = "red";
                    await sleep(300);
                    target.style.color = "";
                    target.style.backgroundColor = "";
                    await sleep(300);
                }
            })
        }
    }
    return{listenFootNoteJump,disposeFootNoteJump,footNoteJumpCallBack}
}
