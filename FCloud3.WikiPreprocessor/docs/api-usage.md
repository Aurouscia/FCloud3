# API 使用指南

## 快速开始

```csharp
using FCloud3.WikiPreprocessor.Options;
using FCloud3.WikiPreprocessor.Mechanics;

// 创建解析器
var parser = new ParserBuilder().BuildParser();

// 解析为纯 HTML
string html = parser.RunToPlain("# 标题\n正文内容");

// 解析为结构化结果
var result = parser.RunToParserResult("# 标题\n正文内容");
// result.Content      - 主体 HTML
// result.PreScript    - 前置脚本
// result.PostScript   - 后置脚本
// result.Style        - 样式
// result.FootNotes    - 脚注 HTML

// 解析为原始结果（含规则使用记录和标题树）
var raw = parser.RunToParserResultRaw("# 标题\n正文内容");
// raw.Content             - 主体 HTML
// raw.UsedRules           - 使用的规则列表
// raw.FootNotes           - 脚注列表
// raw.Titles              - 标题树结构
// raw.InlineMediaQueries  - 行内媒体对象使用的媒体查询样式片段
```

## 配置解析器

### 使用 ParserBuilder

```csharp
var parser = new ParserBuilder()
    // 启用调试信息
    .EnableDebugInfo()
    
    // 启用标题收集
    .TitleGathering.Enable()
    
    // 启用缓存
    .Cache.EnableCache()
    
    // 自定义链接转换函数
    .Link.ReplaceConvertFn((linkItem, customText) =>
    {
        if (customText is not null)
            return $"<a pathName=\"{linkItem.Url}\">{customText}</a>";
        return $"<a pathName=\"{linkItem.Url}\">{linkItem.Text}</a>";
    })
    
    // 添加自定义块规则
    .Block.AddMoreRule(new PrefixBlockRule(">>", "<div class=\"nested-quote\">", "</div>", "嵌套引用"))
    
    // 添加自定义行内规则
    .Inline.AddMoreRule(new CustomInlineRule("%%", "%%", "<mark>", "</mark>", "高亮"))
    
    // 添加自动替换目标
    .AutoReplace.AddReplacingTargets(["词条A", "词条B"], isSingle: true)
    
    // 添加模板
    .Template.AddTemplates([
        new Template("卡片", "<div class=\"card\">[[[__内容__]]]</div>", 
            styles: ".card{border:1px solid #ccc;padding:10px}")
    ])
    
    // 使用自定义颜色解析器
    .UseColorParser(new ColorParser(...))
    
    // 使用定位哈希
    .UseLocatorHash(new MyLocatorHash())
    
    .BuildParser();
```

### 设置外部数据源

```csharp
parser.SetConvertingProvider(new MyConvertingProvider());

public class MyConvertingProvider : IScopedConvertingProvider
{
    public LinkItem? Link(string linkSpan)
    {
        // 根据 linkSpan 返回对应的链接信息
        return new LinkItem("显示文字", "/path");
    }

    public string? Implant(string implantSpan)
    {
        // 将 {implantSpan} 解析为嵌入内容
        return "<a href=\"/w/xxx\">xxx</a>";
    }

    public string? Replace(string replaceTarget)
    {
        // 返回自动替换目标的实际值
        return $"<a href=\"/w/{id}\">{replaceTarget}</a>";
    }
}
```

## 运行时操作

### 动态更换自动替换目标

```csharp
// 注册新的替换目标（可选是否清除旧目标）
parser.Context.AutoReplace.Register(
    targets: ["新词条A", "新词条B"],
    isSingleUse: true,
    clearOld: true
);

// 清除所有替换目标
parser.Context.AutoReplace.Clear();
```

### 获取解析元数据

```csharp
var result = parser.RunToParserResultRaw(input);

// 获取使用的规则（含样式/脚本的）
var usedRules = result.UsedRulesWithCommons;
var ruleNames = result.UsedRuleWithCommonsNames;

// 获取标题树
var titles = result.Titles;
foreach (var title in titles)
{
    Console.WriteLine($"Level {title.Level}: {title.Text} (ID: {title.Id})");
    if (title.Subs is not null)
    {
        foreach (var sub in title.Subs)
            Console.WriteLine($"  Level {sub.Level}: {sub.Text}");
    }
}

// 获取引用
var refs = parser.Context.Ref.Refs;

// 获取脚注
var footNotes = result.FootNotes;
```

## 性能与安全

### 输入限制

- 最大输入长度：`30000` 字符（`Parser.maxInputLength`）
- 超过限制的输入直接原样返回

### 深度保护

- 最大嵌套深度：`24` 层
- 超过深度返回错误元素

### 缓存

```csharp
// 启用缓存（适合重复解析相同内容）
var parser = new ParserBuilder()
    .Cache.EnableCache()
    .BuildParser();
```

注意：缓存与 `SingleUse` 自动替换不兼容。

## 扩展开发

### 自定义块规则

```csharp
public class MyBlockRule : BlockRule
{
    public MyBlockRule() : base("<div class=\"my\">", "</div>", name: "我的规则") { }

    public override bool LineMatched(string line) => line.StartsWith("!!");
    
    public override string GetPureContentOf(string line) => 
        LineMatched(line) ? line[2..].Trim() : line;
    
    public override IHtmlable MakeBlockFromLines(
        IEnumerable<LineAndHash> lines, IInlineParser inlineParser, 
        IRuledBlockParser blockParser, ParserContext context)
    {
        var content = blockParser.Run(lines.ToList());
        return new RuledBlockElement(content, this);
    }
    
    public override bool Equals(IBlockRule? other) => other is MyBlockRule;
    public override string UniqueName => "自定义_我的规则";
}
```

### 自定义行内规则

```csharp
var rule = new CustomInlineRule(
    markLeft: "(((",
    markRight: ")))",
    putLeft: "<span class=\"highlight\">",
    putRight: "</span>",
    name: "高亮",
    style: ".highlight{background-color:yellow}",
    maxLengthBetween: 100
);
```

### 自定义模板

```csharp
var template = new Template(
    name: "信息框",
    source: @"
        <div class=\"infobox\">
            <h3>[__标题__]</h3>
            <div>[[[__内容__]]]</div>
        </div>",
    styles: ".infobox{border:1px solid #ccc;padding:10px}",
    preScripts: "",
    postScripts: ""
);
```
