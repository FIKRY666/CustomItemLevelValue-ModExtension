# ModExtensions 框架 - 完整API文档

## 📋 目录
- [核心概念](#核心概念)
- [ModExtensionsManager API](#modextensionsmanager-api)
- [ModExtensionsUIRefresher API](#modextensionsuirefresher-api)
- [字段命名规范](#字段命名规范)
- [刷新策略指南](#刷新策略指南)
- [错误处理](#错误处理)
- [性能调优](#性能调优)

## 🔧 核心概念

### 框架架构
```
┌─────────────────────────────────────────┐
│         你的Mod (第三方开发者)           │
├─────────────────────────────────────────┤
│  1. 写入字段到物品 Variables/Constants  │
│  2. 调用刷新API                        │
├─────────────────────────────────────────┤
│        ModExtensions 框架层              │
│  • ModExtensionsManager (缓存管理)      │
│  • ModExtensionsUIRefresher (UI协调)    │
├─────────────────────────────────────────┤
│      CustomItemLevelValue 主Mod         │
│  • 扫描并显示字段                       │
│  • 应用样式和布局                       │
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

// 使用示例
item.Variables.SetString($"{PREFIX}Top1_状态", "内容");
```

## 📚 ModExtensionsManager API

### 单例访问
```csharp
// 获取ModExtensionsManager实例
ModExtensionsManager manager = ModExtensionsManager.Instance;
```

### 核心方法

#### 1. `Initialize()`
**作用**: 初始化管理器，清理旧缓存
```csharp
// 在Mod的Start方法中调用一次
ModExtensionsManager.Instance.Initialize();
```

#### 2. `RefreshCacheOnly(Item item)`
**作用**: 仅清除指定物品的扩展缓存，不触发UI刷新
```csharp
/// <summary>
/// 仅更新缓存，不触发UI刷新
/// 适用场景：批量更新字段，最后统一刷新UI
/// </summary>
/// <param name="item">目标物品</param>
public void RefreshCacheOnly(Item item)
```
**使用示例**:
```csharp
// 批量更新场景
foreach (var field in fieldsToUpdate)
{
    item.Variables.SetString(field.Key, field.Value);
    ModExtensionsManager.Instance.RefreshCacheOnly(item); // 只更新缓存
}
// 所有字段更新完成后...
ModExtensionsUIRefresher.RequestUIRefresh(item); // 统一刷新UI
```

#### 3. `RefreshItemCache(Item item, bool refreshUI = true)`
**作用**: 刷新物品缓存，可选择是否触发UI刷新
```csharp
/// <summary>
/// 刷新物品缓存（可控UI刷新）
/// </summary>
/// <param name="item">目标物品</param>
/// <param name="refreshUI">是否触发UI刷新，默认true</param>
public void RefreshItemCache(Item item, bool refreshUI = true)
```
**使用示例**:
```csharp
// 场景1：需要立即显示（默认）
ModExtensionsManager.Instance.RefreshItemCache(item); // 等价于 RefreshItemCache(item, true)

// 场景2：只更新缓存，稍后显示
ModExtensionsManager.Instance.RefreshItemCache(item, false);
// ...其他操作...
ModExtensionsUIRefresher.RequestUIRefresh(item); // 手动触发UI
```

#### 4. `GetExtensionsByPosition(Item item, string position, bool forceRescan = false)`
**作用**: 获取物品在指定位置的扩展内容
```csharp
/// <summary>
/// 获取指定位置的扩展数据
/// </summary>
/// <param name="item">目标物品</param>
/// <param name="position">位置名称（Top1/Top2/Top3/Bottom1/Bottom2）</param>
/// <param name="forceRescan">是否强制重新扫描（忽略缓存）</param>
/// <returns>扩展数据列表</returns>
public List<ExtensionData> GetExtensionsByPosition(Item item, string position, bool forceRescan = false)
```
**返回类型**:
```csharp
public class ExtensionData
{
    public string Key;           // 完整字段键名，如 "Demo_Top1_状态"
    public string Position;      // 位置名称，如 "Top1"
    public string RawValue;      // 原始值（BBCode格式）
    public string DisplayValue;  // 处理后值（Unity富文本）
    public string DisplayName;   // 显示名称（自动提取）
    public int SortOrder;        // 排序顺序
}
```
**使用示例**:
```csharp
// 获取Top1位置的所有扩展字段
var top1Extensions = ModExtensionsManager.Instance.GetExtensionsByPosition(item, "Top1");

// 调试：查看字段详情
foreach (var ext in top1Extensions)
{
    Debug.Log($"字段: {ext.Key}, 值: {ext.DisplayValue}");
}

// 强制重新扫描（清除缓存重新读取）
var freshData = ModExtensionsManager.Instance.GetExtensionsByPosition(
    item, "Top1", forceRescan: true);
```

#### 5. `HasExtensionsAtPosition(Item item, string position)`
**作用**: 检查指定位置是否有扩展内容
```csharp
/// <summary>
/// 快速检查是否有扩展内容
/// </summary>
/// <param name="item">目标物品</param>
/// <param name="position">位置名称</param>
/// <returns>是否有扩展内容</returns>
public bool HasExtensionsAtPosition(Item item, string position)
```
**使用示例**:
```csharp
// 检查是否需要显示Top1位置
if (ModExtensionsManager.Instance.HasExtensionsAtPosition(item, "Top1"))
{
    // 有扩展内容，执行相关逻辑
}
```

#### 6. `RefreshItemPositionCache(Item item, string position)`
**作用**: 刷新指定物品的特定位置缓存
```csharp
/// <summary>
/// 刷新指定位置的缓存
/// </summary>
/// <param name="item">目标物品</param>
/// <param name="position">位置名称</param>
public void RefreshItemPositionCache(Item item, string position)
```
**使用示例**:
```csharp
// 只刷新Top1位置的缓存（性能优化）
ModExtensionsManager.Instance.RefreshItemPositionCache(item, "Top1");
```

#### 7. `RefreshCacheByPrefix(string prefix)`
**作用**: 刷新指定前缀的所有缓存
```csharp
/// <summary>
/// 刷新指定前缀的所有缓存
/// 适用场景：Mod批量更新字段时调用
/// </summary>
/// <param name="prefix">Mod前缀，如 "Demo_"</param>
public void RefreshCacheByPrefix(string prefix)
```
**使用示例**:
```csharp
// 清理本Mod的所有缓存
ModExtensionsManager.Instance.RefreshCacheByPrefix("MyMod_");
```

#### 8. `ForceRefreshAll()`
**作用**: 强制刷新所有缓存
```csharp
/// <summary>
/// 强制刷新所有缓存（谨慎使用）
/// 适用场景：调试或Mod卸载时清理
/// </summary>
public void ForceRefreshAll()
```

### 实用方法

#### 9. `GetCacheStats()`
**作用**: 获取缓存统计信息（调试用）
```csharp
/// <summary>
/// 获取缓存统计信息
/// </summary>
/// <returns>统计信息字符串</returns>
public string GetCacheStats()
```
**使用示例**:
```csharp
string stats = ModExtensionsManager.Instance.GetCacheStats();
Debug.Log(stats); // 输出: [ModExtensions] 缓存统计: 15物品, 32位置, 48条目
```

#### 10. `MarkModAsDeleted(string prefix)`
**作用**: 标记Mod为已删除（添加到黑名单）
```csharp
/// <summary>
/// 标记Mod前缀为已删除
/// 适用场景：Mod卸载时调用，防止残留字段
/// </summary>
/// <param name="prefix">Mod前缀</param>
public void MarkModAsDeleted(string prefix)
```

#### 11. `RemoveAllFieldsWithPrefix(string prefix)`
**作用**: 移除所有包含指定前缀的字段
```csharp
/// <summary>
/// 从所有物品中移除指定前缀的字段
/// 适用场景：Mod卸载时清理残留数据
/// </summary>
/// <param name="prefix">要清理的前缀</param>
public void RemoveAllFieldsWithPrefix(string prefix)
```

### 兼容性方法（旧API）

#### 12. `ClearAllCache()`
```csharp
// 等价于 ForceRefreshAll()
public void ClearAllCache()
```

#### 13. `ClearCacheByPrefix(string prefix)`
```csharp
// 等价于 RefreshCacheByPrefix(prefix)
public void ClearCacheByPrefix(string prefix)
```

#### 14. `ForceClearAll()`
```csharp
// 等价于 ForceRefreshAll()
public void ForceClearAll()
```

## 🔄 ModExtensionsUIRefresher API

### 静态类访问
```csharp
// 直接调用静态方法
ModExtensionsUIRefresher.RequestUIRefresh(item);
```

### 核心方法

#### 1. `RequestUIRefresh(Item item)`
**作用**: 请求刷新指定物品的UI显示
```csharp
/// <summary>
/// 请求UI刷新（触发完整面板重构）
/// 内部采用多层回退机制确保成功
/// </summary>
/// <param name="item">需要刷新UI的物品</param>
public static void RequestUIRefresh(Item item)
```
**内部机制**:
1. 优先使用注册的回调（主Mod已注册）
2. 备用：通过InventoryHelper触发完整刷新
3. 备用：通过反射调用主Mod刷新方法

**使用示例**:
```csharp
// 更新字段后请求UI刷新
item.Variables.SetString($"{PREFIX}Top1_状态", "新状态");
ModExtensionsUIRefresher.RequestUIRefresh(item);
```

#### 2. `RegisterRefreshCallback(Action<Item> callback)`
**作用**: 注册UI刷新回调（主Mod专用）
```csharp
/// <summary>
/// 注册UI刷新回调（主Mod调用）
/// </summary>
/// <param name="callback">刷新回调函数</param>
public static void RegisterRefreshCallback(Action<Item> callback)
```
**主Mod使用示例**:
```csharp
// 在主Mod的初始化代码中
ModExtensionsUIRefresher.RegisterRefreshCallback(RefreshCurrentDisplay);
```

#### 3. `UnregisterRefreshCallback()`
**作用**: 取消注册UI刷新回调
```csharp
/// <summary>
/// 取消注册UI刷新回调
/// 适用场景：Mod卸载时调用
/// </summary>
public static void UnregisterRefreshCallback()
```

#### 4. `HasRefreshCallback()`
**作用**: 检查是否有可用的刷新回调
```csharp
/// <summary>
/// 检查刷新回调是否可用
/// </summary>
/// <returns>是否有注册的回调</returns>
public static bool HasRefreshCallback()
```

## 📝 字段命名规范

### 标准格式
```
[Mod前缀]_[位置]_[字段描述]
```

### 格式详解
| 部分 | 要求 | 示例 |
|------|------|------|
| **Mod前缀** | 2-10字符，以`_`结尾 | `Market_`, `Quest_`, `RPG_` |
| **位置** | 五个固定值之一 | `Top1`, `Top2`, `Top3`, `Bottom1`, `Bottom2` |
| **字段描述** | 使用英文或拼音，清晰描述 | `Price`, `Status`, `Story` |

### 正确示例
```csharp
// ✅ 正确
Market_Top1_Price        // 市场Mod，Top1位置，价格字段
Quest_Bottom1_Story      // 任务Mod，Bottom1位置，故事字段
RPG_Top3_Score           // RPG Mod，Top3位置，评分字段

// ❌ 错误
Price                    // 缺少前缀和位置
Top1_Price               // 位置在前，缺少前缀
Market_Price             // 缺少位置
Market_Top1_价格_历史_最高 // 过于复杂，使用下划线连接
```

### 字段值格式要求
```csharp
// 支持Unity富文本（BBCode）
item.Variables.SetString($"{PREFIX}Top1_状态",
    "[c=#55FF55]✓ 可用[/c] | 耐久: [c=#FFAA00]85%[/c]");

// 支持多行（\n换行）
item.Variables.SetString($"{PREFIX}Top2_详情",
    "第一行信息\n" +
    "[c=#AAAAAA]第二行备注[/c]\n" +
    "[size=12]第三行小字[/size]");

// 支持特殊符号
item.Variables.SetString($"{PREFIX}Top3_评分",
    "评分: ★★★★☆\n" +
    "状态: ✓ 正常 ⚡ 充能中");
```

## 🔄 刷新策略指南

### 场景1：单次更新，立即显示
```csharp
// 用户交互触发，需要即时反馈
private void OnButtonClick(Item item)
{
    item.Variables.SetString($"{PREFIX}Top1_状态", "已激活");
    
    // 标准刷新：缓存+UI一起更新
    ModExtensionsManager.Instance.RefreshItemCache(item);
    // 或等价于：
    // ModExtensionsManager.Instance.RefreshItemCache(item, true);
}
```

### 场景2：批量更新，性能优化
```csharp
// 批量处理多个字段，最后统一刷新
private void UpdateMultipleFields(Item item, List<FieldUpdate> updates)
{
    // 阶段1：只更新缓存
    foreach (var update in updates)
    {
        item.Variables.SetString(update.Key, update.Value);
        ModExtensionsManager.Instance.RefreshCacheOnly(item);
    }
    
    // 阶段2：统一刷新UI
    ModExtensionsUIRefresher.RequestUIRefresh(item);
    
    // 性能对比：10个字段更新
    // 传统方式：10次完整刷新，10倍开销
    // 优化方式：10次缓存更新 + 1次UI刷新，1.1倍开销
}
```

### 场景3：定时更新，频率控制
```csharp
private float _lastUpdateTime;
private const float UPDATE_INTERVAL = 2.0f; // 每2秒更新一次

private void Update()
{
    if (Time.time - _lastUpdateTime >= UPDATE_INTERVAL)
    {
        UpdateData();
        _lastUpdateTime = Time.time;
        
        // 根据数据变化决定刷新方式
        if (数据变化量大)
        {
            ModExtensionsManager.Instance.RefreshItemCache(_currentItem);
        }
        else if (数据轻微变化)
        {
            ModExtensionsManager.Instance.RefreshCacheOnly(_currentItem);
            // 可以累积多次小变化后再刷新UI
        }
    }
}
```

### 场景4：首次加载，延迟优化
```csharp
private Dictionary<int, bool> _initializedItems = new Dictionary<int, bool>();

private void OnItemHovered(ItemHoveringUI ui, Item item)
{
    int instanceId = item.GetInstanceID();
    
    // 检查是否已初始化
    if (!_initializedItems.ContainsKey(instanceId))
    {
        // 首次处理：完整初始化
        InitializeItemFields(item);
        ModExtensionsManager.Instance.RefreshItemCache(item);
        _initializedItems[instanceId] = true;
        
        Debug.Log($"首次初始化物品: {item.DisplayName}");
    }
    else
    {
        // 已处理过：只检查更新
        if (CheckDataChanged(item))
        {
            UpdateItemFields(item);
            ModExtensionsManager.Instance.RefreshItemCache(item);
        }
        // 无变化：使用缓存，零开销
    }
}
```

## 🛡️ 错误处理

### 健壮的刷新逻辑
```csharp
private void SafeRefresh(Item item, string operation)
{
    try
    {
        Debug.Log($"开始{operation}: {item?.DisplayName ?? "null"}");
        
        if (item == null)
        {
            Debug.LogWarning($"{operation}: 物品为null");
            return;
        }
        
        // 尝试标准刷新
        try
        {
            ModExtensionsManager.Instance.RefreshItemCache(item);
            Debug.Log($"{operation}: 标准刷新成功");
            return;
        }
        catch (System.Exception ex1)
        {
            Debug.LogWarning($"{operation}标准刷新失败: {ex1.Message}");
        }
        
        // 备用方案1：仅更新缓存
        try
        {
            ModExtensionsManager.Instance.RefreshCacheOnly(item);
            ModExtensionsUIRefresher.RequestUIRefresh(item);
            Debug.Log($"{operation}: 备用方案1成功");
            return;
        }
        catch (System.Exception ex2)
        {
            Debug.LogWarning($"{operation}备用方案1失败: {ex2.Message}");
        }
        
        // 备用方案2：强制重新扫描
        try
        {
            ModExtensionsManager.Instance.GetExtensionsByPosition(
                item, "Top1", forceRescan: true);
            Debug.Log($"{operation}: 备用方案2成功");
        }
        catch (System.Exception ex3)
        {
            Debug.LogError($"{operation}所有方案失败: {ex3.Message}");
        }
    }
    catch (System.Exception ex)
    {
        Debug.LogError($"{operation}异常: {ex.Message}\n{ex.StackTrace}");
    }
}
```

### 缓存有效性检查
```csharp
private bool IsCacheValid(Item item, string position)
{
    try
    {
        // 检查框架是否加载
        if (ModExtensionsManager.Instance == null)
            return false;
            
        // 检查是否有扩展内容
        if (!ModExtensionsManager.Instance.HasExtensionsAtPosition(item, position))
            return false;
            
        return true;
    }
    catch
    {
        return false;
    }
}
```

### 优雅降级策略
```csharp
private void DisplayInfo(Item item)
{
    // 方案1：使用ModExtensions框架（最优）
    if (IsFrameworkAvailable())
    {
        item.Variables.SetString($"{PREFIX}Top1_信息", GetFormattedInfo());
        ModExtensionsManager.Instance.RefreshItemCache(item);
        return;
    }
    
    // 方案2：直接修改物品名称（降级方案）
    if (IsFallbackAllowed())
    {
        string originalName = item.DisplayName;
        item.SetString("DisplayName", $"{originalName} [{GetSimpleInfo()}]");
        return;
    }
    
    // 方案3：输出到控制台（最低方案）
    Debug.Log($"{item.DisplayName}: {GetSimpleInfo()}");
}
```

## ⚡ 性能调优

### 缓存策略配置
```csharp
// 在Mod初始化时配置
private void ConfigureCache()
{
    // 检查缓存统计
    string stats = ModExtensionsManager.Instance.GetCacheStats();
    Debug.Log($"初始缓存状态: {stats}");
    
    // 根据物品数量调整策略
    var allItems = UnityEngine.Object.FindObjectsOfType<Item>();
    if (allItems.Length > 100)
    {
        // 大量物品：更积极的缓存清理
        InvokeRepeating("CleanupOldCache", 300f, 300f); // 每5分钟清理一次
    }
}

private void CleanupOldCache()
{
    // 清理统计
    string before = ModExtensionsManager.Instance.GetCacheStats();
    ModExtensionsManager.Instance.ForceRefreshAll();
    string after = ModExtensionsManager.Instance.GetCacheStats();
    Debug.Log($"缓存清理: {before} -> {after}");
}
```

### 内存使用监控
```csharp
private void MonitorMemoryUsage()
{
    // 记录处理的物品数量
    int processedCount = _processedItems.Count;
    
    // 估算内存占用（每个物品约1-2KB）
    long estimatedMemory = processedCount * 1500; // 字节
    
    // 定期清理
    if (estimatedMemory > 10 * 1024 * 1024) // 超过10MB
    {
        Debug.LogWarning($"内存占用过高: {estimatedMemory / 1024}KB，清理旧缓存");
        CleanupOldItems();
    }
}

private void CleanupOldItems()
{
    // 清理超过10分钟未访问的物品
    float now = Time.time;
    var toRemove = new List<int>();
    
    foreach (var entry in _lastAccessTime)
    {
        if (now - entry.Value > 600f) // 10分钟
        {
            toRemove.Add(entry.Key);
        }
    }
    
    foreach (int id in toRemove)
    {
        _processedItems.Remove(id);
        _lastAccessTime.Remove(id);
    }
    
    Debug.Log($"清理了 {toRemove.Count} 个旧物品记录");
}
```

### 性能分析工具
```csharp
[System.Serializable]
public class PerformanceMetrics
{
    public int TotalRefreshes;
    public int CacheHits;
    public int CacheMisses;
    public float AverageRefreshTime;
    public List<float> RefreshTimes = new List<float>();
}

private PerformanceMetrics _metrics = new PerformanceMetrics();

private void RefreshWithMetrics(Item item)
{
    System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
    stopwatch.Start();
    
    ModExtensionsManager.Instance.RefreshItemCache(item);
    
    stopwatch.Stop();
    
    // 记录指标
    _metrics.TotalRefreshes++;
    _metrics.RefreshTimes.Add((float)stopwatch.Elapsed.TotalMilliseconds);
    
    // 计算平均值（只保留最近100次）
    if (_metrics.RefreshTimes.Count > 100)
        _metrics.RefreshTimes.RemoveAt(0);
        
    _metrics.AverageRefreshTime = _metrics.RefreshTimes.Average();
    
    // 定期输出报告
    if (_metrics.TotalRefreshes % 50 == 0)
    {
        Debug.Log($"性能报告: 总刷新{_metrics.TotalRefreshes}次, " +
                 $"平均耗时{_metrics.AverageRefreshTime:F2}ms, " +
                 $"缓存命中率{(_metrics.CacheHits * 100f / _metrics.TotalRefreshes):F1}%");
    }
}
```

## 📊 API速查表

### 快速选择指南
| 场景 | 推荐API | 原因 |
|------|---------|------|
| **用户交互后立即显示** | `RefreshItemCache(item)` | 即时反馈，简单可靠 |
| **批量更新多个字段** | `RefreshCacheOnly()` + `RequestUIRefresh()` | 性能最优，减少UI刷新 |
| **只更新数据不刷新UI** | `RefreshCacheOnly(item)` | 后台更新，稍后显示 |
| **强制清除缓存重新加载** | `GetExtensionsByPosition(forceRescan:true)` | 数据源变化时使用 |
| **清理本Mod所有数据** | `RefreshCacheByPrefix(prefix)` | Mod卸载或重置时 |
| **调试和性能分析** | `GetCacheStats()` | 监控缓存状态 |

### 兼容性说明
- **新API**：`RefreshCacheOnly()`, `RefreshItemCache(item, bool)`
- **旧API**：`RefreshItemCache(item)` 保持兼容，等价于 `RefreshItemCache(item, true)`
- **所有API** 都支持向后兼容，新版本不会破坏现有Mod

---

## 🆘 故障排除

### 常见问题
1. **字段不显示**
   - 检查字段命名格式：`前缀_位置_描述`
   - 检查是否调用了刷新API
   - 查看控制台是否有错误信息

2. **刷新延迟**
   - 首次刷新有延迟是正常的（缓存未命中）
   - 后续刷新应该立即显示（缓存命中）
   - 使用 `GetCacheStats()` 检查缓存状态

3. **多Mod冲突**
   - 确保使用独特的前缀
   - 使用 `RefreshCacheByPrefix()` 清理自己的字段
   - 避免修改其他Mod的字段

4. **性能问题**
   - 减少不必要的刷新调用
   - 使用 `RefreshCacheOnly()` 批量更新
   - 监控缓存命中率

### 调试技巧
```csharp
// 启用详细日志
private bool _enableDebugLogs = true;

private void DebugLog(string message)
{
    if (_enableDebugLogs)
        Debug.Log($"[{Time.frameCount}] {message}");
}

// 在关键操作处添加日志
DebugLog($"开始处理物品: {item.DisplayName}");
DebugLog($"字段数量: {item.Variables.Count}");
DebugLog($"缓存状态: {ModExtensionsManager.Instance.GetCacheStats()}");
```

---

**📞 需要更多帮助？** 查看 [实战示例代码](../DemoModExtension.md) 或提交 Issue。
