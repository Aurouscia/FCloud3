import { isFireFox } from "../browserInfo";

export class WikiSourceHighlighter {
    t:Text|undefined = undefined;
    tContent:string|undefined = undefined;
    ranges:[number,number][] = [];
    private styleTagPattern = new RegExp("<style>(.|\n)*?</style>", "g");

    run(t:Text){
        if(isFireFox)
            return;
        CSS.highlights.clear();
        this.ranges = []
        this.t = t;
        this.tContent = t.textContent || "";
        this.tContent = this.tContent?.replace(/\\\*/g, "zz")

        this.matchAndMake(/\*\*.+?\*\*/g, "bold");
        this.matchAndMake(/(?<!\*)\*[^\*]+?\*(?!\*)/g, "italic");
        this.matchAndMake(/~~.+?~~/g, "lineThrough")
        this.matchAndMake(/(?<=(^|\n))#+ .*(?=($|\n))/g, "subtitle");
        this.matchAndMake(/(?<=(^|\n))\[\^.+\].+/g, "footnoteBody");
        this.matchAndMake(/\[\^.+?\]/g, "footnoteEntry");
        this.matchAndMake(/(?<=(^|\n))> /g, "quote")
        this.matchAndMake(/(?<=(^|\n))\- /g, "list")
        this.matchAndMake(this.styleTagPattern, "styleTag")
        this.matchAndMake(/(?<!\{)\{[a-zA-Z0-9\u4e00-\u9fa5:\.]{2,16}\}(?!\})/g, "implant")
        this.matchAndMake(/#.{3,}?(?<!\\)#/g, "color")
        this.matchAndMake(/(?<=(^|\n))\-{3,}(?=($|\n))/g, "sep")
        this.matchAndMake(/\{\{[a-zA-Z0-9\u4e00-\u9fa5]{2,10}\}(.|\n)*?\}/g, "template")
        this.matchAndMake(/\[.+?\](?=(\())/g, "anchorText")
        this.matchAndMake(/(?<=\])\(.+?\)/g, "anchorLink")
        this.matchAndMake(/\[http.+?\]/g, "link")
        this.matchAndMake(/[\^_]\(.+?\)/g, "subsupscript")
        this.matchAndMake(/`.+?`/g, "inlineCode")
    }
    private overlapped(start:number, end:number){
        const res = this.ranges.some(r=>
            (start > r[0] && start < r[1]) || (end > r[0] && end < r[1])
        )
        return res;
    }
    private matchAndMake(regex:RegExp, highlightName:string){
        if(!this.t || !this.tContent)
            return;
        const matches = this.tContent.matchAll(regex);
        if(matches){
            const rangesHere:Range[] = [];
            for(const match of matches){
                const start = match.index;
                const end = match.index + match[0].length;
                if(this.overlapped(start, end))
                    continue;
                if(start>=1 && this.tContent[start-1]=='\\')
                    continue;
                const range = document.createRange();
                range.setStart(this.t, start);
                range.setEnd(this.t, end);
                rangesHere.push(range);
                this.ranges.push([start, end]);
            }
            CSS.highlights.set(highlightName, new Highlight(...rangesHere));
        }
    }
}