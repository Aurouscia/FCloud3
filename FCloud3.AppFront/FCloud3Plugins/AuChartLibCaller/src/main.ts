import { run } from '../lib/auChartLibCaller'

const sample = '/sandbox.html'
const resp = await fetch(sample)
const html = await resp.text()
document.querySelector("#app")!.innerHTML = html

run()
