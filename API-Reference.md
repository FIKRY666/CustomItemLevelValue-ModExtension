# 📚 ModExtensions API 参考

## 核心架构

### ModExtensionsManager
主管理器，负责扫描和提供扩展数据。

```csharp
// 静态实例
ModExtensionsManager.Instance

// 核心方法
bool HasExtensionsAtPosition(Item item, string position)
List<ExtensionData> GetExtensionsByPosition(Item item, string position)

// 工具方法
static void WriteDemoField(Item item, string position, string fieldName, string value)
void CleanupDeletedMods()
```

### ExtensionData 数据结构
```csharp
public class ExtensionData
{
    public string Key;          // 完整键名 "Demo_Top1_状态"
    public string Position;     // 位置 "Top1"
    public string RawValue;     // 原始值 "[c=#55FF55]可用[/c]"
    public string DisplayValue; // 处理后值 "<color=#55FF55>可用</color>"
    public string DisplayName;  // 显示名称 "状态"
}
```

## 字段命名规范

### 格式
```
[Mod前缀]_[位置]_[字段名]
```

### 有效示例
```
Market_Top2_需求评分
RPG_Top3_特殊效果
Quest_Bottom1_任务来源
```

### 位置常量
```csharp
"Top1"    // 稀有度后
"Top2"    // 价值后  
"Top3"    // 功能信息前
"Bottom1" // 描述后
"Bottom2" // 耐久度前
```

## 富文本系统

### 支持标签

#### 1. 颜色 `[c]`
```csharp
"[c=#FF5555]红色[/c]"
"[c=#55FF55]绿色[/c]"
"[c=#5555FF]蓝色[/c]"
"[c=#FFD700]金色[/c]"
```

#### 2. 格式
```csharp
"[b]粗体[/b]"
"[i]斜体[/i]"  
"[u]下划线[/u]"
"[s]删除线[/s]"
```

#### 3. 字号 `[size]`
```csharp
"[size=14]小字[/size]"
"[size=18]标准[/size]"
"[size=24]大字[/size]"
```

#### 4. 特殊
```csharp
"[hr]"  // 水平分隔线
"\n"    // 换行
```

### 颜色语义
| 语义 | 颜色代码 | 示例 |
|------|----------|------|
| 成功/正面 | `#55FF55` | ✓ 可用 |
| 警告/注意 | `#FFAA00` | ⚠️ 警告 |
| 错误/负面 | `#FF5555` | ✗ 损坏 |
| 重要/稀有 | `#FFD700` | ★★★★★ |
| 普通信息 | `#AAAAAA` | 常规说明 |

### 嵌套规则
```csharp
// 正确
"[b][c=#FFD700]金色粗体[/c][/b]"

// 错误
"[b][c=#FFD700]错误嵌套[/c]"  // 缺少闭合标签
```

## 基础用法示例

### 写入字段
```csharp
// 方法1：直接写入
item.Variables.SetString("MyMod_Top1_状态", "[c=#55FF55]可用[/c]");

// 方法2：使用工具方法
ModExtensionsManager.WriteDemoField(item, "Top1", "状态", "[c=#55FF55]可用[/c]");
```

### 读取字段
```csharp
// 检查是否有扩展
bool has = ModExtensionsManager.Instance.HasExtensionsAtPosition(item, "Top1");

// 获取扩展数据
var extensions = ModExtensionsManager.Instance.GetExtensionsByPosition(item, "Top1");
foreach (var ext in extensions)
{
    Debug.Log($"{ext.DisplayName}: {ext.DisplayValue}");
}
```

### 清理字段
```csharp
// 在OnDisable中清理
private void OnDisable()
{
    ItemHoveringUI.onSetupItem -= OnItemHovered;
    CleanupOwnFields();
}

private void CleanupOwnFields()
{
    var allItems = Object.FindObjectsOfType<Item>();
    foreach (var item in allItems)
    {
        RemoveFieldsFromCollection(item.Variables, "MyMod_");
        RemoveFieldsFromCollection(item.Constants, "MyMod_");
    }
}
```

## 实用代码片段

### 基础Mod模板
```csharp
using Duckov.Modding;
using Duckov.UI;
using ItemStatsSystem;
using UnityEngine;

namespace YourModName
{
    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        private const string PREFIX = "YourPrefix_";
        private HashSet<int> _processedItems = new HashSet<int>();
        
        private void OnEnable()
        {
            ItemHoveringUI.onSetupItem += OnItemHovered;
        }
        
        private void OnItemHovered(ItemHoveringUI ui, Item item)
        {
            if (item == null || _processedItems.Contains(item.TypeID))
                return;
                
            _processedItems.Add(item.TypeID);
            
            // 添加你的字段
            item.Variables.SetString($"{PREFIX}Top1_状态", "[c=#55FF55]可用[/c]");
            item.Variables.SetString($"{PREFIX}Bottom2_提示", "⚠️ 使用提示");
        }
        
        private void OnDisable()
        {
            ItemHoveringUI.onSetupItem -= OnItemHovered;
            // 清理字段...
        }
    }
}
```

### 字段操作助手
```csharp
public static class ModExtensionsHelper
{
    public static bool SafeWriteField(Item item, string prefix, 
                                      string position, string fieldName, 
                                      string value)
    {
        try
        {
            item.Variables.SetString($"{prefix}{position}_{fieldName}", value);
            return true;
        }
        catch { return false; }
    }
}
```

## 常见问题

### Q: 字段不显示？
检查：
1. 字段名格式正确 `前缀_位置_字段名`
2. CustomItemLevelValue已加载
3. 查看游戏日志有无错误

### Q: 如何改变显示顺序？
字段按字母顺序排序，可通过数字前缀控制：
```
Mod_Top1_01状态
Mod_Top1_02等级
Mod_Top1_03评分
```

### Q: 支持哪些特殊字符？
支持标准BBCode，避免：
- 过多嵌套标签
- 超长文本
- 游戏不支持的Unicode

---

**详细示例见 [DemoModExtension.cs](./DemoModExtension.cs)**
```
