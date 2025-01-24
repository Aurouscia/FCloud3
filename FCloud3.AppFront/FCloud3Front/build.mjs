import { execa } from 'execa'

const execaOptions = {stdout:'inherit', reject:false}

const firstParam = process.argv[2]
const buildHere = firstParam?.toLowerCase() === '--here'
const viteBuildArgs = buildHere ? ['--outDir', './dist'] : []

//prebuild
await execa("node", ["src/build/prebuildEntry.cjs"], execaOptions)

//vite和vue-tsc并行
const viteBuild = execa("vite build", viteBuildArgs, execaOptions);
const vueTsc = execa("vue-tsc", execaOptions)
await Promise.all([vueTsc, viteBuild])