# 这是什么？
本目录应该会在`npm run prebuild`时  
自动生成一个对应pluginsFound.ts的类型的pluginsFound.json文件，  
并在`npm run build`时被bundle进编译结果里(vite支持导入json为对象)。  
prebuild会自动在build前调用  
**注意** 这步并非可选，必须执行，否则将会造成build失败