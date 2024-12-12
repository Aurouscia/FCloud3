import { parseTargets } from './targetParser/targetParser'

export function run() {
    const targetGroups = parseTargets()
    console.log(targetGroups)
}
