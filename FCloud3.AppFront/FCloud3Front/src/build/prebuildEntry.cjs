const markVersionModule = require('./version/markVersion.cjs')
const pluginSearchModule = require('./plugin/pluginSearch.cjs')

markVersionModule.markVersion()
pluginSearchModule.searchPlugins()