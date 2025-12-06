# 📚 ModExtensions API 参考

## 🎯 概述

ModExtensions 是 **CustomItemLevelValue** Mod 的扩展框架，允许其他Mod在物品悬停UI的 **五个固定位置** 添加自定义信息，**无需修改主Mod代码**。

## 📍 五个显示位置

| 位置 | 显示顺序 | 建议用途 |
|------|----------|----------|
| **Top1** | 稀有度后 | 物品状态、等级、可用性 |
| **Top2** | 价值后 | 市场需求、评分、趋势 |
| **Top3** | 功能信息前 | 特殊效果、套装信息 |
| **Bottom1** | 描述后 | 来源、背景故事 |
| **Bottom2** | 耐久度前 | 使用提示、维护信息 |

## 🔧 快速开始

### 1. 基本Mod结构

```csharp
using Duckov.Modding;
using Duckov.UI;
using ItemStatsSystem;
using UnityEngine;

namespace YourModName
{
    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        private const string MOD_PREFIX = "YourMod_";
        
        private void OnEnable()
        {
            // 监听物品悬停事件
            ItemHoveringUI.onSetupItem += OnItemHovered;
        }
        
        private void OnItemHovered(ItemHoveringUI ui, Item item)
        {
            if (item == null) return;
            
            // 为物品添加扩展字段
            AddYourExtensions(item);
        }
        
        private void AddYourExtensions(Item item)
        {
            // Top1: 状态信息
            item.Variables.SetString($"{MOD_PREFIX}Top1_状态", 
                "[c=#55FF55]✓ 可用[/c] | [c=#FFAA00]已充能[/c]");
            
            // Top2: 数值信息  
            item.Variables.SetString($"{MOD_PREFIX}Top2_评分",
                "评分: [c=#FFD700]8.5/10.0[/c]");
                
            // Bottom1: 背景信息
            item.Variables.SetString($"{MOD_PREFIX}Bottom1_来源",
                "[hr][c=#AAAAAA]来源: 你的Mod制造[/c][hr]");
        }
        
        private void OnDisable()
        {
            // 清理事件
            ItemHoveringUI.onSetupItem -= OnItemHovered;
            
            // 清理你的字段
            CleanupYourFields();
        }
        
        private void CleanupYourFields()
        {
            // 通过框架清理
            var manager = CustomItemLevelValue.Core.ModExtensionsManager.Instance;
            manager.ClearCacheByPrefix(MOD_PREFIX);
            manager.RemoveAllFieldsWithPrefix(MOD_PREFIX);
        }
    }
}
```

### 2. 字段命名规范

```csharp
// 格式: [Mod前缀]_[位置]_[字段名]
item.Variables.SetString("RPGMod_Top1_等级", "等级: [c=#FFD700]★★★★☆[/c]");
item.Variables.SetString("Market_Top2_需求", "需求: [c=#FF5555]高涨[/c] (+25%)");
item.Variables.SetString("Quest_Bottom1_任务", "[c=#888888]主线任务道具[/c]");
```

## 🎨 富文本系统

### 支持标签

```csharp
// 1. 颜色 [c=#RRGGBB]
"[c=#FF5555]红色[/c]"      // 错误/危险
"[c=#55FF55]绿色[/c]"      // 成功/正面  
"[c=#FFAA00]橙色[/c]"      // 警告/注意
"[c=#FFD700]金色[/c]"      // 稀有/重要
"[c=#5555FF]蓝色[/c]"      // 信息/链接
"[c=#AAAAAA]灰色[/c]"      // 次要/背景

// 2. 格式
"[b]粗体[/b]"
"[i]斜体[/i]"
"[u]下划线[/u]"
"[s]删除线[/s]"

// 3. 字号 [size=数字]
"[size=14]小字[/size]"
"[size=18]标准[/size]"
"[size=22]大字[/size]"

// 4. 换行和分隔
"第一行\n第二行"      // 换行
"[hr]"               // 水平分隔线
```

### 颜色语义参考

| 颜色 | 代码 | 适用场景 |
|------|------|----------|
| 🟢 成功绿 | `#55FF55` | 可用、正常、完成 |
| 🟠 警告橙 | `#FFAA00` | 注意、充能、中等 |
| 🔴 危险红 | `#FF5555` | 错误、危险、警告 |
| 🟡 稀有金 | `#FFD700` | 稀有、重要、高价值 |
| 🔵 信息蓝 | `#5555FF` | 信息、链接、状态 |
| ⚪ 次要灰 | `#AAAAAA` | 背景、说明、次要信息 |

### 组合使用

```csharp
// 复杂示例
item.Variables.SetString("RPG_Top1_状态", 
    "[b][c=#FFD700]传说级[/c][/b] [c=#55FF55]✓ 可用[/c]\n" +
    "等级: [c=#FFAA00]★★★☆☆[/c] (3/5)\n" +
    "[hr][c=#AAAAAA]已绑定[/c][hr]");
```

## ⚙️ API参考

### ModExtensionsManager

