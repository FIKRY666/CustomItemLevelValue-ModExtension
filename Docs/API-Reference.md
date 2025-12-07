## 📚 API参考文档

# CustomItemLevelValue ModExtensions API 参考

## 概述

本文档详细说明了 ModExtensions 框架的所有可用 API。框架提供两种使用模式：

- **便携API模式**：通过反射调用，无需DLL引用
- **直接引用模式**：直接引用DLL，性能更好

## 目录

1. [核心管理器 API](#核心管理器-api)
2. [便携API模式](#便携api模式) 
3. [字段命名规范](#字段命名规范)
4. [显示位置常量](#显示位置常量)
5. [富文本格式](#富文本格式)
6. [示例代码](#示例代码)

## 核心管理器 API

### ModExtensionsManager 类

单例类，提供 ModExtensions 框架的核心功能。

#### 属性

| 属性 | 类型 | 说明 |
|------|------|------|
| `Instance` | `ModExtensionsManager` | 获取单例实例（静态属性） |

#### 方法

##### 注册与管理

```csharp
// 注册Mod前缀（启用Mod时调用）
public void RegisterMod(string prefix);

// 标记Mod为已删除（禁用Mod时调用）
public void MarkModAsDeleted(string prefix);

// 获取缓存统计信息（调试用）
public string GetCacheStats();
```

##### 数据写入与读取

```csharp
// 获取指定位置的扩展数据
public List<ExtensionData> GetExtensionsByPosition(
    Item item, 
    string position, 
    bool forceRescan = false);

// 检查指定位置是否有扩展内容
public bool HasExtensionsAtPosition(Item item, string position);
```

##### 缓存管理

```csharp
// 刷新物品缓存（默认触发UI刷新）
public void RefreshItemCache(Item item);

// 刷新物品缓存（可控制UI刷新）
public void RefreshItemCache(Item item, bool refreshUI);

// 仅刷新缓存，不触发UI
public void RefreshCacheOnly(Item item);

// 刷新指定位置的缓存
public void RefreshItemPositionCache(Item item, string position);

// 刷新指定前缀的缓存
public void RefreshCacheByPrefix(string prefix);

// 强制刷新所有缓存
public void ForceRefreshAll();
```

##### 清理工具

```csharp
// 清理空值字段
public void CleanupEmptyExtensionsFields();

// 强制清理所有ModExtensions字段（谨慎使用！）
public void ForcePurgeAllModExtensionsFields();
```

### ExtensionData 结构

表示单个扩展字段的数据。

```csharp
public class ExtensionData
{
    public string Key;           // 完整字段键名，如 "MyMod_Top1_状态"
    public string Position;      // 位置名称，如 "Top1"
    public string RawValue;      // 原始值（BBCode格式）
    public string DisplayValue;  // 处理后值（Unity富文本）
    public string DisplayName;   // 显示名称，如 "状态"
    public int SortOrder;        // 排序顺序
}
```

## 便携API模式

### ModExtensionsAPI 类

静态类，提供无需DLL引用的API访问。

#### 方法

```csharp
// 初始化API（自动调用，失败返回false）
public static bool Init();

// 检查API是否可用
public static bool IsAvailable();

// 注册Mod前缀
public static bool RegisterMod(string prefix);

// 标记Mod为已删除
public static bool MarkModAsDeleted(string prefix);

// 刷新物品缓存
public static bool RefreshItemCache(Item item, bool refreshUI = true);

// 简单测试方法
public static void Test();
```

#### 使用示例

```csharp
// 初始化检查
if (!ModExtensionsAPI.Init())
{
    Debug.LogError("ModExtensions框架未加载！");
    return;
}

// 注册Mod
ModExtensionsAPI.RegisterMod("MyMod_");

// 写入字段后刷新
ModExtensionsAPI.RefreshItemCache(item);
```

## 字段命名规范

### 格式要求

```
{前缀}_{位置}_{字段名}
```

### 示例

| 字段键名 | 说明 |
|----------|------|
| `MyMod_Top1_状态` | Mod前缀: MyMod_, 位置: Top1, 字段名: 状态 |
| `Quest_Bottom1_进度` | Mod前缀: Quest_, 位置: Bottom1, 字段名: 进度 |
| `Market_Top2_价格` | Mod前缀: Market_, 位置: Top2, 字段名: 价格 |

### 命名建议

1. **前缀**：使用简短、唯一的标识符，避免与其他Mod冲突
2. **字段名**：使用有意义的英文或拼音，避免特殊字符
3. **分隔符**：统一使用下划线 `_`

## 显示位置常量

框架支持5个固定显示位置：

### 位置常量

| 位置常量 | 显示顺序 | 推荐用途 |
|----------|----------|----------|
| `"Top1"` | 1 | 状态信息、紧急通知 |
| `"Top2"` | 2 | 市场数据、价值信息 |
| `"Top3"` | 3 | 评分建议、特殊效果 |
| `"Bottom1"` | 4 | 背景故事、来源说明 |
| `"Bottom2"` | 5 | 使用提示、维护建议 |

### 位置获取方法

```csharp
// 获取所有有效位置
string[] validPositions = { "Top1", "Top2", "Top3", "Bottom1", "Bottom2" };

// 检查位置是否有效
bool isValid = validPositions.Contains(position);
```

## 富文本格式

### 支持标签

框架支持 BBCode 到 Unity 富文本的转换：

#### 颜色标签
```csharp
"[color=#FF0000]红色文本[/color]"
"[color=green]绿色文本[/color]"  // 支持颜色名称
"[c=#00FF00]简写颜色[/c]"       // 简写格式
```

#### 样式标签
```csharp
"[b]粗体文本[/b]"      // 加粗
"[i]斜体文本[/i]"      // 斜体
"[size=14]大小14[/size]" // 字号
```

#### 特殊元素
```csharp
"[hr]"                // 水平分隔线
"★☆☆☆☆"              // 星级评分（Unicode字符）
"↑12%"               // 上升符号
"↓5%"                // 下降符号
```

### 颜色参考表

| 颜色 | 代码 | 用途 |
|------|------|------|
| 成功绿 | `#55FF55` | 成功、可用、正常 |
| 警告黄 | `#FFAA00` | 警告、注意、中等 |
| 错误红 | `#FF5555` | 错误、危险、停止 |
| 重要金 | `#FFD700` | 重要、稀有、珍贵 |
| 信息蓝 | `#5555FF` | 信息、说明、提示 |
| 紫色 | `#AA55FF` | 特殊、魔法、独特 |

### 动态变量

支持在文本中插入动态值：

```csharp
// 原始BBCode
string text = "[b]耐久:[/b] {durability}%";

// 处理时替换
text = text.Replace("{durability}", item.Durability.ToString("F0"));
```

## 示例代码

### 完整Mod示例

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
        private Item _currentItem;
        private bool _usePortableAPI = true; // 切换模式

        void OnEnable()
        {
            // 模式选择
            if (_usePortableAPI)
            {
                // 便携API模式
                if (!ModExtensionsAPI.Init())
                {
                    Debug.LogError("框架未加载！");
                    return;
                }
                ModExtensionsAPI.RegisterMod(MOD_PREFIX);
            }
            else
            {
                // 直接引用模式
                ModExtensionsManager.Instance.RegisterMod(MOD_PREFIX);
            }

            ItemHoveringUI.onSetupItem += OnItemHovered;
            Debug.Log("Mod已启用");
        }

        void OnDisable()
        {
            ItemHoveringUI.onSetupItem -= OnItemHovered;
            
            if (_usePortableAPI)
                ModExtensionsAPI.MarkModAsDeleted(MOD_PREFIX);
            else
                ModExtensionsManager.Instance.MarkModAsDeleted(MOD_PREFIX);
        }

        private void OnItemHovered(ItemHoveringUI ui, Item item)
        {
            if (item == null) return;
            _currentItem = item;
            
            // 写入字段
            WriteCustomFields(item);
            
            // 刷新显示
            RefreshItem(item);
        }

        private void WriteCustomFields(Item item)
        {
            // Top1: 状态信息
            string status = item.Durability > 0.5f 
                ? "[color=#55FF55]✓ 状态良好[/color]" 
                : "[color=#FF5555]⚠️ 需要修复[/color]";
            item.Variables.SetString($"{MOD_PREFIX}Top1_状态", status);
            
            // Top2: 价值评估
            float value = CalculateValue(item);
            item.Variables.SetString($"{MOD_PREFIX}Top2_价值", 
                $"[b]估值:[/b] [color=#FFD700]{value:N0}金币[/color]");
            
            // Bottom1: 自定义描述
            item.Variables.SetString($"{MOD_PREFIX}Bottom1_描述", 
                "这是通过ModExtensions框架添加的自定义信息");
        }

        private void RefreshItem(Item item)
        {
            if (_usePortableAPI)
                ModExtensionsAPI.RefreshItemCache(item, true);
            else
                ModExtensionsManager.Instance.RefreshItemCache(item, true);
        }
        
        private float CalculateValue(Item item)
        {
            // 你的价值计算逻辑
            return 1000f;
        }
    }
}
```

### 批量更新示例

```csharp
// 批量更新多个字段，最后统一刷新
void UpdateMultipleItems(List<Item> items)
{
    foreach (var item in items)
    {
        UpdateItemFields(item);
        // 先更新缓存，不触发UI
        ModExtensionsManager.Instance.RefreshCacheOnly(item);
    }
    
    // 所有字段更新完后，统一触发UI刷新
    if (items.Count > 0)
    {
        ModExtensionsUIRefresher.RequestUIRefresh(items[0]);
    }
}
```

### 错误处理示例

```csharp
void SafeRefresh(Item item)
{
    try
    {
        if (_usePortableAPI)
        {
            if (!ModExtensionsAPI.RefreshItemCache(item))
            {
                Debug.LogWarning("便携API刷新失败，尝试直接引用模式");
                TryDirectMode(item);
            }
        }
        else
        {
            ModExtensionsManager.Instance.RefreshItemCache(item, true);
        }
    }
    catch (System.Exception ex)
    {
        Debug.LogError($"刷新失败: {ex.Message}");
        // 备用方案
        BackupRefresh(item);
    }
}

void BackupRefresh(Item item)
{
    // 强制重新扫描
    var data = ModExtensionsManager.Instance.GetExtensionsByPosition(
        item, "Top1", forceRescan: true);
    
    // 手动触发UI更新
    var hoverUI = UnityEngine.Object.FindObjectOfType<ItemHoveringUI>();
    if (hoverUI != null)
    {
        hoverUI.SetupItem(item);
    }
}
```

## 性能最佳实践

### 推荐做法

1. **批量更新**：更新多个字段后统一刷新
2. **缓存利用**：避免频繁强制重新扫描
3. **前缀管理**：及时清理不再使用的字段
4. **错误处理**：添加适当的异常捕获

### 避免的做法

1. 不要在Update()中每帧刷新
2. 不要创建过多的字段（建议每个位置1-2个）
3. 不要忘记在Mod禁用时清理字段

## 调试技巧

### 启用调试日志

```csharp
// 在Mod初始化时添加
void Start()
{
    #if DEBUG
    Debug.Log($"[MyMod] 框架状态: {ModExtensionsManager.Instance.GetCacheStats()}");
    
    // 测试API连接
    ModExtensionsAPI.Test();
    #endif
}
```

### 检查字段写入

```csharp
void CheckFieldWritten(Item item, string fieldKey)
{
    bool hasField = item.Variables.ContainsKey(fieldKey);
    Debug.Log($"字段 {fieldKey} 是否存在: {hasField}");
    
    if (hasField)
    {
        string value = item.Variables.GetString(fieldKey);
        Debug.Log($"字段值: {value}");
    }
}
```

## 常见问题

### Q: 字段写入后不显示？
1. 检查字段键名格式：`{前缀}_{位置}_{字段名}`
2. 确认调用了刷新API
3. 检查主Mod是否已加载

### Q: 如何切换使用模式？
1. 便携API → 直接引用：添加DLL引用，修改API调用
2. 直接引用 → 便携API：移除DLL引用，使用ModExtensionsAPI类

### Q: 多个Mod字段冲突？
确保每个Mod使用唯一的前缀，框架会自动隔离不同Mod的字段。

## 版本历史

| 版本 | 日期 | 变更说明 |
|------|------|----------|
| 2.0.0 | 2024.01 | 新增便携API模式，支持无DLL引用 |
| 1.5.0 | 2023.12 | 优化缓存系统，性能提升 |
| 1.0.0 | 2023.11 | 初始版本发布 |

## 获取帮助

- [GitHub Issues](https://github.com/FIKRY666/CustomItemLevelValue-ModExtension/issues) - 报告问题
- [Steam讨论区](https://steamcommunity.com/workshop/filedetails/discussion/3612733981/689742326557380285/) - 社区支持
- [示例项目](https://github.com/FIKRY666/CustomItemLevelValue-ModExtension/tree/main/Demo) - 完整代码示例

