# ModExtensions 框架 - 完整API文档（优化版）

## 📋 目录
- [核心概念](#核心概念)
- [ModExtensionsManager API](#modextensionsmanager-api)
- [ModExtensionsUIRefresher API](#modextensionsuirefresher-api)
- [字段命名规范](#字段命名规范)
- [刷新策略指南](#刷新策略指南)
- [错误处理](#错误处理)
- [性能调优](#性能调优)
- [Mod卸载清理](#mod卸载清理)
- [实战示例](#实战示例)

## 🔧 核心概念

### 框架架构（分离式设计）
```
┌─────────────────────────────────────────┐
│         你的Mod (第三方开发者)           │
├─────────────────────────────────────────┤
│  分离式刷新API：                        │
│  • RefreshCacheOnly() → 仅更新缓存      │
│  • RequestUIRefresh() → 仅重构UI        │
├─────────────────────────────────────────┤
│        ModExtensions 框架层              │
│  • ModExtensionsManager (智能缓存)      │
│  • ModExtensionsUIRefresher (可靠刷新)  │
│  • 主动清理机制 (防残留)                │
├─────────────────────────────────────────┤
│      CustomItemLevelValue 主Mod         │
│  • 扫描并显示字段                       │
│  • 自动清理孤立字段                     │
│  • 分级日志系统                         │
└─────────────────────────────────────────┘
```

### 五个显示位置常量
```csharp
// 在代码中直接使用这些字符串
"Top1"    // 位置1：稀有度后，价值前
"Top2"    // 位置2：价值后，属性前  
"Top3"    // 位置3：属性后，容器前
"Bottom1" // 位置4：描述后，耐久前
"Bottom2" // 位置5：耐久前，最后位置

// 正确使用示例
string fieldKey = $"{PREFIX}Top1_状态";
item.Variables.SetString(fieldKey, "[c=#55FF55]✓ 在线[/c]");
```

## 📚 ModExtensionsManager API

### 🆕 新增：主动清理机制
框架在初始化时会自动扫描并清理孤立的ModExtensions字段，防止Mod卸载后残留。

### 单例访问
```csharp
// 获取ModExtensionsManager实例
ModExtensionsManager manager = ModExtensionsManager.Instance;

// 初始化（主Mod自动调用，第三方Mod无需调用）
manager.Initialize(); // 🆕 包含自动清理孤立字段
```

### 核心方法

#### 1. `RefreshCacheOnly(Item item)` 🆕
**作用**: 仅清除指定物品的扩展缓存，不触发UI刷新
```csharp
/// <summary>
/// 【新API】仅更新缓存，不触发UI刷新
/// 适用场景：批量更新字段，最后统一刷新UI（性能优化）
/// 教学点：分离式API设计，减少不必要UI刷新
/// </summary>
/// <param name="item">目标物品</param>
public void RefreshCacheOnly(Item item)
```
**使用示例**:
```csharp
// 批量更新场景 - 性能优化示例
foreach (var field in fieldsToUpdate)
{
    item.Variables.SetString(field.Key, field.Value);
    ModExtensionsManager.Instance.RefreshCacheOnly(item); // ✅ 只更新缓存，不刷新UI
}
// 所有字段更新完成后...
ModExtensionsUIRefresher.RequestUIRefresh(item); // ✅ 统一触发一次UI重构
```

#### 2. `RefreshItemCache(Item item, bool refreshUI = true)` 🆕
**作用**: 刷新物品缓存，可选择是否触发UI刷新
```csharp
/// <summary>
/// 【增强API】刷新物品缓存（可控UI刷新）
/// 兼容性：原RefreshItemCache(item)等价于RefreshItemCache(item, true)
/// </summary>
/// <param name="item">目标物品</param>
/// <param name="refreshUI">是否触发UI刷新，默认true</param>
public void RefreshItemCache(Item item, bool refreshUI = true)
```
**使用示例**:
```csharp
// 场景1：需要立即显示（用户交互响应）
ModExtensionsManager.Instance.RefreshItemCache(item); // ✅ 缓存+UI一起刷新

// 场景2：后台更新，稍后显示
ModExtensionsManager.Instance.RefreshItemCache(item, false); // ✅ 只更新缓存
// ...其他操作...
ModExtensionsUIRefresher.RequestUIRefresh(item); // ✅ 手动触发UI

// 场景3：兼容旧代码（保持原有行为）
ModExtensionsManager.Instance.RefreshItemCache(item); // ✅ 等价于RefreshItemCache(item, true)
```

#### 3. `GetExtensionsByPosition(Item item, string position, bool forceRescan = false)`
**作用**: 获取物品在指定位置的扩展内容
```csharp
/// <summary>
/// 获取指定位置的扩展数据
/// 🆕 增强：支持forceRescan参数强制重新扫描
/// </summary>
public List<ExtensionData> GetExtensionsByPosition(Item item, string position, bool forceRescan = false)
```
**使用示例**:
```csharp
// 获取Top1位置的所有扩展字段
var top1Extensions = ModExtensionsManager.Instance.GetExtensionsByPosition(item, "Top1");

// 调试：查看字段详情
foreach (var ext in top1Extensions)
{
    Debug.Log($"字段: {ext.Key}, 显示值: {ext.DisplayValue}");
}

// 🆕 强制重新扫描（忽略缓存，重新读取字段）
var freshData = ModExtensionsManager.Instance.GetExtensionsByPosition(
    item, "Top1", forceRescan: true);
```

#### 4. `HasExtensionsAtPosition(Item item, string position)`
**作用**: 快速检查指定位置是否有扩展内容
```csharp
// 性能优化：比GetExtensionsByPosition更快
if (ModExtensionsManager.Instance.HasExtensionsAtPosition(item, "Top1"))
{
    // 有扩展内容，执行相关逻辑
}
```

#### 5. `RefreshItemPositionCache(Item item, string position)`
**作用**: 刷新指定物品的特定位置缓存
```csharp
// 只刷新Top1位置的缓存（精准性能优化）
ModExtensionsManager.Instance.RefreshItemPositionCache(item, "Top1");
```

#### 6. `RefreshCacheByPrefix(string prefix)`
**作用**: 刷新指定前缀的所有缓存
```csharp
// 清理本Mod的所有缓存（Mod重置或卸载时使用）
ModExtensionsManager.Instance.RefreshCacheByPrefix("MyMod_");
```

### 🆕 清理与维护API

#### 7. `MarkModAsDeleted(string prefix)`
**作用**: 标记Mod为已删除（添加到黑名单）
```csharp
/// <summary>
/// 【重要】Mod卸载时必须调用！
/// 标记Mod前缀为已删除，防止字段残留
/// 教学点：协同清理机制，确保无残留
/// </summary>
public void MarkModAsDeleted(string prefix)
```
**Mod卸载时调用**:
```csharp
private void OnDisable()
{
    // 通知主Mod此Mod已删除
    ModExtensionsManager.Instance.MarkModAsDeleted("MyMod_");
    // ... 其他清理代码
}
```

#### 8. `RemoveAllFieldsWithPrefix(string prefix)`
**作用**: 从所有物品中移除指定前缀的字段
```csharp
// 主动清理本Mod的所有字段
ModExtensionsManager.Instance.RemoveAllFieldsWithPrefix("MyMod_");
```

#### 9. `CleanupAllDeletedModsImmediate()` 🆕
**作用**: 立即清理所有已标记为删除的Mod字段
```csharp
// 主Mod定期调用，或场景切换时调用
ModExtensionsManager.Instance.CleanupAllDeletedModsImmediate();
```

### 实用方法

#### 10. `GetCacheStats()`
**作用**: 获取缓存统计信息（调试用）
```csharp
string stats = ModExtensionsManager.Instance.GetCacheStats();
// 输出示例: [ModExtensions] 缓存统计: 15物品, 32位置, 48条目
```

#### 11. `ForceRefreshAll()`
**作用**: 强制刷新所有缓存（谨慎使用）
```csharp
// 调试或极端情况使用
ModExtensionsManager.Instance.ForceRefreshAll();
```

## 🔄 ModExtensionsUIRefresher API

### 🆕 增强：可靠的多层回退机制
`RequestUIRefresh()` 现在包含三层回退，确保UI刷新100%成功。

### 核心方法

#### 1. `RequestUIRefresh(Item item)` 🆕
**作用**: 请求刷新指定物品的UI显示（可靠版）
```csharp
/// <summary>
/// 【增强】请求UI刷新（触发完整面板重构）
/// 内部采用三层回退机制确保成功：
/// 1. 注册的回调（主Mod）
/// 2. InventoryHelper.ForceRefreshItemCache()
/// 3. 反射调用主Mod方法
/// </summary>
public static void RequestUIRefresh(Item item)
```
**使用示例**:
```csharp
// 更新字段后请求UI刷新
item.Variables.SetString($"{PREFIX}Top1_状态", "新状态");
ModExtensionsUIRefresher.RequestUIRefresh(item); // ✅ 可靠触发完整UI重构
```

#### 2. `RegisterRefreshCallback(Action<Item> callback)`
**作用**: 注册UI刷新回调（主Mod专用）
```csharp
// 主Mod初始化时调用
ModExtensionsUIRefresher.RegisterRefreshCallback(RefreshCurrentDisplay);
```

#### 3. `UnregisterRefreshCallback()`
**作用**: 取消注册UI刷新回调
```csharp
// 主Mod卸载时调用
ModExtensionsUIRefresher.UnregisterRefreshCallback();
```

## 📝 字段命名规范

### 标准格式（必须遵守）
```
[Mod前缀]_[位置]_[字段描述]
```
- **Mod前缀**: 2-10字符，以`_`结尾，如 `Market_`, `Quest_`
- **位置**: 五个固定值之一: `Top1`, `Top2`, `Top3`, `Bottom1`, `Bottom2`
- **字段描述**: 英文或拼音，清晰简短，如 `Price`, `Status`

### 正确示例
```csharp
// ✅ 正确
Market_Top1_Price        // 市场Mod的价格显示
RPG_Top3_Attributes      // RPG Mod的属性显示
Quest_Bottom1_Progress   // 任务Mod的进度显示

// ❌ 错误
Price                    // 缺少前缀和位置
Top1_Price               // 缺少前缀
MyMod_Price              // 缺少位置
MyMod_Top1_非常长的字段描述 // 过于复杂
```

### 字段值格式要求
```csharp
// 支持Unity富文本（BBCode）
item.Variables.SetString($"{PREFIX}Top1_状态",
    "[c=#55FF55]✓ 可用[/c] | 耐久: [c=#FFAA00]85%[/c]");

// 🆕 支持渐变文本（使用ApplyHorizontalGradient）
string coloredText = ApplyHorizontalGradient(
    "渐变文本示例", 
    "#FF3333", "#FFFF66", 12);
    
// 支持多行（\n换行）
item.Variables.SetString($"{PREFIX}Top2_详情",
    "第一行信息\n" +
    "[c=#AAAAAA]第二行备注[/c]\n" +
    "第三行内容");
```

## 🔄 刷新策略指南

### 🆕 分离式刷新最佳实践

#### 场景1：用户交互 → 立即刷新
```csharp
private void OnUserInteraction(Item item)
{
    // 用户点击按钮，需要即时反馈
    item.Variables.SetString($"{PREFIX}Top1_状态", "已激活");
    
    // ✅ 标准刷新：缓存+UI一起更新
    ModExtensionsManager.Instance.RefreshItemCache(item);
}
```

#### 场景2：批量处理 → 性能优化
```csharp
private void BatchUpdateFields(Item item, List<FieldUpdate> updates)
{
    int updateCount = 0;
    
    // ✅ 阶段1：只更新缓存（高性能）
    foreach (var update in updates)
    {
        item.Variables.SetString(update.Key, update.Value);
        ModExtensionsManager.Instance.RefreshCacheOnly(item);
        updateCount++;
    }
    
    // ✅ 阶段2：统一刷新UI（一次触发）
    ModExtensionsUIRefresher.RequestUIRefresh(item);
    
    Debug.Log($"批量更新完成: {updateCount}字段，1次UI刷新");
    // 性能提升: 10字段更新 → 从10次UI刷新减少到1次
}
```

#### 场景3：定时更新 → 智能节流
```csharp
private float _nextUpdateTime;
private const float UPDATE_COOLDOWN = 0.5f; // 最小更新间隔

private void UpdateDataPeriodically(Item item)
{
    if (Time.time < _nextUpdateTime) return;
    
    bool dataChanged = CheckDataChanged(item);
    
    if (dataChanged)
    {
        item.Variables.SetString($"{PREFIX}Top1_数据", GetCurrentData());
        
        // ✅ 根据变化程度选择刷新策略
        if (IsMajorChange())
        {
            ModExtensionsManager.Instance.RefreshItemCache(item); // 立即刷新
        }
        else
        {
            ModExtensionsManager.Instance.RefreshCacheOnly(item); // 只更新缓存
            // 累积小变化，稍后统一刷新
        }
        
        _nextUpdateTime = Time.time + UPDATE_COOLDOWN;
    }
}
```

#### 场景4：首次加载 → 延迟优化
```csharp
private HashSet<int> _processedItems = new HashSet<int>();

private void OnItemFirstHovered(Item item)
{
    int instanceId = item.GetInstanceID();
    
    if (!_processedItems.Contains(instanceId))
    {
        // ✅ 首次处理：完整初始化
        InitializeAllFields(item);
        ModExtensionsManager.Instance.RefreshItemCache(item);
        _processedItems.Add(instanceId);
        
        Debug.Log($"首次初始化: {item.DisplayName}");
    }
    else
    {
        // ✅ 已处理过：检查增量更新
        if (CheckForUpdates(item))
        {
            UpdateChangedFields(item);
            ModExtensionsManager.Instance.RefreshItemCache(item);
        }
        // 无变化：零开销使用缓存
    }
}
```

## 🛡️ 错误处理

### 🆕 健壮的刷新封装
```csharp
/// <summary>
/// 安全的刷新包装方法（推荐使用）
/// 教学点：多层错误处理和优雅降级
/// </summary>
private void SafeRefreshWithFallback(Item item, string context = "")
{
    try
    {
        Debug.Log($"[安全刷新] 开始: {item?.DisplayName} ({context})");
        
        if (item == null)
        {
            Debug.LogWarning("[安全刷新] 物品为null");
            return;
        }
        
        // 尝试1：标准分离式刷新
        try
        {
            ModExtensionsManager.Instance.RefreshCacheOnly(item);
            ModExtensionsUIRefresher.RequestUIRefresh(item);
            Debug.Log("[安全刷新] ✅ 分离式刷新成功");
            return;
        }
        catch (Exception ex1)
        {
            Debug.LogWarning($"[安全刷新] 分离式失败: {ex1.Message}");
        }
        
        // 尝试2：传统刷新
        try
        {
            ModExtensionsManager.Instance.RefreshItemCache(item);
            Debug.Log("[安全刷新] ✅ 传统刷新成功");
            return;
        }
        catch (Exception ex2)
        {
            Debug.LogWarning($"[安全刷新] 传统刷新失败: {ex2.Message}");
        }
        
        // 尝试3：强制重新扫描
        try
        {
            ModExtensionsManager.Instance.GetExtensionsByPosition(
                item, "Top1", forceRescan: true);
            Debug.Log("[安全刷新] ✅ 强制重扫描成功");
        }
        catch (Exception ex3)
        {
            Debug.LogError($"[安全刷新] ❌ 所有方案失败: {ex3.Message}");
            throw; // 重新抛出给上层处理
        }
    }
    catch (Exception ex)
    {
        Debug.LogError($"[安全刷新] 未处理异常: {ex.Message}\n{ex.StackTrace}");
        // 可以考虑记录到文件或发送错误报告
    }
}
```

### 框架状态检查
```csharp
/// <summary>
/// 检查ModExtensions框架是否可用
/// </summary>
private bool IsModExtensionsAvailable()
{
    try
    {
        // 检查管理器
        if (ModExtensionsManager.Instance == null)
            return false;
            
        // 检查刷新器
        var refresherType = Type.GetType("CustomItemLevelValue.Core.ModExtensionsUIRefresher");
        if (refresherType == null)
            return false;
            
        // 简单功能测试
        return ModExtensionsUIRefresher.HasRefreshCallback();
    }
    catch
    {
        return false;
    }
}
```

## ⚡ 性能调优

### 🆕 分级日志系统
```csharp
// 在演示Mod中实现的日志级别控制
public enum LogLevel
{
    None = 0,      // 无日志 - 发布版本
    Error = 1,     // 仅错误 - 玩家版本
    Warning = 2,   // 错误 + 警告
    Info = 3,      // 重要信息（默认）
    Debug = 4,     // 调试信息 - 开发者
    Verbose = 5    // 极度详细 - 性能测试
}

// 运行时动态调整
private void AdjustLoggingForPerformance()
{
    #if DEBUG
    SetLogLevel(LogLevel.Debug);  // 开发时详细日志
    #else
    SetLogLevel(LogLevel.Info);   // 发布时精简日志
    #endif
    
    // 根据帧率自动调整
    if (1.0f / Time.deltaTime < 30) // 帧率低于30
    {
        SetLogLevel(LogLevel.Warning); // 减少日志输出
    }
}
```

### 缓存监控
```csharp
private void MonitorCachePerformance()
{
    // 定期检查缓存效率
    string stats = ModExtensionsManager.Instance.GetCacheStats();
    
    // 解析统计信息
    // 格式: [ModExtensions] 缓存统计: 15物品, 32位置, 48条目
    var parts = stats.Split(':');
    if (parts.Length > 1)
    {
        var numbers = parts[1].Trim().Split(',');
        int items = ExtractNumber(numbers[0]);
        int positions = ExtractNumber(numbers[1]);
        int entries = ExtractNumber(numbers[2]);
        
        // 计算缓存密度
        float density = (float)entries / (items * 5); // 5个位置
        if (density < 0.1f)
        {
            Debug.Log($"缓存密度低({density:P0})，考虑清理");
            ModExtensionsManager.Instance.ForceRefreshAll();
        }
    }
}
```

### 🆕 内存优化策略
```csharp
private class ItemCacheRecord
{
    public int InstanceId;
    public float LastAccessTime;
    public int AccessCount;
}

private Dictionary<int, ItemCacheRecord> _accessRecords = new Dictionary<int, ItemCacheRecord>();
private const float CLEANUP_THRESHOLD = 300f; // 5分钟未访问

private void TrackItemAccess(Item item)
{
    int id = item.GetInstanceID();
    
    if (!_accessRecords.ContainsKey(id))
    {
        _accessRecords[id] = new ItemCacheRecord
        {
            InstanceId = id,
            LastAccessTime = Time.time,
            AccessCount = 1
        };
    }
    else
    {
        var record = _accessRecords[id];
        record.LastAccessTime = Time.time;
        record.AccessCount++;
    }
    
    // 定期清理长时间未访问的记录
    if (Time.frameCount % 300 == 0) // 每300帧检查一次
    {
        CleanupOldRecords();
    }
}

private void CleanupOldRecords()
{
    float now = Time.time;
    var toRemove = new List<int>();
    
    foreach (var kvp in _accessRecords)
    {
        if (now - kvp.Value.LastAccessTime > CLEANUP_THRESHOLD)
        {
            toRemove.Add(kvp.Key);
        }
    }
    
    foreach (int id in toRemove)
    {
        _accessRecords.Remove(id);
        // 可选：清理该物品的缓存
        // ModExtensionsManager.Instance.RefreshItemPositionCache(?, ?);
    }
    
    if (toRemove.Count > 0)
    {
        Debug.Log($"清理了 {toRemove.Count} 个旧物品访问记录");
    }
}
```

## 🗑️ Mod卸载清理

### 🆕 三层清理保障机制

#### 1. 主动通知（Mod卸载时）
```csharp
private void OnDisable()
{
    try
    {
        // 第1层：通知主Mod标记此Mod为已删除
        if (ModExtensionsManager.Instance != null)
        {
            ModExtensionsManager.Instance.MarkModAsDeleted("MyMod_");
        }
        
        // 第2层：清理本地字段（可选，主Mod会负责）
        CleanupLocalFields();
        
        // 第3层：清理本地状态
        ClearLocalState();
        
        Debug.Log("✅ Mod卸载清理完成");
    }
    catch (Exception ex)
    {
        Debug.LogError($"Mod卸载异常: {ex.Message}");
    }
}
```

#### 2. 主Mod自动清理（初始化时）
主Mod在初始化时会自动扫描并清理：
- 已标记为删除的Mod字段
- 孤立无主的ModExtensions字段
- 无效或损坏的字段

#### 3. 定期维护（运行时）
```csharp
// 主Mod定期执行的清理协程
private IEnumerator PeriodicMaintenance()
{
    while (true)
    {
        yield return new WaitForSeconds(300f); // 每5分钟
        
        try
        {
            // 清理已删除Mod的字段
            ModExtensionsManager.Instance.CleanupAllDeletedModsImmediate();
            
            // 检查缓存健康度
            string stats = ModExtensionsManager.Instance.GetCacheStats();
            Debug.Log($"[维护] 缓存状态: {stats}");
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[维护] 清理失败: {ex.Message}");
        }
    }
}
```

### 清理检查清单
- [ ] 调用 `MarkModAsDeleted("YourPrefix_")`
- [ ] 移除所有事件监听器
- [ ] 清理本地状态变量
- [ ] 停止所有协程
- [ ] 验证无内存泄漏

## 📊 API速查表（更新版）

### 快速选择指南
| 场景 | 推荐API | 性能影响 | 教学点 |
|------|---------|----------|--------|
| **用户交互后立即显示** | `RefreshItemCache(item)` | 中等 | 即时反馈优先 |
| **批量更新多个字段** | `RefreshCacheOnly()` + `RequestUIRefresh()` | 低 | 分离式API优势 |
| **只更新数据不刷新UI** | `RefreshCacheOnly(item)` | 很低 | 后台预处理 |
| **强制清除缓存重新加载** | `GetExtensionsByPosition(forceRescan:true)` | 高 | 数据源变化时 |
| **Mod卸载清理** | `MarkModAsDeleted(prefix)` | 低 | 协同清理机制 |
| **调试和性能分析** | `GetCacheStats()` | 可忽略 | 监控缓存状态 |

### 🆕 新增API总结
| API | 类型 | 说明 |
|-----|------|------|
| `RefreshCacheOnly()` | 核心 | 仅更新缓存，不触发UI |
| `RefreshItemCache(item, bool)` | 核心 | 可控UI刷新的增强版 |
| `CleanupAllDeletedModsImmediate()` | 维护 | 批量清理已删除Mod字段 |
| 主动清理机制 | 架构 | 初始化时自动检测清理 |

## 🚀 实战示例

### 完整Mod模板
```csharp
using Duckov.Modding;
using ItemStatsSystem;
using UnityEngine;

namespace YourModNamespace
{
    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        private const string PREFIX = "YourMod_";
        private Item _lastItem;
        
        private void Start()
        {
            Debug.Log("YourMod 已加载");
        }
        
        private void OnEnable()
        {
            ItemHoveringUI.onSetupItem += OnItemHovered;
        }
        
        private void OnDisable()
        {
            ItemHoveringUI.onSetupItem -= OnItemHovered;
            
            // 🆕 重要：通知主Mod清理字段
            if (ModExtensionsManager.Instance != null)
            {
                ModExtensionsManager.Instance.MarkModAsDeleted(PREFIX);
            }
        }
        
        private void OnItemHovered(ItemHoveringUI ui, Item item)
        {
            _lastItem = item;
            
            // 1. 更新字段
            UpdateModFields(item);
            
            // 2. 智能刷新
            if (ShouldRefreshImmediately())
            {
                ModExtensionsManager.Instance.RefreshItemCache(item);
            }
            else
            {
                ModExtensionsManager.Instance.RefreshCacheOnly(item);
                // 累积变化，稍后统一刷新
            }
        }
        
        private void UpdateModFields(Item item)
        {
            // Top1位置：状态信息
            item.Variables.SetString($"{PREFIX}Top1_状态",
                $"[c=#55FF55]✓ 已连接[/c]\n" +
                $"时间: {System.DateTime.Now:HH:mm}");
                
            // Top2位置：数据展示
            item.Variables.SetString($"{PREFIX}Top2_数据",
                $"计数: [b]42[/b]\n" +
                $"[c=#AAAAAA]上次更新: 刚刚[/c]");
                
            // 更多字段...
        }
        
        private void Update()
        {
            // 定时更新示例
            if (_lastItem != null && Time.frameCount % 300 == 0)
            {
                UpdateModFields(_lastItem);
                ModExtensionsManager.Instance.RefreshItemCache(_lastItem);
            }
        }
    }
}
```

### 性能监控Mod
```csharp
// PerformanceMonitorMod.cs
// 专门监控ModExtensions性能的调试Mod
public class PerformanceMonitorMod : Duckov.Modding.ModBehaviour
{
    private struct RefreshRecord
    {
        public string ModName;
        public int RefreshCount;
        public float TotalTime;
        public DateTime LastRefresh;
    }
    
    private Dictionary<string, RefreshRecord> _modStats = new Dictionary<string, RefreshRecord>();
    
    private void Update()
    {
        // 每60秒输出性能报告
        if (Time.frameCount % 3600 == 0)
        {
            OutputPerformanceReport();
        }
    }
    
    private void OutputPerformanceReport()
    {
        Debug.Log("=== ModExtensions 性能报告 ===");
        Debug.Log($"时间: {DateTime.Now:HH:mm:ss}");
        Debug.Log($"框架状态: {ModExtensionsManager.Instance.GetCacheStats()}");
        
        foreach (var stat in _modStats.Values.OrderByDescending(s => s.RefreshCount))
        {
            float avgTime = stat.TotalTime / Mathf.Max(1, stat.RefreshCount);
            Debug.Log($"{stat.ModName}: {stat.RefreshCount}次, 平均{avgTime:F2}ms, 最后{stat.LastRefresh:HH:mm:ss}");
        }
        
        Debug.Log("=============================");
    }
}
```

---

## 🆘 故障排除

### 常见问题解决方案

| 问题 | 可能原因 | 解决方案 |
|------|----------|----------|
| **字段不显示** | 1. 命名格式错误<br>2. 未调用刷新API<br>3. 主Mod未加载 | 1. 检查 `前缀_位置_描述` 格式<br>2. 调用 `RefreshItemCache()`<br>3. 检查Mod加载顺序 |
| **刷新延迟** | 1. 首次缓存未命中<br>2. 反射调用开销<br>3. UI重构耗时 | 1. 正常现象，后续会快<br>2. 使用 `RefreshCacheOnly()` 优化<br>3. 减少字段数量和复杂度 |
| **残留字段** | 1. Mod未正确清理<br>2. 主Mod清理机制未触发 | 1. 确保调用 `MarkModAsDeleted()`<br>2. 重启游戏触发自动清理 |
| **性能下降** | 1. 刷新频率过高<br>2. 字段过多过复杂<br>3. 日志输出过多 | 1. 使用 `RefreshCacheOnly()` 批量处理<br>2. 简化字段内容<br>3. 调整日志级别为Info或Warning |

### 调试命令
```csharp
// 在游戏控制台中输入
Debug.Log(ModExtensionsManager.Instance.GetCacheStats()); // 查看缓存状态
ModExtensionsManager.Instance.ForceRefreshAll();          // 强制刷新所有缓存
ModExtensionsManager.Instance.CleanupAllDeletedModsImmediate(); // 清理残留
```

---

## 📞 支持与贡献

### 获取帮助
- **查看示例**：[DemoModExtension](../DemoModExtension/) 完整可运行的示例
- **提交Issue**：在GitHub仓库报告问题
- **社区讨论**：加入开发社区交流

### 贡献指南
1. 遵循字段命名规范
2. 使用分离式刷新API优化性能
3. Mod卸载时正确清理
4. 添加适当的错误处理
5. 考虑性能影响，添加日志级别控制

### 版本兼容性
- **v1.0+**：基础API，`RefreshItemCache(item)`
- **v1.5+**：分离式API，`RefreshCacheOnly()` + `RequestUIRefresh()`
- **v2.0+**：主动清理机制，自动字段检测

---

**🎯 记住核心原则：**
1. **分离式设计**：缓存更新与UI刷新解耦
2. **主动清理**：主Mod负责字段生命周期
3. **性能优先**：批量操作，减少不必要刷新
4. **健壮性**：多层错误处理，优雅降级

**Happy Modding!** 🚀
