export class WikiSourceHighlighter {
    t:Text|undefined = undefined;
    ranges:[number,number][] = [];

    run(t:Text){
        CSS.highlights.clear();
        this.ranges = []
        this.t = t;
        this.matchAndMake(/\*\*.+?\*\*/g, "bold");
        this.matchAndMake(/\*.+?\*/g, "italic");
        this.matchAndMake(/~~.+~~/g, "lineThrough")
        this.matchAndMake(/(?<=(^|\n))# .*(?=($|\n))/g, "subtitle-1");
        this.matchAndMake(/(?<=(^|\n))## .*(?=($|\n))/g, "subtitle-2");
        this.matchAndMake(/(?<=(^|\n))###+ .*(?=($|\n))/g, "subtitle-3");
        this.matchAndMake(/(?<=(^|\n))\[\^.+\].+/g, "footnoteBody");
        this.matchAndMake(/\[\^.+?\]/g, "footnoteEntry");
        this.matchAndMake(/(?<=(^|\n))> /g, "quote")
        this.matchAndMake(/(?<=(^|\n))\- /g, "list")
        this.matchAndMake(/(?<!\{)\{[a-zA-Z0-9\u4e00-\u9fa5:]{2,16}\}(?!\})/g, "implant")
        this.matchAndMake(/#.{3,}#/g, "color")
        this.matchAndMake(/(?<=(^|\n))\-{3,}(?=($|\n))/g, "sep")
        this.matchAndMake(/\{\{[a-zA-Z0-9\u4e00-\u9fa5]{2,10}\}(.|\n)*\}/g, "template")
        this.matchAndMake(/\[.+?\](?=(\())/g, "anchorText")
        this.matchAndMake(/(?<=\])\(.+?\)/g, "anchorLink")
        this.matchAndMake(/\[http.+?\]/g, "link")
    }
    private overlapped(start:number, end:number){
        const res = this.ranges.some(r=>
            (start > r[0] && start < r[1]) || (end > r[0] && end < r[1])
        )
        return res;
    }
    private matchAndMake(regex:RegExp, highlightName:string){
        if(!this.t || !this.t.textContent)
            return;
        const matches = this.t.textContent.matchAll(regex);
        if(matches){
            const rangesHere:Range[] = [];
            for(const match of matches){
                const start = match.index;
                const end = match.index + match[0].length;
                if(this.overlapped(start, end))
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