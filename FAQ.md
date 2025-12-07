# Frequently Asked Questions (FAQ)

## 🔧 安装与配置

### Q: 如何选择使用哪种模式？
**A:** 
- **新手/快速测试** → 选择**模式1（便携API）**，只需要复制一个.cs文件
- **正式项目/性能要求高** → 选择**模式2（直接引用）**，需要引用DLL

### Q: 两种模式能混用吗？
**A:** 可以，但不推荐。建议一个项目内保持统一。

### Q: 主Mod在哪里下载？
**A:** 
- Steam创意工坊: [订阅链接](https://steamcommunity.com/sharedfiles/filedetails/?id=3612733981)
- GitHub Releases: [下载页面](https://github.com/FIKRY666/CustomItemLevelValue-ModExtension/releases)

## 🐛 常见问题

### Q: Mod加载了但字段不显示
**检查清单：**
1. ✅ 主Mod是否已安装并启用？
2. ✅ 字段名格式是否正确？`{前缀}_{位置}_{字段名}`
3. ✅ 是否调用了刷新API？`RefreshItemCache(item)`
4. ✅ 游戏日志是否有报错？（按F8查看）

### Q: 出现"类型未找到"错误
**A:** 
- **模式1用户**：确保下载了最新的`ModExtensionsAPI.cs`文件
- **模式2用户**：确保正确引用了`CustomItemLevelValue.dll`

### Q: 性能问题/游戏卡顿
**解决方法：**
1. 减少字段数量（每个位置1-2个字段）
2. 避免在Update()中每帧刷新
3. 使用`RefreshCacheOnly()`进行批量更新
4. 升级到模式2（直接引用）以获得更好性能

## 💻 开发问题

### Q: 字段应该放在哪个位置？
**A:** 参考下表选择：

### Q: 为什么我已经删除的字段还是会显示？
**A:** 测试阶段的缓存问题直接使用F11强制执行清除。

| 位置 | 显示顺序 | 推荐内容 |
|------|----------|----------|
| Top1 | 1 | 重要状态、警告信息 |
| Top2 | 2 | 数值信息、市场数据 |
| Top3 | 3 | 评分、建议、效果 |
| Bottom1 | 4 | 描述、背景、来源 |
| Bottom2 | 5 | 提示、说明、备注 |

### Q: 如何动态更新字段？
```csharp
// 示例：每秒更新一次
private float _updateTimer;

void Update()
{
    _updateTimer += Time.deltaTime;
    if (_updateTimer >= 1f && _currentItem != null)
    {
        UpdateField(_currentItem);
        RefreshItem(_currentItem);
        _updateTimer = 0f;
    }
}
```

### Q: 多个Mod会冲突吗？
**A:** 不会。框架使用前缀系统隔离：
- `YourMod_Top1_状态`
- `OtherMod_Top1_状态`
两个字段会同时显示，互不干扰。

## 🎨 样式与显示

### Q: 支持哪些富文本格式？
**A:** 支持BBCode格式：
- 颜色：`[color=#FF0000]文字[/color]`
- 粗体：`[b]粗体[/b]`
- 斜体：`[i]斜体[/i]`
- 简写颜色：`[c=green]绿色[/c]`

### Q: 如何添加图标/表情？
**A:** 使用Unicode字符：

## 🔄 更新与维护

### Q: 如何升级框架版本？
**A:** 
1. 备份你的Mod项目
2. 下载新版本文件
3. 替换旧的DLL或API文件
4. 重新编译测试

### Q: 旧版本存档兼容吗？
**A:** 框架会自动清理旧字段，但建议：
1. 先备份存档
2. 测试重要物品
3. 使用框架的清理工具：`CleanupEmptyExtensionsFields()`

## 🚨 错误代码

### "未找到ModExtensionsManager"
**可能原因：**
1. 主Mod未安装
2. 游戏版本不兼容
3. Mod加载顺序问题

**解决方案：**
```csharp
// 添加检查代码
void Start()
{
    StartCoroutine(CheckFrameworkLoaded());
}

IEnumerator CheckFrameworkLoaded()
{
    yield return new WaitForSeconds(2f);
    
    if (!ModExtensionsAPI.IsAvailable())
    {
        Debug.LogError("❌ 框架未加载！请检查：");
        Debug.LogError("1. 是否安装了主Mod？");
        Debug.LogError("2. 主Mod是否启用？");
        Debug.LogError("3. 游戏版本是否兼容？");
    }
}
```

### "字段写入失败"
**可能原因：**
1. 物品为null
2. Variables集合为null
3. 字段名包含非法字符

**解决方案：**
```csharp
void SafeWriteField(Item item, string key, string value)
{
    try
    {
        if (item == null || item.Variables == null)
        {
            Debug.LogWarning("物品或Variables为null");
            return;
        }
        
        if (string.IsNullOrEmpty(key))
        {
            Debug.LogWarning("字段键名为空");
            return;
        }
        
        item.Variables.SetString(key, value);
    }
    catch (System.Exception ex)
    {
        Debug.LogError($"写入字段失败: {ex.Message}");
    }
}
```

## 🤝 社区支持

### Q: 在哪里提问？
**A:** 
1. **GitHub Issues** - 技术问题、Bug报告
2. **Steam讨论区** - 使用问题、经验分享
3. **QQ群** - 979203137

### Q: 如何贡献代码？
**A:** 
1. Fork本仓库
2. 创建功能分支
3. 编写代码和测试
4. 提交Pull Request

## 📊 调试技巧

### 启用详细日志
```csharp
// 在Mod的Start方法中添加
void Start()
{
    #if DEBUG
    Debug.Log("[MyMod] 开始初始化...");
    
    // 测试框架连接
    bool frameworkLoaded = ModExtensionsAPI.Init();
    Debug.Log($"[MyMod] 框架加载: {frameworkLoaded}");
    
    // 获取缓存统计
    if (!frameworkLoaded)
    {
        string stats = ModExtensionsManager.Instance.GetCacheStats();
        Debug.Log($"[MyMod] 缓存统计: {stats}");
    }
    #endif
}
```

### 检查字段状态
```csharp
void DebugItemFields(Item item)
{
    if (item == null) return;
    
    Debug.Log($"=== 物品字段检查: {item.DisplayName} ===");
    
    // 检查Variables
    if (item.Variables != null)
    {
        foreach (var data in item.Variables)
        {
            if (data.Key.Contains("YourMod_"))
            {
                Debug.Log($"字段: {data.Key} = {data.GetString()}");
            }
        }
    }
}
```
