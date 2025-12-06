export interface GrammarHelpItem{
    title:string,
    desc:string,
    code?:string,
    demo?:string
}

export const grammarHelpsStandard:GrammarHelpItem[] = [
    {
        title:"加粗",
        desc:"加粗一般用于重点强调处，使用一对双星号(**)将加粗部分包围",
        code:"请**不要**把头手伸出窗外。",
        demo:"请<b>不要</b>把头手伸出窗外。"
    },
    {
        title:"斜体",
        desc:"斜体一般用于事物的名称或被解释的示例，使用一对星号(*)将斜体部分包围",
        code:"词语 *垃圾* 在此处指不再被引用的对象。",
        demo:"词语 <i>垃圾</i> 在此处指不再被引用的对象。"
    },
    {
        title:"删除线",
        desc:"删除线一般用于不再正确的陈述，使用一对双波浪号(~~)将删除线部分包围",
        code:"上海中心大厦是~~世界第二高楼~~世界第三高楼。",
        demo:"上海中心大厦是<s>世界第二高楼</s>世界第三高楼。"
    },
    {
        title:"链接",
        desc:"可点击跳转到其他页面，使用中括号([xxx])来创建链接",
        code:"百度的链接是[https://baidu.com]",
        demo:"百度的链接是<a href=\"https://baidu.com\">https://baidu.com</a>"
    },
    {
        title:"有文本的链接",
        desc:"可点击跳转到其他页面，使用\"[文字](链接)\"创建链接可设置显示文本",
        code:"遇到问题可以去[百度](https://baidu.com)搜一搜。",
        demo:"遇到问题可以去<a href=\"https://baidu.com\" target=\"_blank\">百度</a>搜一搜。"
    },
    {
        title:"次级标题",
        desc:"在行首使用一个或多个井号(#)表示嵌套的多层标题，层级越低井号越多",
        code:"# 今日菜单\n## 主食\n法式鹅肝、意大利面、战斧牛排\n## 甜点\n提拉米苏、冰淇淋",
        demo:"<h2>今日菜单</h2><div class=\"indent\">"+
        "<h3>主食</h3><div class=\"indent\"><p>法式鹅肝、意大利面、战斧牛排</p></div>"+
        "<h3>甜点</h3><div class=\"indent\"><p>提拉米苏、冰淇淋</p></div></div>"
    },
    {
        title:"列表",
        desc:"列表用于表示一系列并列的事务，使用行首的一个减号(-)表示",
        code:"可用的交通方式：\n- 地铁二号线\n- 608路公交车\n - 711路公交车\n请自行选择",
        demo:"可用的交通方式：<ul><li>地铁二号线</li><li>608路公交车</li><li>711路公交车</li></ul>请自行选择"
    },
    {
        title:"引用",
        desc:"引用他人的发言或作品，使用行首的一个大于号(>)表示",
        code:"不愿透露姓名的受访者表示：\n> 这是我吃过的最难吃的东西",
        demo:"不愿透露姓名的受访者表示：<div class=\"quote\"><p>这是我吃过的最难吃的东西</p></div>"
    },
    {
        title:"分隔线",
        desc:"使用三个减号(---)表示一条横向分隔线",
        code:"山河破碎风飘絮\n---\n身世浮沉雨打萍",
        demo:"<p>山河破碎风飘絮</p><div class=\"sep\"></div><p>身世浮沉雨打萍</p>"
    },
    {
        title:"表格",
        desc:"用于展示结构化的数据，使用竖杠(|)分隔单元格表示表格",
        code:"| 姓名 | 张三 | 李四 |\n| 年龄 | 23 | 22 |",
        demo:"<table><tbody><tr><td>姓名</td><td>张三</td><td>李四</td></tr><tr><td>年龄</td><td>23</td><td>22</td></tr></tbody></table>"
    },
    {
        title:"行内代码块",
        desc:"可用于专有名称或车次号、航班号等，将以等宽字体显示，使用两个顿音符(`)将代码块部分包围",
        code:"函数的第一个参数`username`表示用户名",
        demo:"函数的第一个参数<code>username</code>表示用户名"
    },
    {
        title:"脚注",
        desc:"用于编写注释或引用出处，使用\"[^xx]\"表示文中的脚注锚点，在单独一行使用同名的\"[^xx]:\"表示脚注的本体",
        code:"我推荐使用vue[^1]编写网站。\n[^1]:一种web前端框架",
        demo:"我推荐使用vue<sup><a class=\"refentry\">[1]</a></sup>编写网站。"+
        "<div class=\"refbodies\"><div><div><a class=\"ref\">[1]</a>一种web前端框架</div></div></div>"
    },
    {
        title:"html",
        desc:"可以直接编写html代码。<br/>如果需要学习请"+
        "<a href=\"https://developer.mozilla.org/zh-CN/docs/Web/HTML\" target=\"_blank\">查看教程</a>，"+
        "为安全考虑script标签和onxx事件注册属性会被过滤，需要使用js请咨询管理员",
        code:"<span style=\"color:red\">你好</span>，很<u>高兴</u>认识你！",
        demo:"<span style=\"color:red\">你好</span>，很<u>高兴</u>认识你！"
    },
]

