# 这是什么？
本目录内的.ts文件应该会在`npm run prebuild`时  
自动生成对应类型(并无类型检查，直接as的)的json文件，  
并在`npm run build`时被bundle进编译结果里(vite支持导入json为对象)。  
prebuild会自动在build前调用

说实话，我感觉这像是奇淫技巧，很怪，我自己瞎琢磨出来的办法