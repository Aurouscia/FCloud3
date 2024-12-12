import { parseTargets } from './targetParser/targetParser'
import { renderTargetGroups } from './targetRenderer/targetRenderer'

export function run() {
    const targetGroups = parseTargets()
    renderTargetGroups(targetGroups)
}
