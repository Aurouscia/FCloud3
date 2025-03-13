export async function runJsFile(path: string, scriptContainer: HTMLDivElement): Promise<boolean> {
    scriptContainer.innerHTML = '';
    const scriptLabel = document.createElement('script');
    scriptLabel.type = 'module';
    scriptLabel.src = path;
    return new Promise((resolve) => {
        scriptLabel.addEventListener('load', () => {
            resolve(true);
        });
        scriptContainer.appendChild(scriptLabel);
    });
}
export async function runJs(code: string, scriptContainer: HTMLDivElement, codeImport?: string): Promise<boolean> {
    const rand = Math.random().toString(36).substring(7)
    return new Promise((resolve) => {
        const script = document.createElement('script');
        script.type = 'module';
        script.innerHTML = `
            ${codeImport}
            try {
                ${code} 
                window.dispatchEvent(new CustomEvent('scriptExecuted', { detail: { success: true, id:'${rand}' } }));
            }
            catch (error) {
                window.dispatchEvent(new CustomEvent('scriptExecuted', { detail: { success: false, id:'${rand}', error } }));     
            }
        `;
        scriptContainer.appendChild(script);

        const eventListener = (event:CustomEvent) => {
            if (event.type === 'scriptExecuted' && event.detail.id == rand) {
                if (event.detail.success) {
                    resolve(true);
                } else {
                    console.error(event.detail.error);
                    resolve(false);
                }
                window.removeEventListener('scriptExecuted', eventListener); // 移除监听器
            }
        };
        window.addEventListener('scriptExecuted', eventListener);
    });
}