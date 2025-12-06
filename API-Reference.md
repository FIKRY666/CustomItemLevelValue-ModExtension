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

## 🔄 动态缓存管理

### 缓存刷新API
当你的Mod动态更新字段时，需要手动刷新缓存才能立即显示新内容：

```csharp
// 使用缓存助手类（推荐）
using CustomItemLevelValue.Utilities;

// 刷新单个物品的缓存
ModExtensionsCacheHelper.RefreshItemCache(item);

// 刷新特定位置缓存
ModExtensionsCacheHelper.RefreshPositionCache(item, "Top1");

// 刷新指定前缀的缓存
ModExtensionsCacheHelper.RefreshByPrefix("MyMod_");

// 强制刷新所有缓存
ModExtensionsCacheHelper.RefreshAll();

// 查看缓存统计
string stats = ModExtensionsCacheHelper.GetStats();
Debug.Log(stats); // 输出: [ModExtensions] 缓存统计: X物品, Y位置, Z条目
```

### 动态更新示例
```csharp
// 实时数据更新演示
public class RealTimeMod : Duckov.Modding.ModBehaviour
{
    private const string PREFIX = "Dynamic_";
    private Dictionary<int, Coroutine> _starAnimations = new Dictionary<int, Coroutine>();
    
    private void OnEnable()
    {
        ItemHoveringUI.onSetupItem += OnItemHovered;
    }
    
    private void OnItemHovered(ItemHoveringUI ui, Item item)
    {
        if (item == null) return;
        
        // 启动星星动画
        StartStarAnimation(item);
    }
    
    private void StartStarAnimation(Item item)
    {
        int itemId = item.GetInstanceID();
        
        // 停止已有动画
        if (_starAnimations.ContainsKey(itemId) && _starAnimations[itemId] != null)
        {
            StopCoroutine(_starAnimations[itemId]);
        }
        
        // 启动新动画
        _starAnimations[itemId] = StartCoroutine(StarAnimationRoutine(item));
    }
    
    private IEnumerator StarAnimationRoutine(Item item)
    {
        int stars = 1; // 从1颗星开始
        
        while (true)
        {
            // 更新星星显示
            UpdateStarDisplay(item, stars);
            
            // 刷新缓存，立即显示更新
            ModExtensionsCacheHelper.RefreshItemCache(item);
            
            // 等待0.3秒
            yield return new WaitForSeconds(0.3f);
            
            // 更新星星数量
            stars++;
            if (stars > 5) // 达到5颗后重置
            {
                // 倒退回1颗星
                for (int i = 4; i >= 1; i--)
                {
                    UpdateStarDisplay(item, i);
                    ModExtensionsCacheHelper.RefreshItemCache(item);
                    yield return new WaitForSeconds(0.3f);
                }
                stars = 1;
            }
        }
    }
    
    private void UpdateStarDisplay(Item item, int filledStars)
    {
        // 生成星星字符串
        string starsText = "";
        for (int i = 1; i <= 5; i++)
        {
            if (i <= filledStars)
            {
                starsText += "[c=#FFD700]★[/c]"; // 实心金星
            }
            else
            {
                starsText += "[c=#AAAAAA]☆[/c]"; // 空心灰星
            }
            if (i < 5) starsText += " ";
        }
        
        // 添加动画提示
        string animationHint = filledStars < 5 ? 
            $"[c=#FFAA00]↗ 增长中...[/c]" : 
            $"[c=#55FFFF]↘ 回落中...[/c]";
        
        // 更新字段
        item.Variables.SetString($"{PREFIX}Top1_动态评分", 
            $"[b]动态评分:[/b] {starsText}\n{animationHint}");
    }
    
    private void OnDisable()
    {
        ItemHoveringUI.onSetupItem -= OnItemHovered;
        
        // 停止所有动画
        foreach (var coroutine in _starAnimations.Values)
        {
            if (coroutine != null) StopCoroutine(coroutine);
        }
        _starAnimations.Clear();
        
        // 清理字段
        ModExtensionsCacheHelper.RefreshByPrefix(PREFIX);
    }
}
```

### 🔄 缓存刷新助手类

