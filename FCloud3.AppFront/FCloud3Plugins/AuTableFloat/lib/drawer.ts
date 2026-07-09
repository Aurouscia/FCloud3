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
    dispose: () => void
}

let drawerIdCounter = 0

// 当 window.location 发生变化时，需要自动关闭并销毁 drawer。
// SPA 中的路由跳转通常不会触发通用事件，因此这里统一拦截 history.pushState /
// history.replaceState，并监听 popstate / hashchange。
// 每个 drawer 创建时都会把自己的 dispose 回调注册到 locationChangeHandlers 中，
// 地址变化时统一通知所有 drawer 执行销毁。
const locationChangeHandlers = new Set<() => void>()
let historyPatched = false

function notifyLocationChanged() {
    locationChangeHandlers.forEach((handler) => handler())
}

function ensureHistoryPatch() {
    if (historyPatched) {
        return
    }
    historyPatched = true

    // 全局 history 只补丁一次：第一个 drawer 创建时注入拦截，
    // 第一次地址变化并通知完所有 drawer 后，再自动还原原始方法与事件监听，
    // 避免 drawer 全部销毁后仍永久占用全局 API。
    const originalPushState = history.pushState.bind(history)
    const originalReplaceState = history.replaceState.bind(history)

    history.pushState = function (
        ...args: [data: unknown, unused: string, url?: string | URL | null]
    ) {
        originalPushState.apply(this, args)
        notifyLocationChanged()
    }
    history.replaceState = function (
        ...args: [data: unknown, unused: string, url?: string | URL | null]
    ) {
        originalReplaceState.apply(this, args)
        notifyLocationChanged()
    }

    const onPopState = () => notifyLocationChanged()
    const onHashChange = () => notifyLocationChanged()
    window.addEventListener('popstate', onPopState)
    window.addEventListener('hashchange', onHashChange)

    // 第一次地址变化后，在所有 drawer 的 handler 执行完毕时还原全局环境。
    locationChangeHandlers.add(() => {
        window.removeEventListener('popstate', onPopState)
        window.removeEventListener('hashchange', onHashChange)
        history.pushState = originalPushState
        history.replaceState = originalReplaceState
        historyPatched = false
    })
}

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

    const cleanups: (() => void)[] = []
    let disposed = false

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

    // 销毁当前 drawer：先调用 close() 启动 0.3s 的滑出/淡出过渡动画，
    // 等待 300ms 动画完全结束后，再调用 removeDrawer() 把按钮、遮罩、
    // 面板以及被移入其中的 table 一并从 DOM 中移除，并清理已注册的事件回调。
    function dispose() {
        if (disposed) {
            return
        }
        disposed = true
        close()
        cleanups.forEach((cleanup) => cleanup())
        cleanups.length = 0
        setTimeout(() => removeDrawer(table), 300)
    }

    backdrop.addEventListener('click', close)

    // 注册当前 drawer 的销毁回调，地址变化时自动 dispose。
    ensureHistoryPatch()
    const locationChangeHandler = () => dispose()
    locationChangeHandlers.add(locationChangeHandler)
    cleanups.push(() => locationChangeHandlers.delete(locationChangeHandler))

    return { open, close, dispose }
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
