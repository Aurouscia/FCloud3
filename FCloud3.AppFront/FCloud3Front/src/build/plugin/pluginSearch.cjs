const fs = require('fs')
const path = require('path')

module.exports = {
    searchPlugins:function(){
        const pluginsDirName = 'plugins'
        const pluginsPath = `public/${pluginsDirName}`
        const resPath = `src/build/plugin/pluginsFound.js`
        const found = {}
        const items = fs.readdirSync(pluginsPath)
        items.forEach(item => {
          const itemPath = path.join(pluginsPath, item)
          const itemStats = fs.statSync(itemPath)
          if (itemStats.isDirectory()) {
            const entryFile = fs.readdirSync(itemPath)[0]
            if(entryFile && entryFile.endsWith('js')){
                const value = `/${pluginsDirName}/${item}/${entryFile}`;
                found[item] = value;
            }
          }
        })
        let code = 'export const pluginsFound = {\n'
        for(const fkey of Object.keys(found)){
            code += `  ${fkey} : "${found[fkey]}"\n`
        }
        code += '}\n'
        fs.writeFileSync(resPath, code)
    }
}