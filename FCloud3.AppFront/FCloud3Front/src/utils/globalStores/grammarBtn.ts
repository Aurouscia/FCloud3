import { defineStore } from "pinia";
import { ref } from "vue";

export type Grammar = 'italic'|'bold'|'delete'|'subtitle'
const grammars:Grammar[] = ['bold', 'italic', 'delete', 'subtitle']

export const useGrammarBtnStore = defineStore('grammarBtn',()=>{
    let editableTargets:HTMLElement[] = []
    let editedCallback:()=>void = ()=>{}
    const selectedGrammars = ref<{type:Grammar,borL:number,borR:number}[]>([])
    function setGrammarBtnTarget(...targets:HTMLElement[]){
        editableTargets = targets
    }
    function setEditedCallback(cb?:()=>void){
        if(!cb)
            cb = ()=>{}
        editedCallback = cb
    }
    function distinguishGrammar(){
        selectedGrammars.value = []
        const sel = window.getSelection();
        const focusNode = sel?.focusNode
        const focusText = sel?.focusNode?.textContent
        const focusEle = sel?.focusNode?.parentElement
        if(!focusNode || !focusText || !focusEle || !editableTargets.some(t=>t === focusEle)){
            return;
        }
        const rg = sel.getRangeAt(0)
        if(!rg){
            return;
        }
        const { startOffset:s, endOffset:e } = rg
        for(const type of grammars){
            if(markClosureGrammars.includes(type)){
                //如果是成对标记行内规则
                const mark = markClosureMark(type)
                const markLen = mark.length;
                let leftMatched = false;
                let rightMatched = false;
                let borL = s;
                while(s - borL <= markLen){
                    let area = focusText.substring(borL, e)
                    if(area.startsWith(mark)){
                        leftMatched = true
                        break;
                    }
                    borL--;
                }
                let borR = e;
                while(borR - e <= markLen){
                    let area = focusText.substring(s, borR)
                    if(area.endsWith(mark)){
                        rightMatched = true
                        break;
                    }
                    borR++
                }
                if(leftMatched && rightMatched){
                    const area = focusText.substring(borL, borR)
                    if(!area.includes('\n')){
                        //不能有换行
                        selectedGrammars.value.push({type, borL, borR})
                        break;
                    }
                }
            }
        }
    }
    function clickGrammarBtn(type:Grammar){
        const sel = window.getSelection();
        const focusNode = sel?.focusNode
        const focusText = sel?.focusNode?.textContent
        const focusEle = sel?.focusNode?.parentElement
        if(!focusNode || !focusText || !focusEle || !editableTargets.some(t=>t === focusEle)){
            return;
        }
        const rg = sel.getRangeAt(0)
        if(!rg){
            return;
        }
        const { startOffset:s, endOffset:e } = rg
        if(markClosureGrammars.includes(type)){
            //如果是成对标记行内规则
            const mark = markClosureMark(type)
            const markLen = mark.length
            const selected = selectedGrammars.value.find(g=>g.type==type)
            if(selected){
                const { borL, borR } = selected
                const left = focusText.substring(0, borL)
                const middle = focusText.substring(borL + markLen, borR - markLen)
                const right = focusText.substring(borR)
                focusNode.textContent = left+middle+right;
                setRange(focusNode, borL, borR-markLen*2)
                editedCallback()
            }else{
                const left = focusText.substring(0, s)
                const middle = focusText.substring(s, e)
                if(middle.includes('\n')){
                    //不能有换行
                    return;
                }
                const right = focusText.substring(e)
                focusNode.textContent = left+mark+middle+mark+right;
                editedCallback()
                setRange(focusNode, s, e+markLen*2)
            }
        }
        distinguishGrammar()
        return;
    }

    const markClosureGrammars:Grammar[] = ['bold','italic','delete']
    function markClosureMark(type:Grammar){
        if(type=='bold')
            return '**'
        if(type=='italic')
            return '*'
        if(type=='delete')
            return '~~'
        return ''
    }

    function setRange(node:Node, s:number, e:number){
        const range = document.createRange()
        range.setStart(node, s)
        range.setEnd(node, e)
        window.getSelection()?.removeAllRanges()
        window.getSelection()?.addRange(range)
        distinguishGrammar()
    }

    return { setGrammarBtnTarget, setEditedCallback, clickGrammarBtn, distinguishGrammar, selectedGrammars }
})