export function split(input:string, trim:boolean=true, removeEmptyLine:boolean=true){
    var lines:Array<string> = [];

    if(input.indexOf(tplt_L)==-1&&input.indexOf(tplt_R)==-1){
        lines = input.split(lineSep)
    }
    else{
        var layer = 0;
        var start = 0;
        var length = 0;
        for(var i=0;i<input.length;i++)
        {
            const c = input[i];
            if (c == tplt_L){
                layer++;
            }
            if (c == tplt_R){
                layer--;
            }
            if (c == lineSep && layer == 0)
            {
                lines.push(input.substring(start,start+length));
                start += length+1;
                length = 0;
            }
            else{
                length++;
            }
        }
        lines.push(input.substring(start));
    }
    
    if(trim){
        lines.forEach((x,i)=>{
            lines[i] = x.trim();
        });
    }
    if(removeEmptyLine){
        lines = lines.filter(x=>x);
    }
    return lines;
}

export interface LineAndHash{
    text:string,
    hash:string,
    indexStart:number,
    indextEnd:number
}

export const tplt_L:string = "{";
export const tplt_R:string = "}";
export const lineSep = '\n';