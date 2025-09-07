//应通过<script type="module">引用本文件

//运行特定插件（符合本项目插件约定的js脚本）
//关于插件，请参考前端README.md文件
async function runFCloud3Plugin(path){
    const module = await import(path)
    if(typeof module['run'] === 'function'){
        await module.run() //可能异步可能同步，await它就完事了
    }
}

//副作用代码：向window对象进行猴子补丁，添加上述函数
//最好不要猴子补丁，但vite不允许有它不能掌控的动态import代码(public和src里都不行)，只能用此下策
window.runFCloud3Plugin = runFCloud3Plugin