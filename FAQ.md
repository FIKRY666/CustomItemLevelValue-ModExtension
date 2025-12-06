# 🔧 常见问题解答 (FAQ)

## 📖 目录
- [安装与配置](#安装与配置)
- [开发问题](#开发问题)
- [性能问题](#性能问题)
- [显示问题](#显示问题)
- [兼容性问题](#兼容性问题)
- [调试与故障排除](#调试与故障排除)

---

## 🛠️ 安装与配置

### Q1: ModExtensions框架需要什么前置条件？
**A:** 需要以下条件：
1. 《逃离鸭科夫》游戏本体（最新版本）
2. **[CustomItemLevelValue 主Mod](https://steamcommunity.com/sharedfiles/filedetails/?id=3612733981)**（必须启用）
3. .NET Standard 2.1 开发环境（如果自己编译）

**验证方法：**
```csharp
// 在Start()方法中添加验证
void Start()
{
    var frameworkType = System.Type.GetType("CustomItemLevelValue.Core.ModExtensionsManager, CustomItemLevelValue");
    if (frameworkType == null)
    {
        Debug.LogError("❌ 未找到ModExtensions框架！请确保主Mod已正确安装并启用。");
    }
    else
    {
        Debug.Log("✅ ModExtensions框架已加载");
    }
}
```

### Q2: 如何检查我的Mod是否正确加载？
**A:** 查看游戏日志文件：
1. 启动游戏，打开控制台（F1或~键）
2. 查看是否有类似日志：
   ```
   [YourMod] Mod已成功加载
   [ModExtensions] 初始化完成 v1.0.5
   ```
3. 如果看不到日志，检查：
   - Mod文件夹位置：`Duckov_Data/Mods/YourModName/`
   - 文件结构：必须有 `YourMod.dll`, `info.ini`, `preview.png`
   - info.ini 配置正确

### Q3: 为什么我的Mod没有显示在游戏Mod列表中？
**A:** 常见原因和解决方案：

| 问题 | 解决方案 |
|------|----------|
| **文件夹位置错误** | 确保在 `Duckov_Data/Mods/` 下，而不是子文件夹 |
| **缺少info.ini** | 创建info.ini文件，内容：<br>`name=YourModName`<br>`displayName=你的Mod名称`<br>`description=描述` |
| **DLL文件损坏** | 重新编译或下载DLL文件 |
| **游戏版本不兼容** | 确保Mod针对正确的游戏版本编译 |

---

## 💻 开发问题

### Q4: 字段写入后为什么没有显示？
**A:** 按照以下步骤排查：

```csharp
// 1. 确认字段名格式正确
private const string PREFIX = "Test_"; // 必须有下划线结尾
item.Variables.SetString($"{PREFIX}Top1_状态", "测试"); // 位置必须正确

// 2. 确认调用了刷新
ModExtensionsManager.Instance.RefreshItemCache(item);

// 3. 添加调试日志
Debug.Log($"✅ 已写入字段: {PREFIX}Top1_状态");
Debug.Log($"🔄 已触发刷新: {item.DisplayName}");

// 4. 检查字段是否真的被写入
string value = item.Variables.GetString($"{PREFIX}Top1_状态", "未找到");
Debug.Log($"🔍 字段值: {value}");
```

### Q5: 如何选择正确的显示位置？
**A:** 五个位置的特点和选择建议：

| 位置 | 最佳用途 | 示例 | 注意事项 |
|------|----------|------|----------|
| **Top1** | 最紧急/重要的信息 | 警告、状态、等级 | 紧接稀有度显示，最显眼 |
| **Top2** | 数值类信息 | 价格、评分、需求 | 在价值信息后，适合数据展示 |
| **Top3** | 详细说明/建议 | 装备适配、使用建议 | 在核心属性后，空间较充足 |
| **Bottom1** | 背景/故事信息 | 来源、历史、任务 | 在描述后，适合长文本 |
| **Bottom2** | 补充提示信息 | 维护建议、使用技巧 | 最后显示，最不显眼 |

**选择原则：**
- 紧急信息 → **Top1**
- 数据信息 → **Top2**
- 建议信息 → **Top3**
- 故事信息 → **Bottom1**
- 提示信息 → **Bottom2**

### Q6: 如何实现动态更新（如实时价格）？
**A:** 两种方案：

**方案1：定时更新（推荐）**
```csharp
private float _updateInterval = 5f; // 5秒更新一次
private float _timer;

void Update()
{
    _timer += Time.deltaTime;
    if (_timer >= _updateInterval)
    {
        _timer = 0f;
        
        // 高性能更新：只更新缓存，最后统一刷新UI
        UpdateAllItemsCacheOnly();
        
        // 如果当前有物品悬停，刷新其UI
        if (_lastHoveredItem != null)
        {
            ModExtensionsUIRefresher.RequestUIRefresh(_lastHoveredItem);
        }
    }
}

void UpdateAllItemsCacheOnly()
{
    foreach (var item in _trackedItems)
    {
        item.Variables.SetString($"{PREFIX}Top2_价格", 获取价格(item));
        ModExtensionsManager.Instance.RefreshCacheOnly(item);
    }
}
```

**方案2：事件驱动更新**
```csharp
// 监听游戏事件
MarketSystem.OnPriceChanged += OnPriceChanged;

private void OnPriceChanged(int itemId, float newPrice)
{
    // 只更新特定物品
    var item = FindItemById(itemId);
    if (item != null)
    {
        item.Variables.SetString($"{PREFIX}Top2_价格", $"{newPrice}金币");
        ModExtensionsManager.Instance.RefreshItemCache(item);
    }
}
```

### Q7: 如何添加点击交互功能？
**A:** 目前框架主要支持显示，但可以通过以下方式实现简单交互：

```csharp
// 1. 在文本中添加提示
item.Variables.SetString($"{PREFIX}Top3_可交互",
    "[b][c=#FFAA00]点击展开详情[/c][/b]\n" +
    "当前状态: 折叠");

// 2. 监听按键事件
void Update()
{
    if (Input.GetKeyDown(KeyCode.E) && _lastHoveredItem != null)
    {
        ToggleDetails(_lastHoveredItem);
    }
}

void ToggleDetails(Item item)
{
    _showDetails = !_showDetails;
    item.Variables.SetString($"{PREFIX}Top3_可交互",
        _showDetails ? 获取详细内容() : 获取简要内容());
    ModExtensionsManager.Instance.RefreshItemCache(item);
}
```

---

## ⚡ 性能问题

### Q8: 我的Mod导致游戏卡顿，如何优化？
**A:** 性能优化检查清单：

✅ **已实施** | ❌ **需要改进** | 优化建议
--- | --- | ---
✅ | ❌ | **减少刷新频率**：每秒最多刷新1-2次，不是60次
✅ | ❌ | **使用缓存**：相同数据不要重复计算
✅ | ❌ | **批量更新**：先更新所有缓存，最后统一刷新UI
✅ | ❌ | **避免在Update中做复杂计算**：移到协程或定时器
✅ | ❌ | **清理无用字段**：Mod禁用时清理自己的字段

**性能测试代码：**
```csharp
private System.Diagnostics.Stopwatch _stopwatch = new System.Diagnostics.Stopwatch();

void TestPerformance()
{
    _stopwatch.Restart();
    
    // 你的逻辑...
    ModExtensionsManager.Instance.RefreshItemCache(item);
    
    _stopwatch.Stop();
    Debug.Log($"⏱️ 刷新耗时: {_stopwatch.ElapsedMilliseconds}ms");
    
    // 目标：< 10ms 为优秀，< 30ms 为可接受，> 50ms 需要优化
}
```

### Q9: 为什么第一次显示有延迟，后续就很快？
**A:** 这是正常现象，原因和解决方案：

**原因分析：**
1. **首次缓存未命中**：需要完整扫描物品字段
2. **反射调用开销**：第一次调用需要JIT编译
3. **UI元素创建**：首次需要创建文本对象

**优化建议：**
```csharp
// 方案1：预热缓存（游戏开始时）
IEnumerator PreloadCache()
{
    yield return new WaitForSeconds(5f); // 等待游戏稳定
    
    var commonItems = 获取常用物品列表();
    foreach (var item in commonItems)
    {
        // 预先扫描并缓存
        ModExtensionsManager.Instance.GetExtensionsByPosition(item, "Top1");
        yield return null; // 每帧处理一个，避免卡顿
    }
}

// 方案2：使用分离式API优化首次体验
private bool _isFirstTime = true;

void OnItemHovered(ItemHoveringUI ui, Item item)
{
    if (_isFirstTime)
    {
        // 首次：完整流程
        ApplyFieldsAndRefresh(item);
        _isFirstTime = false;
    }
    else
    {
        // 后续：使用缓存
        if (CheckIfDataChanged(item))
        {
            ApplyFieldsAndRefresh(item);
        }
        // 无变化：什么也不做，使用缓存
    }
}
```

### Q10: 如何监控Mod的性能表现？
**A:** 添加性能监控代码：

```csharp
public class PerformanceMonitor : MonoBehaviour
{
    private int _frameCount;
    private float _totalTime;
    private List<float> _refreshTimes = new List<float>();
    
    void Update()
    {
        _frameCount++;
        _totalTime += Time.deltaTime;
        
        // 每秒报告一次
        if (_totalTime >= 1f)
        {
            float fps = _frameCount / _totalTime;
            float avgRefreshTime = _refreshTimes.Count > 0 ? 
                _refreshTimes.Average() : 0;
                
            Debug.Log($"📊 性能报告: FPS={fps:F1}, 平均刷新={avgRefreshTime:F1}ms");
            
            _frameCount = 0;
            _totalTime = 0f;
            _refreshTimes.Clear();
        }
    }
    
    public void RecordRefreshTime(float milliseconds)
    {
        if (_refreshTimes.Count > 100) _refreshTimes.RemoveAt(0);
        _refreshTimes.Add(milliseconds);
    }
}

// 使用示例
void RefreshWithMonitor(Item item)
{
    var stopwatch = System.Diagnostics.Stopwatch.StartNew();
    
    ModExtensionsManager.Instance.RefreshItemCache(item);
    
    stopwatch.Stop();
    PerformanceMonitor.Instance.RecordRefreshTime(stopwatch.ElapsedMilliseconds);
}
```

---

## 🎨 显示问题

### Q11: 我的文本颜色/样式没有正确显示？
**A:** BBCode格式检查清单：

**常见错误：**
```csharp
// ❌ 错误：标签未闭合
"[c=#FF0000红色文字"  

// ❌ 错误：标签大小写不一致  
"[C=#FF0000]文字[/c]"  // C大写，c小写

// ❌ 错误：嵌套顺序错误
"[b][c=#FF0000]文字[/b][/c]"  // 应先闭合颜色标签

// ✅ 正确：
"[c=#FF0000]红色文字[/c]"
"[b][c=#FF0000]红色粗体[/c][/b]"
```

**调试方法：**
```csharp
void TestBBCode(Item item)
{
    string testText = "[c=#FF0000]测试[/c] [b]粗体[/b]";
    item.Variables.SetString($"{PREFIX}Test_BBCode", testText);
    
    // 检查实际存储的值
    string stored = item.Variables.GetString($"{PREFIX}Test_BBCode", "");
    Debug.Log($"📝 存储的BBCode: {stored}");
    
    ModExtensionsManager.Instance.RefreshItemCache(item);
}
```

### Q12: 为什么我的文本没有自动渐变效果？
**A:** 渐变效果只在使用框架的`ApplyHorizontalGradient`方法时生效。如果你直接写入纯文本，需要手动处理渐变：

**方案1：使用框架的渐变方法（推荐）**
```csharp
// 先获取渐变方法（通过反射）
private System.Reflection.MethodInfo _gradientMethod;

void Initialize()
{
    var demoModType = System.Type.GetType("DemoModExtension.ModBehaviour, DemoModExtension");
    if (demoModType != null)
    {
        _gradientMethod = demoModType.GetMethod("ApplyHorizontalGradient",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
    }
}

string ApplyGradient(string text, string startColor, string endColor)
{
    if (_gradientMethod != null)
    {
        var demoModInstance = FindObjectOfType(demoModType);
        return _gradientMethod.Invoke(demoModInstance, 
            new object[] { text, startColor, endColor, 15 }) as string;
    }
    return text; // 回退：无渐变
}
```

**方案2：手动实现简单渐变**
```csharp
string SimpleGradient(string text, string startColor, string endColor)
{
    if (text.Length <= 1) return $"[c={startColor}]{text}[/c]";
    
    var result = new System.Text.StringBuilder();
    for (int i = 0; i < text.Length; i++)
    {
        float t = (float)i / (text.Length - 1);
        // 简单线性插值
        string color = t < 0.5f ? startColor : endColor;
        result.Append($"[c={color}]{text[i]}[/c]");
    }
    return result.ToString();
}
```

### Q13: 文本太长被截断或换行不正常？
**A:** 文本长度和换行建议：

**最佳实践：**
```csharp
// 每行建议长度
const int MAX_LINE_LENGTH = 30; // 中文字符
const int MAX_LINES = 4;        // 最多4行

string OptimizeText(string content)
{
    // 1. 限制总长度
    if (content.Length > 120) 
        content = content.Substring(0, 117) + "...";
    
    // 2. 智能换行
    content = content.Replace("。", "。\n")
                     .Replace("，", "，\n")
                     .Replace("；", "；\n");
    
    // 3. 确保BBCode标签完整
    // 检查标签配对...
    
    return content;
}

// 使用示例
item.Variables.SetString($"{PREFIX}Bottom1_故事", 
    OptimizeText(获取背景故事()));
```

### Q14: 如何添加图标或特殊符号？
**A:** 支持的Unicode符号：

```csharp
// 常用图标符号
string GetStatusIcon(bool isActive)
{
    return isActive ? 
        "[c=#55FF55]✓[/c]" :  // 绿色勾
        "[c=#FF5555]✗[/c]";   // 红色叉
}

string GetStarRating(int stars)
{
    string filled = "[c=#FFD700]★[/c]";  // 金色实心星
    string empty = "[c=#666666]☆[/c]";   // 灰色空心星
    
    return new string('★', stars) + 
           new string('☆', 5 - stars);
}

// 组合使用
item.Variables.SetString($"{PREFIX}Top1_状态",
    $"{GetStatusIcon(true)} 可用 | {GetStarRating(4)}");
```

---

## 🔗 兼容性问题

### Q15: 如何避免与其他Mod的字段冲突？
**A:** 字段命名规范和冲突解决方案：

**强制规范：**
```csharp
// ✅ 正确格式：[前缀]_[位置]_[字段名]
private const string PREFIX = "UniqueMod_"; // 必须唯一！
// 示例: "RPG_Top1_等级", "Market_Top2_价格"

// ❌ 错误格式：
// "状态"                    // 缺少前缀和位置
// "Top1_Mod_状态"          // 位置在前
// "Mod_状态"               // 缺少位置

// 冲突检测工具
void CheckFieldConflicts(Item item)
{
    var allFields = 获取所有字段列表();
    var myFields = allFields.Where(f => f.StartsWith(PREFIX));
    
    foreach (var field in myFields)
    {
        // 检查是否有其他Mod使用相似字段名
        var conflict = allFields.FirstOrDefault(f => 
            f != field && 
            f.Replace(PREFIX, "").Equals(field.Replace(PREFIX, "")));
            
        if (conflict != null)
        {
            Debug.LogWarning($"⚠️ 字段可能冲突: {field} vs {conflict}");
            // 考虑修改前缀
        }
    }
}
```

### Q16: 我的Mod与某个特定Mod不兼容？
**A:** 不兼容排查流程：

1. **识别冲突Mod**
   ```csharp
   void DetectConflictingMods()
   {
       var allMods = ModManager.GetActiveMods();
       foreach (var mod in allMods)
       {
           Debug.Log($"🔍 已加载Mod: {mod.Name}");
       }
   }
   ```

2. **隔离测试**
   - 只启用你的Mod和主框架
   - 逐步添加其他Mod，观察问题出现时机
   - 记录导致冲突的Mod名称

3. **解决方案**
   ```csharp
   // 方案1：检测并避开冲突Mod
   bool HasConflictingMod()
   {
       return ModManager.GetActiveMods()
           .Any(m => m.Name == "冲突Mod名称");
   }
   
   void OnItemHovered(ItemHoveringUI ui, Item item)
   {
       if (HasConflictingMod())
       {
           // 简化显示或完全禁用
           Debug.LogWarning("⚠️ 检测到冲突Mod，使用简化模式");
           return;
       }
       
       // 正常逻辑...
   }
   
   // 方案2：字段名前缀添加标识
   private const string PREFIX = "MyMod_v2_"; // 添加版本标识
   ```

### Q17: 游戏更新后我的Mod失效了？
**A:** 游戏更新应对策略：

**立即措施：**
1. 检查游戏日志中的错误信息
2. 临时禁用Mod，等待更新
3. 在Mod说明中标注支持的游戏版本

**长期方案：**
```csharp
// 版本兼容性检查
void CheckGameVersion()
{
    string currentGameVersion = 获取游戏版本();
    string supportedVersion = "1.2.3"; // 你的Mod支持的版本
    
    if (!IsVersionCompatible(currentGameVersion, supportedVersion))
    {
        Debug.LogError($"❌ 游戏版本不兼容: {currentGameVersion}，需要 {supportedVersion}+");
        
        // 优雅降级或禁用功能
        item.Variables.SetString($"{PREFIX}Top1_警告",
            "[c=#FF5555]⚠️ Mod需要更新以适配当前游戏版本[/c]");
    }
}

bool IsVersionCompatible(string current, string required)
{
    // 简单版本检查逻辑
    var currentParts = current.Split('.');
    var requiredParts = required.Split('.');
    
    for (int i = 0; i < Math.Min(currentParts.Length, requiredParts.Length); i++)
    {
        int currentNum = int.Parse(currentParts[i]);
        int requiredNum = int.Parse(requiredParts[i]);
        
        if (currentNum < requiredNum) return false;
        if (currentNum > requiredNum) return true;
    }
    
    return true;
}
```

---

## 🐛 调试与故障排除

### Q18: 如何启用详细调试日志？
**A:** 框架和你的Mod的日志控制：

**框架日志（主Mod）：**
```csharp
// 在主Mod的ModBehaviour.cs中添加
void EnableDebugLogs()
{
    // 设置框架日志级别
    ModExtensionsUIRefresher.SetLogLevel(LogLevel.Debug);
    
    // 或者在游戏控制台输入指令
    // 需要实现控制台命令系统
}
```

**你的Mod日志：**
```csharp
public enum LogLevel { None, Error, Warning, Info, Debug, Verbose }
private LogLevel _currentLogLevel = LogLevel.Info;

void Log(string message, LogLevel level = LogLevel.Info)
{
    if (_currentLogLevel >= level)
    {
        Debug.Log($"[YourMod] {message}");
    }
}

// 运行时切换日志级别
void Update()
{
    if (Input.GetKeyDown(KeyCode.F6)) _currentLogLevel--;
    if (Input.GetKeyDown(KeyCode.F7)) _currentLogLevel++;
    
    if (Input.GetKeyDown(KeyCode.F8))
    {
        Log($"当前日志级别: {_currentLogLevel}", LogLevel.Info);
    }
}
```

### Q19: 遇到奇怪的bug如何收集信息？
**A:** 创建诊断报告：

```csharp
public string GenerateDiagnosticReport()
{
    var report = new System.Text.StringBuilder();
    report.AppendLine("=== ModExtensions 诊断报告 ===");
    report.AppendLine($"时间: {System.DateTime.Now}");
    report.AppendLine($"游戏版本: {Application.version}");
    
    // 框架状态
    var managerType = System.Type.GetType("CustomItemLevelValue.Core.ModExtensionsManager, CustomItemLevelValue");
    report.AppendLine($"框架加载: {(managerType != null ? "✅" : "❌")}");
    
    // 活动Mod
    report.AppendLine("活动Mod列表:");
    foreach (var mod in ModManager.GetActiveMods())
    {
        report.AppendLine($"  - {mod.Name}");
    }
    
    // 当前物品状态
    if (_lastHoveredItem != null)
    {
        report.AppendLine($"最后物品: {_lastHoveredItem.DisplayName}");
        report.AppendLine($"物品ID: {_lastHoveredItem.TypeID}");
        
        // 检查字段
        report.AppendLine("ModExtensions字段:");
        var fields = _lastHoveredItem.Variables
            .Where(v => v.Key.Contains("Top") || v.Key.Contains("Bottom"))
            .Select(v => $"{v.Key} = {v.GetString()?.Substring(0, Math.Min(50, v.GetString()?.Length ?? 0))}...");
        
        foreach (var field in fields)
        {
            report.AppendLine($"  - {field}");
        }
    }
    
    return report.ToString();
}

// 使用：按F9生成报告
void Update()
{
    if (Input.GetKeyDown(KeyCode.F9))
    {
        string report = GenerateDiagnosticReport();
        Debug.Log(report);
        
        // 保存到文件
        System.IO.File.WriteAllText("ModDiagnostic.txt", report);
    }
}
```

### Q20: 如何联系开发者获取帮助？
**A:** 提供完整的问题报告应包括：

**必须提供的信息：**
1. **游戏版本**：设置 → 关于中查看
2. **Mod版本**：你的Mod版本号
3. **重现步骤**：如何触发问题的详细步骤
4. **期望行为**：你期望看到什么
5. **实际行为**：实际看到了什么
6. **错误日志**：游戏控制台的完整错误信息
7. **其他Mod列表**：同时启用的其他Mod

**问题报告模板：**
```
【问题报告】
游戏版本: 1.2.3.456
Mod版本: 2.0.0
前置Mod: CustomItemLevelValue v2.1.0

问题描述:
[详细描述问题]

重现步骤:
1. 启动游戏，启用相关Mod
2. 进入游戏场景
3. 悬停某个特定物品
4. 观察问题出现

期望行为:
[应该显示什么]

实际行为:
[实际显示什么]

错误日志:
[复制完整的错误信息]

已尝试的解决方案:
1. 重启游戏 ❌
2. 重新安装Mod ❌
3. 只启用必需Mod ❌
```

---

## 🆘 紧急故障排除流程

### 问题：完全无法显示任何信息

**诊断步骤：**
```csharp
// 应急诊断代码
void EmergencyDiagnostic()
{
    Debug.Log("🚨 开始应急诊断...");
    
    // 1. 检查框架
    var framework = System.Type.GetType("CustomItemLevelValue.Core.ModExtensionsManager, CustomItemLevelValue");
    Debug.Log($"框架状态: {(framework != null ? "✅ 加载成功" : "❌ 未找到")}");
    
    // 2. 检查事件系统
    Debug.Log($"onSetupItem事件: {(ItemHoveringUI.onSetupItem != null ? "✅ 存在" : "❌ 空")}");
    
    // 3. 测试字段写入
    var testItem = FindAnyItem();
    if (testItem != null)
    {
        testItem.Variables.SetString("Test_Diagnostic", "测试");
        string value = testItem.Variables.GetString("Test_Diagnostic", "未找到");
        Debug.Log($"字段测试: {(value == "测试" ? "✅ 成功" : "❌ 失败")}");
    }
    
    // 4. 强制刷新测试
    if (testItem != null)
    {
        try
        {
            ModExtensionsManager.Instance.RefreshItemCache(testItem);
            Debug.Log("✅ 强制刷新成功");
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ 刷新失败: {ex.Message}");
        }
    }
    
    Debug.Log("🚨 应急诊断完成");
}
```

**快速解决方案：**
1. **重启游戏** - 最简单的解决方案
2. **检查加载顺序** - 确保主Mod在你的Mod之前加载
3. **清理缓存** - 删除 `Duckov_Data/Mods/` 下的缓存文件
4. **最小化测试** - 只启用主Mod和你的Mod测试

---

**💡 提示**：如果以上方案都无法解决问题，请提交完整的诊断报告到GitHub Issues，我们会尽快处理！

---

*最后更新：2025年12月*  
*如果本FAQ没有解决你的问题，请提交新的Issue或联系开发者。*
