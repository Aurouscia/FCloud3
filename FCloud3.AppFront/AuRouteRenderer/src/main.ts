import './style.css'
import { run } from '../lib/auRouteRenderer'

const sample = '/sandbox/table1.html'
const resp = await fetch(sample)
const html = await resp.text()
document.querySelector("#app")!.innerHTML = html

run()