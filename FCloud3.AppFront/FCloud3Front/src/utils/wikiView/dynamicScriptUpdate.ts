export function updateScript(element:HTMLDivElement, script:string){
    element.innerHTML = "";
    const newChild = document.createElement("script");
    newChild.innerHTML = script;
    element.appendChild(newChild);
}