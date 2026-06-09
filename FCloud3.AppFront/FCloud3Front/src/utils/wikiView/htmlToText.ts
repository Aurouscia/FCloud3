export function htmlToText(html: string): string {
    const div = document.createElement('div');
    div.innerHTML = html;
    return div.textContent || '';
}
