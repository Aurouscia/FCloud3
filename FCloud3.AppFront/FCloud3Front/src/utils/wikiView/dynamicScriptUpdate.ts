export function updateScript(element:HTMLDivElement, script:string, type?:"module"){
    element.innerHTML = "";
    const newChild = document.createElement("script");
    newChild.innerHTML = script;
    if(type)
        newChild.type = type;
    element.appendChild(newChild);
}