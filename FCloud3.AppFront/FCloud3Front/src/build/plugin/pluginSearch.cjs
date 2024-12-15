const fs = require('fs')
const path = require('path')

module.exports = {
    searchPlugins:function(){
        const pluginsDirName = 'plugins'
        const pluginsPath = `public/${pluginsDirName}`
        const resPath = `src/build/plugin/pluginsFound.json`
        const found = []
        let items;
        try{
          items = fs.readdirSync(pluginsPath)
        }
        catch{
          items = []
        }
        items.forEach(item => {
          const itemPath = path.join(pluginsPath, item)
          const itemStats = fs.statSync(itemPath)
          if (itemStats.isDirectory()) {
            const dirContents = fs.readdirSync(itemPath)
            const entryFileName = dirContents.find(x=>x.endsWith('.entry.js'))
            const triggerFileName = dirContents.find(x=>x==='trigger.txt')
            if(entryFileName && triggerFileName){
              const name = item
              const entry = `/${pluginsDirName}/${item}/${entryFileName}`
              const triggerPath = path.join(itemPath, triggerFileName)
              const trigger = fs.readFileSync(triggerPath, 'utf-8')
              const pluginObj = {
                name,
                entry,
                trigger
              }
              found.push(pluginObj)
            }
            else{
              console.warn(`插件[${item}]未遵守约定`)
            }
          }
        })

        fs.writeFileSync(resPath, JSON.stringify(found, null, 2))
        console.log(`[fcloud3]注册 ${found.length} 个插件`)
    }
}