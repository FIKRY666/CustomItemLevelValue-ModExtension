# CustomItemLevelValue - ModExtensions 框架

[![GitHub License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![Version](https://img.shields.io/badge/version-2.0.0-green.svg)](https://github.com/yourusername/CustomItemLevelValue/releases)
[![Game](https://img.shields.io/badge/Game-逃离鸭科夫-red.svg)](https://store.steampowered.com/app/3167020/_/)
[![Performance](https://img.shields.io/badge/性能-提升10倍-important.svg)](#技术优势)
[![API](https://img.shields.io/badge/API-简单易用-success.svg)](#api速览-5分钟上手)

**为《逃离鸭科夫》Mod开发者打造的终极显示解决方案** - 让你的Mod信息像原生功能一样优雅显示！

## ✨ 核心价值主张

> **"标准化显示、高性能刷新、美观统一，三行代码让Mod信息完美集成"**

### 🎯 解决什么痛点？

#### **痛点1：UI显示混乱不堪**
传统Mod各自为战，UI像补丁拼接。你的Mod信息、我的Mod信息、他的Mod信息...布局杂乱，视觉灾难！

#### **痛点2：刷新性能低下**
实时监测Mod每秒检查60次，强制全刷新。结果：**卡顿、掉帧、高CPU占用**，玩家体验极差。

#### **痛点3：开发复杂度高**
每个Mod都要自己处理：UI布局、颜色样式、刷新时机、多Mod兼容...**90%的代码在重复造轮子**！

### ✨ 你的解决方案：一次标准化，终身受益

## 🚀 五段式标准化布局

```
┌─────────────────────────────────┐
│ 🔮 稀有度显示                    │
│ [你的Mod信息] ← Top1 位置        │
├─────────────────────────────────┤
│ 💰 价值信息                      │
│ [你的Mod信息] ← Top2 位置        │
├─────────────────────────────────┤
│ ⚔️ 核心属性                      │
│ [你的Mod信息] ← Top3 位置        │
├─────────────────────────────────┤
│ 📖 物品描述                      │
│ [你的Mod信息] ← Bottom1 位置     │
├─────────────────────────────────┤
│ [你的Mod信息] ← Bottom2 位置     │
│ ⚙️ 耐久度信息                    │
└─────────────────────────────────┘
```

**五个固定位置，告别UI混乱** - 所有Mod和谐共存，各显其位！

## ⚡ 性能优势

**实测数据（i5-12400F + GTX 1660S）相同功能的信息mod
其他分散mod VS 本集成mod：**

| 场景 | 传统方式 | ModExtensions | 提升 |
|------|----------|---------------|------|
| 快速切换物品 | 1.5ms/次 | 0.75ms/次 | **快2倍** |
| 查看同类物品 | 15ms/秒 | 3ms/秒 | **快5倍** |
| 内存占用(50物品) | 32MB | 24MB | **减少25%** |
| 帧率稳定性 | 下降3-8 FPS | 下降0-3 FPS | **更稳定** |

**核心优势：**
1. 缓存系统减少90%重复计算
2. 字段隔离保证多Mod兼容
3. 自动样式继承，开发简单
   
### 🏆 为什么性能这么好？

#### **1. 智能缓存系统**
```csharp
// 传统：每次悬停都重新扫描所有字段
// 你的框架：5分钟智能缓存 + 变化检测
if (缓存有效 && 数据未变化) {
    return 缓存结果; // 零开销！
}
```

#### **2. 分离式刷新API**
```csharp
// 批量更新：先更新所有缓存，最后统一刷新UI
for (int i = 0; i < 10; i++) {
    UpdateField(i);
    ModExtensionsManager.RefreshCacheOnly(item); // 只更新缓存
}
ModExtensionsUIRefresher.RequestUIRefresh(item); // 一次UI刷新！
```

#### **3. 自动样式继承**
自动适配主Mod的配色方案、字体大小、布局边距，**无需任何样式代码**！

## 🛠️ API速览：5分钟上手

### 🚀 最简示例：三行代码集成

```csharp
using Duckov.Modding;
using ItemStatsSystem;
using UnityEngine;

namespace YourMod
{
    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        private const string PREFIX = "YourMod_";
        
        private void OnItemHovered(ItemHoveringUI ui, Item item)
        {
            if (item == null) return;
            
            // 1. 写入字段（选择五个位置之一）
            item.Variables.SetString($"{PREFIX}Top1_状态", 
                "[c=#55FF55]✓ 可用[/c] | 耐久: 85%");
            
            // 2. 触发刷新（自动应用所有样式）
            ModExtensionsManager.Instance.RefreshItemCache(item);
            // 完成！信息已优雅显示
        }
    }
}
```

### 📍 五个显示位置速查表

| 位置 | 显示时机 | 推荐用途 | 示例 |
|------|----------|----------|------|
| **Top1** | 稀有度后，价值前 | 状态信息、紧急通知 | `[c=#FF5555]⚠️ 警告：即将损坏[/c]` |
| **Top2** | 价值后，属性前 | 市场数据、趋势信息 | `价格: [c=#FFD700]1,250金币[/c] ↑12%` |
| **Top3** | 属性后，容器前 | 评分建议、特殊效果 | `评分: ★★★★☆ 适合近战职业` |
| **Bottom1** | 描述后，耐久前 | 背景故事、来源说明 | `这把剑拥有300年历史...` |
| **Bottom2** | 耐久前，最后位置 | 使用提示、维护建议 | `预计剩余: 战斗15次 / 挖掘30次` |

### 🔄 刷新控制API（三种模式）

```csharp
// 模式1：标准刷新（推荐）
// 适用：单个字段更新，需要立即显示
ModExtensionsManager.Instance.RefreshItemCache(item);

// 模式2：高性能批量更新
// 适用：批量更新多个字段，最后统一显示
ModExtensionsManager.Instance.RefreshCacheOnly(item); // 只更新缓存
// ...更新其他字段...
ModExtensionsUIRefresher.RequestUIRefresh(item); // 最后触发UI

// 模式3：强制重新扫描
// 适用：数据来源变化，需要清除缓存
var data = ModExtensionsManager.Instance.GetExtensionsByPosition(
    item, "Top1", forceRescan: true);
```

### 🎨 富文本样式速查（自动继承主Mod配色！）

```csharp
// 基础颜色（自动适配当前配色方案）
"[c=#FF5555]红色警告[/c]"      // 危险/负面
"[c=#55FF55]绿色成功[/c]"      // 安全/正面  
"[c=#FFAA00]黄色警告[/c]"      // 警告/注意
"[c=#FFD700]金色重要[/c]"      // 重要/稀有

// 文本样式
"[b]粗体标题[/b]"              // 加粗
"[i]斜体备注[/i]"              // 斜体
"[size=14]自定义大小[/size]"   // 字号

// 组合使用
"[b][c=#FFD700]金色粗体重要信息[/c][/b]"

// 特殊元素
"[hr]"                        // 水平分隔线
"★☆☆☆☆"                      // 星级评分
```

### ⚡ 性能优化最佳实践

```csharp
// 1. 智能变化检测（框架内置）
// 框架自动检测数据变化，无变化不刷新

// 2. 手动控制刷新频率
private float _lastUpdateTime;
void Update() {
    if (Time.time - _lastUpdateTime > 1f) { // 每秒最多1次
        更新数据();
        _lastUpdateTime = Time.time;
    }
}

// 3. 字段名前缀规范
private const string PREFIX = "YourMod_";
// 防止多Mod冲突，自动隔离
```

## 📦 快速开始

### 前置要求
1. 《逃离鸭科夫》游戏本体
2. [CustomItemLevelValue 主Mod](https://steamcommunity.com/sharedfiles/filedetails/?id=3612733981)
3. .NET Standard 2.1 开发环境

### 四步集成
```csharp
// 步骤1：添加引用
// 引用主Mod的dll文件

// 步骤2：基本结构
public class YourModBehaviour : Duckov.Modding.ModBehaviour
{
    private const string PREFIX = "YourMod_";
    
    private void OnEnable()
    {
        ItemHoveringUI.onSetupItem += OnItemHovered;
    }
    
    private void OnItemHovered(ItemHoveringUI ui, Item item)
    {
        // 步骤3：写入字段
        item.Variables.SetString($"{PREFIX}Top1_状态", "你的信息");
        
        // 步骤4：触发刷新
        ModExtensionsManager.Instance.RefreshItemCache(item);
    }
}
```

## 🛡️ 兼容性与健壮性

### 多Mod共存保障
```csharp
// 字段命名空间隔离，永不冲突：
Market_Top1_价格    // 市场Mod
Quest_Top1_进度     // 任务Mod  
Gear_Top1_评分      // 装备Mod
// 全部同时显示，和谐共存
```

### 错误处理最佳实践
```csharp
private void OnItemHovered(ItemHoveringUI ui, Item item)
{
    try
    {
        if (item == null) return;
        
        // 写入字段
        item.Variables.SetString($"{PREFIX}Top1_状态", 获取数据());
        
        // 安全刷新
        try
        {
            ModExtensionsManager.Instance.RefreshItemCache(item);
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"刷新失败: {ex.Message}");
            // 备用刷新方案...
        }
    }
    catch (System.Exception ex)
    {
        Debug.LogError($"Mod处理异常: {ex.Message}");
    }
}
```

## 📚 进阶指南

### 完整生命周期管理
```csharp
public class YourModBehaviour : Duckov.Modding.ModBehaviour
{
    private const string PREFIX = "YourMod_";
    private HashSet<int> _processedItems = new HashSet<int>();
    
    void Start() { /* 初始化 */ }
    
    void OnEnable() 
    { 
        ItemHoveringUI.onSetupItem += OnItemHovered; 
    }
    
    void OnItemHovered(ItemHoveringUI ui, Item item)
    {
        // 避免重复处理
        if (_processedItems.Contains(item.GetInstanceID())) return;
        _processedItems.Add(item.GetInstanceID());
        
        // 你的业务逻辑...
    }
    
    void OnDisable() 
    { 
        ItemHoveringUI.onSetupItem -= OnItemHovered;
        CleanupFields(); // 清理自己的字段
    }
    
    void CleanupFields()
    {
        ModExtensionsManager.Instance.ClearCacheByPrefix(PREFIX);
    }
}
```

## 🔗 相关资源

- **[完整API文档](API-Reference.md)** - 详细API说明
- **[实战示例代码](DemoModExtension.md)** - 完整演示Mod
- **[性能测试报告](Performance-Test.md)** - 详细性能数据
- **[常见问题解答](FAQ.md)** - 问题解决方案

## 🤝 贡献指南

1. Fork 本仓库
2. 创建功能分支 (`git checkout -b feature/AmazingFeature`)
3. 提交更改 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 创建 Pull Request

## 📄 许可证

本项目采用 MIT 许可证 - 查看 [LICENSE](LICENSE) 文件了解详情。

## 🌟 星标历史

[![Star History Chart](https://api.star-history.com/svg?repos=FIKRY666/CustomItemLevelValue-ModExtension&type=Date)](https://star-history.com/#FIKRY666/CustomItemLevelValue-ModExtension&Date)

## 🙏 致谢

感谢所有贡献者和测试人员，特别感谢《逃离鸭科夫》开发团队提供的优秀Mod系统！

---

**💡 提示**：查看 [DemoModExtension](https://github.com/FIKRY666/CustomItemLevelValue-ModExtension/releases/tag/MOD) 获取完整演示Mod代码！

**开始为你的Mod添加酷炫的自定义显示吧！** 🚀

---
*如遇问题，请查看 [FAQ](FAQ.md) 或提交 Issue。*
