import { defineStore } from "pinia";
import { ref } from "vue";

export const useLocalStylesStore = defineStore('localStyles', ()=>{
    const css = ref<string>()
    const fontSizeRem = ref<number>(0)
    const lineHeightRem = ref<number>(0)
    const color = ref<string>()
    const fontFamily = ref<string>()

    const elId = 'localStyles'
    function applyStyles(){
        let el = document.getElementById(elId)
        if(el){
            el.remove()
        }
        el = document.createElement('style')
        document.body.appendChild(el)
        el.innerHTML = getStyleString()
    }
    function getStyleString(){
        const res:string[] = []
        res.push('.wikiView .indent * {')
        if(fontSizeRem.value)
            res.push(`font-size: ${fontSizeRem.value}rem;`)
        if(lineHeightRem.value)
            res.push(`line-height: ${lineHeightRem.value}rem;`)
        if(color.value)
            res.push(`color: ${color.value};`)
        if(fontFamily.value)
            res.push(`font-family: ${fontFamily.value};`)
        res.push('}')
        if(css.value)
            res.push(css.value)
        return res.join('\n')
    }
    return { 
        css,
        fontSizeRem, 
        lineHeightRem,
        color,
        fontFamily,
        applyStyles
    }
}, {
    persist: {
        key: 'fcloud3-localStyles'
    }
})