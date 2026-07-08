export function createDrawerButton(text: string): HTMLButtonElement {
    const button = document.createElement('button')
    button.style.display = 'block'
    button.style.margin = '10px auto'
    button.textContent = text
    return button
}

interface Drawer {
    open: () => void
    close: () => void
}

let drawerIdCounter = 0

function createDrawer(table: HTMLTableElement, direction: 'left' | 'right', id: string): Drawer {
    const backdrop = document.createElement('div')
    backdrop.classList.add('au-table-float-backdrop')
    backdrop.dataset.auTableFloatDrawerId = id
    backdrop.style.position = 'fixed'
    backdrop.style.inset = '0'
    backdrop.style.backgroundColor = 'black'
    backdrop.style.opacity = '0'
    backdrop.style.transition = 'opacity 0.3s'
    backdrop.style.zIndex = '9998'
    backdrop.style.pointerEvents = 'none'

    const panel = document.createElement('div')
    panel.classList.add('au-table-float-panel')
    panel.classList.add('au-table-float-drawer')
    panel.dataset.auTableFloatDrawerId = id
    panel.style.position = 'fixed'
    panel.style.top = '0'
    panel.style.bottom = '0'
    panel.style.width = '80%'
    panel.style.maxWidth = '400px'
    panel.style.backgroundColor = 'white'
    panel.style.zIndex = '9999'
    panel.style.transition = 'transform 0.3s'
    panel.style.overflow = 'auto'

    if (direction === 'right') {
        panel.style.right = '0'
        panel.style.transform = 'translateX(100%)'
    }
    else {
        panel.style.left = '0'
        panel.style.transform = 'translateX(-100%)'
    }

    panel.appendChild(table)
    document.body.appendChild(backdrop)
    document.body.appendChild(panel)

    function open() {
        backdrop.style.pointerEvents = 'auto'
        backdrop.style.opacity = '0.4'
        panel.style.transform = 'translateX(0)'
    }

    function close() {
        backdrop.style.pointerEvents = 'none'
        backdrop.style.opacity = '0'
        panel.style.transform = direction === 'right' ? 'translateX(100%)' : 'translateX(-100%)'
    }

    backdrop.addEventListener('click', close)

    return { open, close }
}

export function applyDrawer(table: HTMLTableElement, direction: 'left' | 'right', buttonText: string): void {
    const id = `au-table-float-drawer-${++drawerIdCounter}`
    table.dataset.auTableFloatDrawerId = id

    const button = createDrawerButton(buttonText)
    button.dataset.auTableFloatDrawerId = id

    const parent = table.parentElement
    if (parent) {
        parent.insertBefore(button, table)
    }
    else {
        document.body.appendChild(button)
    }

    const drawer = createDrawer(table, direction, id)
    button.addEventListener('click', drawer.open)
}

export function removeDrawer(table: HTMLTableElement): void {
    const id = table.dataset.auTableFloatDrawerId
    if (!id) {
        return
    }
    delete table.dataset.auTableFloatDrawerId

    const selector = `[data-au-table-float-drawer-id="${id}"]`
    document.querySelector(`button${selector}`)?.remove()
    document.querySelector(`div${selector}.au-table-float-backdrop`)?.remove()
    document.querySelector(`div${selector}.au-table-float-panel`)?.remove()
}
