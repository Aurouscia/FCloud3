# 架构设计

## 整体结构

```
Parser (入口)
├── BlockParser (块级解析)
│   ├── TitledBlockParser (标题解析)
│   └── RuledBlockParser (规则块解析)
│       └── InlineParser (行内解析)
│           └── TemplateParser (模板解析)
└── ParserContext (解析上下文)
    ├── RuleUsage (规则使用记录)
    ├── FootNote (脚注收集)
    ├── TitleGathering (标题收集)
    ├── Ref (引用收集)
    ├── AutoReplace (自动替换)
    ├── Caches (解析缓存)
    └── ConvertingProvider (外部数据源)
```

## 解析流程

输入文本 → `LineSplitter` 分行 → `BlockParser` 块解析 → `InlineParser` 行内解析 → `TemplateParser` 模板解析 → HTML 输出

### 1. 分行 (LineSplitter)

- 以 `\n` 为分隔符将输入拆分为行
- **模板调用的大括号 `{}` 内部不触发换行**，确保模板参数中的换行被保留
- 每行经过 `HtmlSanitizer` 进行 HTML 编码，防止 XSS
- 若配置了 `ILocatorHash`，计算每行的哈希值用于定位

### 2. 块级解析 (BlockParser)

#### 2.1 标题解析 (TitledBlockParser)

- 检测行首的 `#` 标记，支持 `#` ~ `#########`（1~9 级）
- 标题行会将其后的内容纳入自己的"势力范围"
- 标题内容本身会经过 `InlineParser` 解析（支持行内语法）
- 输出 `<hN>` 标签 + `<div class="indent">` 包裹的内容
- **特殊处理**：若标题的势力范围仅包含一个列表（`ul`），则输出为 `<div class="titledList">` 结构，而非 `hx` 标签
- 标题等级可通过 `TitleLevelOffset` 进行偏移调整

#### 2.2 规则块解析 (RuledBlockParser)

- 对每行检测是否匹配某个 `IBlockRule`
- 相邻的同规则行合并为一个块，由该规则的 `MakeBlockFromLines` 处理
- 无规则标记的行使用 `EmptyBlockRule`，逐行解析为 `<p>` 段落

### 3. 行内解析 (InlineParser)

#### 3.1 模板调用检测

- 先检测是否包含模板调用语法 `{{模板名}参数}`
- 若包含，交给 `TemplateParser` 处理
- 模板调用检测关闭时（`mayContainTemplateCall: false`），跳过此步骤

#### 3.2 行内规则标记 (MakeMarks)

- 遍历所有 `IInlineRule`，在文本中查找左标记 `MarkLeft` 和右标记 `MarkRight`
- 支持转义：`\` 可转义 `[` `]` `*` `|` `-` `~` `#`
- 标记之间不能重叠，先匹配到的标记占据位置
- 支持 `MaxLengthBetween` 限制标记间最大长度
- 支持 `IsSingleUse` 一次性规则（如自动替换）

#### 3.3 递归拆分 (SplitByMarks)

- 找到最左侧的有效标记，将文本拆为：左文本 + 标记内容 + 右文本
- 标记内容交给对应规则的 `MakeElementFromSpan` 处理
- 左右文本递归继续解析

### 4. 模板解析 (TemplateParser)

#### 4.1 调用拆分

- 按大括号 `{}` 的层级拆分文本为片段
- 片段类型：
  - `Plain`：普通文本
  - `Template`：模板调用 `{{模板名}参数}`
  - `Implant`：植入调用 `{内容}`

#### 4.2 模板调用解析

- 格式：`{{模板名}参数列表}`
- 参数分隔：`&&`
- 键值分隔：`::` 或 `：：`
- 若只有一个参数且无名，则自动匹配第一个插槽

#### 4.3 模板插槽类型

| 插槽类型 | 语法 | 说明 |
|----------|------|------|
| 纯文本插槽 | `[__名称__]` | 内容不解析，原样插入 |
| 行内解析插槽 | `[[__名称__]]` | 内容仅做行内解析 |
| 块解析插槽 | `[[[__名称__]]]` | 内容做完整块级解析 |
| 唯一ID插槽 | `[__%名称%__]` | 自动填入唯一标识符 |

### 5. 输出组装

解析完成后，根据 `Parser` 的不同方法组装输出：

- `RunToPlain`：内容 + 公共样式/脚本/脚注
- `RunToParserResult`：分离为 Content / PreScript / PostScript / Style / FootNotes
- `RunToParserResultRaw`：额外返回使用的规则列表和标题树

## 深度保护

- 解析器设有递归深度限制（默认最大 24 层）
- 超过限制返回 `ErrorElement`（显示"规则嵌套层数过多"）
- 防止模板嵌套、块规则嵌套等导致的栈溢出

## 缓存机制

- 通过 `CacheOptions` 启用
- 缓存行内解析结果（不含模板调用的场景）
- 缓存块解析结果
- 每次解析前后通过 `BeforeParsing` / `AfterParsing` 管理缓存生命周期

## 外部数据源 (IScopedConvertingProvider)

解析器通过此接口与外部系统交互：

| 方法 | 用途 |
|------|------|
| `Link(string)` | 将 `[xxx]` 或 `[文字](xxx)` 中的 `xxx` 解析为链接 |
| `Implant(string)` | 将 `{xxx}` 中的 `xxx` 解析为嵌入内容 |
| `Replace(string)` | 自动替换目标的实际值 |

## 核心类图

```
IHtmlable (接口)
├── Element (抽象基类)
│   ├── InlineElement
│   │   ├── TextElement
│   │   ├── RuledInlineElement
│   │   ├── TextConvertedElement
│   │   ├── AnchorElement
│   │   ├── InlineObjectElement
│   │   ├── FootNoteEntryElement
│   │   └── CachedElement
│   └── BlockElement
│       ├── TitledBlockElement
│       ├── RuledBlockElement
│       ├── SimpleBlockElement
│       ├── FootNoteBodyElement
│       └── FootNoteBodyPlaceholderElement
├── ElementCollection (列表)
└── TemplateElement

IRule (接口)
├── IBlockRule
│   ├── BlockRule (抽象)
│   │   ├── EmptyBlockRule
│   │   ├── PrefixBlockRule
│   │   │   └── ListBlockRule
│   │   ├── SepBlockRule
│   │   ├── MiniTableBlockRule
│   │   └── FootNoteBodyRule
│   └── LineCommentRule
└── IInlineRule
    ├── InlineRule (抽象)
    │   ├── CustomInlineRule
    │   ├── LiteralInlineRule
    │   ├── FootNoteAnchorRule
    │   ├── ManualAnchorRule
    │   ├── ManualTextedAnchorRule
    │   └── InlineObjectRule
    ├── RelyInlineRule
    └── ColorTextRule

Template : IRule
```
