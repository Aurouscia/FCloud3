export async function runJsFile(path:string, scriptContainer:HTMLDivElement){
    scriptContainer.innerHTML = ''
    const scriptLabel = document.createElement('script')
    scriptLabel.type = 'module'
    scriptLabel.src = path;
    scriptContainer.appendChild(scriptLabel);
    return true
}
export async function runJs(code:string, scriptContainer:HTMLDivElement) {
    scriptContainer.innerHTML = ''
    const scriptLabel = document.createElement('script')
    scriptLabel.type = 'module'
    scriptContainer.appendChild(scriptLabel);
    scriptLabel.innerHTML = code;
}