export const grammarHelpsExtended:GrammarHelpItem[] = [
    {
        title:"标题默认折起",
        desc:"在标题前方添加 ^ 符号可将标题后的段落默认折起。",
    },
    {
        title:"标题隐藏(1)",
        desc:"将标题设为两个斜杠 // 可将该标题隐藏（仅限段落的大标题）。",
    },
    {
        title:"标题隐藏(2)",
        desc:"将标题设为三个斜杠 /// 可将该标题隐藏，并保留段落上方的空隙（仅限段落的大标题）。",
    },
    {
        title:"居中和靠右",
        desc:"行首一个英文句点和三个空格表示本行居中，行首三个英文句点和三个空格表示本行靠右。",
        code:"> 这是我吃过的最难吃的东西\n> ...   ——不愿透露姓名的受访者\n.   本台记者独家报道",
        demo:"<div class=\"quote\"><p>这是我吃过的最难吃的东西</p>"+
        "<div style=\"text-align:right\"><p>——不愿透露姓名的受访者</p></div></div>"+
        "<div style=\"text-align:center\"><p>本台记者独家报道</p></div>"
    },
    {
        title:"词条链接",
        desc:"本系统支持自动链接，也可手动创建通往本站其他词条的链接，使用[显示名称](链接名)",
        code:"请参见[1路](beijing-bus-1)词条",
        demo:"请参见<a href=\"#\">1路</a>词条"
    },
    {
        title:"嵌入素材",
        desc:"素材在本站指行内小图片，可用来表示国籍等。\n需先在素材管理页上传，然后使用大括号({xxx})引用。\n"+
        "如果需要控制图片尺寸，可写{xxx:3}（3倍行高）或{xxx:20px}来指定其高度",
        code:"欢迎使用fcloud3{fcloud3:4}内容管理系统。",
        demo:"欢迎使用fcloud3<img class=\"wikiInlineImg\" src=\"/fcloud.svg\" style=\"height:2rem\">内容管理系统。"
    },
    {
        title:"网络资源",
        desc:"如果有指向图片/视频/音频文件的链接，可写在中括号内，例如：\n[http://wiki.jowei19.com/fcloud.svg]\n\n"+
        "如果需要控制资源尺寸，可写<u>[xxx|3]</u>（3倍行高）或<u>[xxx|20px]</u>来指定其高度。\n"+
        "如果需要控制资源位置，可写<u>[xxx|3|left]</u>。注意中括号内三个部分之间的分隔符是竖杠（ | ）\n"+
        "（其中<u>left</u>表示靠左浮动，<u>right</u>表示靠右浮动(默认)，<u>block</u>表示占整行并居中，<u>inline</u>表示行内）",
        code:"各位女士们先生们，欢迎 [http://wiki.jowei19.com/fcloud.svg|100px|block] 欢迎使用fcloud3内容管理系统。",
        demo:"各位女士们先生们，欢迎<img src=\"/fcloud.svg\" style=\"display:block;height:100px;margin:auto\"/>欢迎使用fcloud3内容管理系统。"
    },
    {
        title:"上角标/下角标",
        desc:"使用^()和_()表示上下角标，注意使用英文括号。\n一般用于计量单位、简易的数学表达式和化学式等。",
        code:"18m^(2)，H_(2)SO_(4)",
        demo:"18m<sup>2</sup>，H<sub>2</sub>SO<sub>4</sub>",
    },
    {
        title: "注释行",
        desc: "行开头处添加//（左侧可以有空格）则该行会被忽略，不会显示在词条展示页。",
        code: "此处的动能为1.8MJ\n   //由E=mv^2算出\n十分恐怖",
        demo: "此处的动能为1.8MJ\n十分恐怖"
    },
    {
        title: "带标题的列表",
        desc: "若一个“#”标题下方只有一个“-”列表，那么它们将被一个框框起来。\n如果想阻止这个框出现，可在列表后添加一行空注释，即“//”",
        code: "# 早餐\n- 每人一个煮鸡蛋\n- 每人两个馒头\n- 无限量供应豆浆",
        demo: "<div class=\"titledList\"><div class=\"titledListTitle\">早餐</div><ul><li>每人一个煮鸡蛋</li><li>每人两个馒头</li><li>无限量供应豆浆</li></ul></div>"
    },
    {
        title:"自定义色字体/色块",
        desc:"可使用\"#颜色#\"创建指定颜色的色块，使用\"#颜色\\@文本#\"创建指定颜色的文字\n"+
        "具名颜色请参考<a href=\"https://developer.mozilla.org/zh-CN/docs/Web/CSS/named-color\" target=\"_blank\">此处</a>\n"+
        "或使用六位16进制rgb例如\"ff0000\"，或使用\"rgb(255,255,255)\"表示颜色",
        code:"大海和天空是#blue\\@蓝色#的\n#ff0000#一号线 \n#green#二号线",
        demo:"<p>大海和天空是<span class=\"coloredText\" style=\"color:blue\">蓝色</span>的</p>"+
        "<p><span class=\"coloredBlock\" style=\"color:red;background-color:ff0000\"></span>一号线</p>"+
        "<p><span class=\"coloredBlock\" style=\"color:red;background-color:green\"></span>二号线</p>"
    },
    {
        title:"自定义色背景单元格",
        desc:"可在单元格中写\"文字/-c-/颜色\"来创建任意背景颜色的单元格，字体颜色会根据背景颜色自动调整。本规则也可在表格段落中使用\n"+
        "具名颜色请参考<a href=\"https://developer.mozilla.org/zh-CN/docs/Web/CSS/named-color\" target=\"_blank\">此处</a>\n"+
        "或使用六位16进制rgb，例如ff0000，或使用\"rgb(255,255,255)\"表示颜色",
        code:"| 一号线/-c-/cornflowerblue | 132km | \n| 二号线/-c-/rgb(255,200,200) | 180km |",
        demo:"<table><tbody><tr><td style=\"background-color:cornflowerblue;color:white\">一号线</td><td>132km</td></tr>"+
        "<tr><td style=\"background-color:rgb(255,200,200);color:black\">二号线</td><td>180km</td></tr></tbody></table>"
    },
    {
        title:"带边框文字",
        desc:"一般用于逝者姓名，使用\\bd将文本包围",
        code:"制作人员名单：张三、\\bd李四\\bd，王五",
        demo:"制作人员名单：张三、<span style=\"padding:2px;border:1px solid black\">李四</span>、王五"
    },
    {
        title:"鼠标移入显示",
        desc:"一般用于整活",
        code:"她平时特别喜欢喝奶茶\\hd（别告诉她是我说的）\\hd",
        demo:"她平时特别喜欢喝奶茶<span class=\"hoverToDisplayDemo\">（别告诉她是我说的）</span>"
    }
]