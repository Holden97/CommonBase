# CSV/Excel 转 ScriptableObject 工具说明

## 功能概述

本工具支持将 Excel（.xlsx, .xls）和 CSV（.csv）文件转换为 Unity ScriptableObject。

## 文件格式要求

### 数据表结构（适用于 Excel 和 CSV）

| 行号 | 内容 | 说明 | 示例 |
|------|------|------|------|
| 第1行 | 配置信息 | 格式：`type=类型名称\|base=基类名称` | `type=ItemData\|base=` |
| 第2行 | 字段注释 | 中文注释，用于生成代码注释 | `物品ID,物品名称,物品价格` |
| 第3行 | 字段名称 | 英文字段名 | `itemId,itemName,price` |
| 第4行 | 字段类型 | C# 类型名称 | `int,string,float` |
| 第5行起 | 数据行 | 实际数据 | `1001,生命药水,50.5` |

### 支持的数据类型

- **基础类型**：`int`, `float`, `double`, `string`, `bool`
- **列表类型**：`List<int>`, `List<string>`
- **Unity 类型**：`Sprite`, `GameObject`, `AudioClip`

### 特殊说明

1. **bool 类型**：使用 `1` 表示 `true`，`0` 表示 `false`
2. **List 类型**：使用分号 `;` 分隔多个值
   - 示例：`10;20;30` → `List<int> { 10, 20, 30 }`
3. **Unity 资源类型**：填写资源的 Assets 路径
   - 示例：`Assets/Sprites/Icon.png`

## CSV 文件示例

### 简单示例（ExampleData.csv）

```csv
type=ExampleData|base=,,,
数据ID,数据名称,数据值,描述信息
id,name,value,description
int,string,float,string
1,示例数据1,100.5,这是第一条示例数据
2,示例数据2,200.8,这是第二条示例数据
3,示例数据3,300.3,这是第三条示例数据
```

### 复杂示例（ComplexExampleData.csv）

```csv
type=ItemData|base=,,,,,,,
物品ID,物品名称,物品价格,是否可堆叠,最大堆叠数,物品等级,标签列表,属性加成列表
itemId,itemName,price,stackable,maxStack,level,tags,bonuses
int,string,float,bool,int,int,List<string>,List<int>
1001,生命药水,50.5,1,99,1,药水;消耗品,10;0;0
1002,魔法药水,75.8,1,99,1,药水;消耗品,0;20;0
1003,钢铁剑,500.0,0,1,5,武器;近战,50;0;10
```

## 使用步骤

1. **准备数据文件**
   - 创建 Excel 或 CSV 文件
   - 按照上述格式填写数据

2. **打开工具窗口**
   - Unity 菜单：`Tools > 数据转换` (快捷键：Ctrl+T)

3. **配置命名空间**
   - 在工具窗口中设置"命名空间名称"

4. **选择数据文件夹**
   - 点击"浏览数据表文件夹"按钮
   - 选择包含 Excel/CSV 文件的文件夹

5. **生成 C# 类文件**
   - 点击"生成C#文件"按钮
   - 生成的文件位于：`Assets/Scripts/Configs/`

6. **生成 ScriptableObject**
   - 点击"生成SO"按钮
   - 生成的 SO 文件位于：`Assets/Resources/SOConfigs/`

## 注意事项

1. CSV 文件必须使用 UTF-8 编码
2. CSV 中如果字段包含逗号，请使用双引号包裹
3. 第一行的配置信息中，如果没有基类，base 部分可以为空
4. 生成操作会清空目标文件夹中的旧文件

## 文件命名规则

- 数据类：`{TypeName}.cs`（例如：`ItemData.cs`）
- SO 类：`{TypeName}SO.cs`（例如：`ItemDataSO.cs`）
- SO 资源：`{TypeName}SOData.asset`（例如：`ItemDataSOData.asset`）