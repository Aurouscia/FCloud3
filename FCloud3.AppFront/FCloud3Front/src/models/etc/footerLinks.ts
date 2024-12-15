export interface FooterLinks
{
    linksLeft: FooterLink[]
    linksRight: FooterLink[]
}
export interface FooterLink
{
    text: string;
    url: string|null|undefined;
}
export function parseFooterLinks(str:string):FooterLinks{
    const rows = str.trim().split('\n')
    let writingLeft = true
    let linksLeft:FooterLink[] = []
    let linksRight:FooterLink[] = []
    rows.forEach(r=>{
        r = r.trim()
        if(r==='====='){
            writingLeft = false
            return
        }
        const parts = r.split('=')
        if(parts.length<2 || parts.some(x=>!x))
            return
        const text = parts[0].trim()
        const url = parts[1].trim()
        const link = {text, url}
        if(writingLeft)
            linksLeft.push(link)
        else
            linksRight.push(link)
    })
    return {
        linksLeft,
        linksRight
    }
}