```csharp
// 获取实例
var manager = CustomItemLevelValue.Core.ModExtensionsManager.Instance;

// 检查位置是否有内容
bool hasTop1 = manager.HasExtensionsAtPosition(item, "Top1");

// 获取位置的所有扩展
var extensions = manager.GetExtensionsByPosition(item, "Top2");
foreach (var ext in extensions)
{
    Debug.Log($"{ext.DisplayName}: {ext.DisplayValue}");
}

// 清理API
manager.RefreshItemCache(item);          // 清理指定物品缓存
manager.ClearCacheByPrefix("YourMod_");  // 清理指定前缀缓存
manager.ClearAllCache();                 // 清理所有缓存
```

### ExtensionData 结构

```csharp
public class ExtensionData
{
    public string Key;          // "YourMod_Top1_状态"
    public string Position;     // "Top1" 
    public string RawValue;     // "[c=#55FF55]可用[/c]"
    public string DisplayValue; // 处理后的富文本
    public string DisplayName;  // "状态" (自动从Key提取)
    public int SortOrder;       // 显示顺序
}
```

## 📝 最佳实践

### 1. 字段设计原则

```csharp
// ✅ 好：简洁、信息明确
item.Variables.SetString("Mod_Top1_等级", "LV: [c=#FFD700]42[/c]");

// ❌ 不好：太长、信息混杂
item.Variables.SetString("Mod_Top1_所有信息", 
    "等级42 经验5000/6000 攻击力150 防御80 生命值300 魔法值200...");
```

### 2. 颜色使用规范

```csharp
// 状态指示
"[c=#55FF55]✓ 正常[/c]"      // 正常状态
"[c=#FFAA00]⚠️ 警告[/c]"     // 需要关注
"[c=#FF5555]✗ 损坏[/c]"      // 异常状态

// 数值显示  
"[c=#FFD700]100%[/c]"        // 高/满值
"[c=#FFAA00]65%[/c]"         // 中等
"[c=#FF5555]30%[/c]"         // 低值

// 文本强调
"[b][c=#FFD700]重要![/c][/b]" // 重要提示
"[c=#AAAAAA](次要信息)[/c]"   // 备注说明
```

### 3. 清理策略

```csharp
private void OnDisable()
{
    // 1. 移除事件监听
    ItemHoveringUI.onSetupItem -= OnItemHovered;
    
    // 2. 通过框架清理字段
    try
    {
        var manager = CustomItemLevelValue.Core.ModExtensionsManager.Instance;
        manager.ClearCacheByPrefix(MOD_PREFIX);
        manager.RemoveAllFieldsWithPrefix(MOD_PREFIX);
    }
    catch { }
    
    // 3. 直接清理（备选）
    DirectCleanupFields();
}

private void DirectCleanupFields()
{
    var allItems = Object.FindObjectsOfType<Item>();
    foreach (var item in allItems)
    {
        // 清理Variables
        var toRemove = new List<CustomData>();
        foreach (var data in item.Variables)
        {
            if (data?.Key?.StartsWith(MOD_PREFIX) == true)
                toRemove.Add(data);
        }
        foreach (var data in toRemove)
            item.Variables.Remove(data);
            
        // 同样清理Constants...
    }
}
```

### 4. 性能优化

```csharp
private HashSet<int> _processedItems = new HashSet<int>();

private void OnItemHovered(ItemHoveringUI ui, Item item)
{
    if (item == null) return;
    
    // 避免重复处理同一物品
    if (_processedItems.Contains(item.GetInstanceID()))
        return;
        
    _processedItems.Add(item.GetInstanceID());
    
    // 添加字段...
}

// 定时清理已处理记录
private IEnumerator CleanupProcessedRecords()
{
    while (true)
    {
        yield return new WaitForSeconds(300); // 5分钟
        _processedItems.Clear();
    }
}
```

## 🐛 故障排除

### Q1: 字段不显示？
**检查清单：**
- ✓ 字段名格式正确 `前缀_位置_字段名`
- ✓ CustomItemLevelValue Mod已加载且启用
- ✓ 查看游戏日志有无错误
- ✓ 尝试清理缓存 `F11` (调试功能)

### Q2: 颜色/格式不生效？
- 检查标签是否正确闭合 `[c=#FF0000]文本[/c]`
- 避免嵌套层级过深
- 确保使用支持的BBCode标签

### Q3: 同ID物品只有一个显示？
- ModExtensions基于**物品实例**而非TypeID
- 确保为每个物品实例都添加字段
- 移除 `_processedItems` 检查或使用实例ID

### Q4: 编辑Mod后显示旧内容？
- 主Mod启动时会自动清理缓存
- 可手动清理：游戏内按 `F11`
- 或重启游戏

## 🎮 示例Mod

完整示例见 [DemoModExtension](./DemoModExtension.cs)，演示了：
- 五个位置的标准用法
- 富文本颜色和格式
- 正确的清理逻辑
- 实例级别的字段管理

## 📞 支持与反馈

- 🐛 **问题报告**：游戏日志 + 详细描述
- 💡 **功能建议**：通过社区提交
- 📖 **文档更新**：欢迎贡献示例

---

**开始扩展你的Mod吧！** 🚀 使用这个框架，你可以为《逃离鸭科夫》的物品系统添加丰富的自定义信息，提升玩家体验。
