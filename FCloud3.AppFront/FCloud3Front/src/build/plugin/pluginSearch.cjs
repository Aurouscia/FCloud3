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
            const optionsFileName = dirContents.find(x=>x==='options.json')
            const docsFileName = dirContents.find(x=>x==='docs.html')
            if(entryFileName && optionsFileName){
              const name = item
              const entry = `/${pluginsDirName}/${item}/${entryFileName}`
              const optionsPath = path.join(itemPath, optionsFileName)
              const optionsContent = fs.readFileSync(optionsPath, 'utf-8')
              let options
              try{
                options = JSON.parse(optionsContent)
              }
              catch{
                console.warn(`插件[${item}]的options.json格式错误`)
                return
              }
              const triggers = options.triggers
              if(!Array.isArray(triggers) || triggers.length===0){
                console.warn(`插件[${item}]的options.json缺少triggers数组`)
                return
              }
              const priority = typeof options.priority === 'number' ? options.priority : 0
              const pluginObj = {
                name,
                entry,
                triggers,
                priority
              }
              if(typeof options.displayName === 'string'){
                pluginObj.displayName = options.displayName
              }
              if(typeof options.description === 'string'){
                pluginObj.description = options.description
              }
              if(docsFileName){
                pluginObj.docs = `/${pluginsDirName}/${item}/${docsFileName}`
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