```csharp
// ModExtensionsCacheHelper.cs - 提供给其他Mod使用
using UnityEngine;

namespace CustomItemLevelValue.Utilities
{
    /// <summary>
    /// ModExtensions缓存刷新助手
    /// 供其他Mod开发者安全地刷新缓存
    /// </summary>
    public static class ModExtensionsCacheHelper
    {
        /// <summary>
        /// 刷新指定物品的扩展缓存
        /// </summary>
        /// <param name="item">要刷新的物品</param>
        /// <returns>是否成功</returns>
        public static bool RefreshItemCache(Item item)
        {
            return ExecuteCacheMethod("RefreshItemCache", item);
        }
        
        /// <summary>
        /// 刷新指定物品的特定位置缓存
        /// </summary>
        /// <param name="item">目标物品</param>
        /// <param name="position">位置名称</param>
        /// <returns>是否成功</returns>
        public static bool RefreshPositionCache(Item item, string position)
        {
            return ExecuteCacheMethod("RefreshItemPositionCache", item, position);
        }
        
        /// <summary>
        /// 刷新指定前缀的缓存
        /// </summary>
        /// <param name="prefix">Mod前缀</param>
        /// <returns>是否成功</returns>
        public static bool RefreshByPrefix(string prefix)
        {
            return ExecuteCacheMethod("RefreshCacheByPrefix", prefix);
        }
        
        /// <summary>
        /// 强制刷新所有缓存
        /// </summary>
        /// <returns>是否成功</returns>
        public static bool RefreshAll()
        {
            return ExecuteCacheMethod("ForceRefreshAll");
        }
        
        /// <summary>
        /// 获取缓存统计信息
        /// </summary>
        /// <returns>统计信息字符串</returns>
        public static string GetStats()
        {
            try
            {
                var type = GetManagerType();
                if (type == null) return "ModExtensions未加载";
                
                var instance = type.GetProperty("Instance").GetValue(null);
                var method = type.GetMethod("GetCacheStats");
                
                return method.Invoke(instance, null) as string;
            }
            catch (System.Exception)
            {
                return "获取统计失败";
            }
        }
        
        private static bool ExecuteCacheMethod(string methodName, params object[] args)
        {
            try
            {
                var type = GetManagerType();
                if (type == null) return false;
                
                var instance = type.GetProperty("Instance").GetValue(null);
                var method = type.GetMethod(methodName);
                
                if (method == null) return false;
                
                method.Invoke(instance, args);
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[CacheHelper] {methodName}失败: {ex.Message}");
                return false;
            }
        }
        
        private static System.Type GetManagerType()
        {
            return System.Type.GetType("CustomItemLevelValue.Core.ModExtensionsManager, CustomItemLevelValue");
        }
    }
}
```

## 🔧 快速开始

### 1. 基本Mod结构

```csharp
using Duckov.Modding;
using Duckov.UI;
using ItemStatsSystem;
using UnityEngine;
using CustomItemLevelValue.Utilities; // 引用缓存助手

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
            
            // 使用缓存助手清理
            ModExtensionsCacheHelper.RefreshByPrefix(MOD_PREFIX);
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

// 缓存刷新API
manager.RefreshItemCache(item);          // 刷新指定物品缓存
manager.RefreshItemPositionCache(item, "Top1"); // 刷新指定位置
manager.RefreshCacheByPrefix("YourMod_"); // 刷新前缀缓存
manager.ForceRefreshAll();               // 强制刷新所有缓存

// 统计信息
string stats = manager.GetCacheStats();
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

### 2. 动态数据刷新策略

```csharp
// ✅ 推荐：数据变更后立即刷新
void UpdateRealTimeData(Item item, float newValue)
{
    item.Variables.SetString($"{PREFIX}Top2_实时数据", 
        $"[c=#55FFFF]实时: {newValue:F1}[/c]");
    
    ModExtensionsCacheHelper.RefreshItemCache(item); // 立即生效
}

// ✅ 批量更新优化
void BatchUpdate(List<Item> items)
{
    foreach (var item in items)
    {
        UpdateFields(item);
    }
    ModExtensionsCacheHelper.RefreshAll(); // 一次刷新所有
}

// ❌ 避免：高频无意义刷新
void Update() // 每帧调用
{
    // 不要在这里刷新缓存！
}
```

### 3. 颜色使用规范

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

### 4. 清理策略

```csharp
private void OnDisable()
{
    // 1. 移除事件监听
    ItemHoveringUI.onSetupItem -= OnItemHovered;
    
    // 2. 使用缓存助手清理
    ModExtensionsCacheHelper.RefreshByPrefix(MOD_PREFIX);
    
    // 3. 直接清理（备选方案）
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

### 5. 性能优化

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
- ✓ 尝试清理缓存 `ModExtensionsCacheHelper.RefreshAll()`

### Q2: 动态更新不生效？
- ✓ 更新字段后是否调用了 `RefreshItemCache(item)`？
- ✓ 检查缓存助手是否成功加载
- ✓ 查看游戏日志是否有反射错误

### Q3: 颜色/格式不生效？
- 检查标签是否正确闭合 `[c=#FF0000]文本[/c]`
- 避免嵌套层级过深
- 确保使用支持的BBCode标签

### Q4: 同ID物品只有一个显示？
- ModExtensions基于**物品实例**而非TypeID
- 确保为每个物品实例都添加字段
- 移除 `_processedItems` 检查或使用实例ID

### Q5: 编辑Mod后显示旧内容？
- 主Mod启动时会自动清理缓存
- 可手动清理：`ModExtensionsCacheHelper.RefreshAll()`
- 或重启游戏

## 🎮 示例Mod

完整示例见 [DemoModExtension](./DemoModExtension.md)，演示了：
- 五个位置的标准用法
- 富文本颜色和格式
- 正确的清理逻辑
- 实例级别的字段管理
- 动态数据更新和缓存刷新

## 📞 支持与反馈

- 🐛 **问题报告**：游戏日志 + 详细描述
- 💡 **功能建议**：通过社区提交
- 📖 **文档更新**：欢迎贡献示例
- 🔄 **缓存问题**：使用 `ModExtensionsCacheHelper.GetStats()` 获取信息

---

**开始扩展你的Mod吧！** 🚀 使用这个框架，你可以为《逃离鸭科夫》的物品系统添加丰富的自定义信息，支持实时数据更新和动态显示。
