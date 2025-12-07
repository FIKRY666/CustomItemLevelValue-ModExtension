# 🎮 CustomItemLevelValue - ModExtensions 框架

<div align="center">

[![GitHub License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![Version](https://img.shields.io/badge/version-2.0.0-brightgreen.svg)](https://github.com/FIKRY666/CustomItemLevelValue-ModExtension/releases)
[![Downloads](https://img.shields.io/github/downloads/FIKRY666/CustomItemLevelValue-ModExtension/total.svg?color=orange)](https://github.com/FIKRY666/CustomItemLevelValue-ModExtension/releases)
[![Open Issues](https://img.shields.io/github/issues/FIKRY666/CustomItemLevelValue-ModExtension.svg)](https://github.com/FIKRY666/CustomItemLevelValue-ModExtension/issues)
[![Stars](https://img.shields.io/github/stars/FIKRY666/CustomItemLevelValue-ModExtension.svg?style=social)](https://github.com/FIKRY666/CustomItemLevelValue-ModExtension/stargazers)

</div>

## 🚀 一句话介绍

**"三行代码，让任何Mod都能在《逃离鸭科夫》物品面板中优雅显示自定义信息"**

## ✨ 核心价值

传统Mod显示信息：UI混乱、性能低下、重复造轮子
你的Mod使用本框架：**布局统一、性能优化、开发简单**

## 🎯 GitHub特色玩法

### 🔄 双模式支持：你选哪个？

| 模式选择 | 适合人群 | 需要下载的文件 | 下载链接 |
|---------|---------|--------------|----------|
| 🔹 **模式1：便携API** | 新手、快速测试、不想引用DLL | `ModExtensionsAPI.cs` | [GitHub Releases - API](https://github.com/FIKRY666/CustomItemLevelValue-ModExtension/releases/tag/API) |
| 🔸 **模式2：直接引用** | 正式项目、性能要求高 | `CustomItemLevelValue.dll` | [Steam创意工坊订阅](https://steamcommunity.com/sharedfiles/filedetails/?id=3612733981) |
### 📥 快速获取文件


## ⚡ 两种模式，任你选择

### 模式1️⃣: 便携API（零依赖）

**适合人群**：想快速测试、不想引用DLL、新手Modder

```csharp
// 1. 下载这个文件到你的项目:
// ModExtensionsAPI.cs (从Releases页面获取)

// 2. 在代码中使用：
using UnityEngine;

public class YourMod : MonoBehaviour
{
    private const string MOD_PREFIX = "YourMod_";
    
    void Start()
    {
        // 初始化API（自动连接主框架）
        ModExtensionsAPI.Init();
        
        // 注册你的Mod
        ModExtensionsAPI.RegisterMod(MOD_PREFIX);
    }
    
    void OnItemHovered(Item item)
    {
        // 写入字段
        item.Variables.SetString($"{MOD_PREFIX}Top1_状态", 
            "[color=green]✓ 运行正常[/color]");
        
        // 刷新显示（三行代码完成！）
        ModExtensionsAPI.RefreshItemCache(item);
    }
}
```

### 模式2️⃣: 直接引用（高性能）

**适合人群**：正式项目、性能要求高、老手Modder

```csharp
// 1. 引用这个DLL到你的项目:
// CustomItemLevelValue.dll (从Releases页面下载)

// 2. 在代码中使用：
using CustomItemLevelValue.Core;

public class YourMod : MonoBehaviour
{
    private const string MOD_PREFIX = "YourMod_";
    
    void Start()
    {
        // 直接访问管理器
        ModExtensionsManager.Instance.RegisterMod(MOD_PREFIX);
    }
    
    void OnItemHovered(Item item)
    {
        // 写入字段
        item.Variables.SetString($"{MOD_PREFIX}Top1_状态", 
            "[color=green]✓ 运行正常[/color]");
        
        // 刷新显示（三行代码完成！）
        ModExtensionsManager.Instance.RefreshItemCache(item);
    }
}
```

## 📍 五个显示位置速查表

| 位置 | 显示时机 | 推荐用途 | 示例 |
|------|----------|----------|------|
| **Top1** | 稀有度后 | 状态信息 | `[color=green]✓ 可用[/color]` |
| **Top2** | 价值后 | 市场数据 | `价格: [color=yellow]1,250金币[/color]` |
| **Top3** | 属性后 | 评分建议 | `评分: ★★★★☆` |
| **Bottom1** | 描述后 | 背景故事 | `拥有300年历史...` |
| **Bottom2** | 耐久前 | 使用提示 | `预计剩余: 战斗15次` |

## 🎨 富文本样式速查

```csharp
// 基础颜色
"[color=green]绿色 - 成功/可用[/color]"
"[color=red]红色 - 危险/错误[/color]"  
"[color=yellow]黄色 - 警告/注意[/color]"
"[color=orange]橙色 - 重要信息[/color]"

// 文本样式
"[b]粗体标题[/b]"              // 加粗
"[i]斜体备注[/i]"              // 斜体

// 组合使用
"[b][color=yellow]重要:[/color][/b] 请及时处理"
```

## 📦 安装四步曲

### 第一步：下载框架
```bash
# 选择你的模式：
🔹 模式1用户: 下载 ModExtensionsAPI.cs
🔸 模式2用户: 下载 CustomItemLevelValue.dll
```

### 第二步：安装主Mod
1. 订阅 [Steam创意工坊](https://steamcommunity.com/sharedfiles/filedetails/?id=3612733981)
2. 或下载 [GitHub Release](https://github.com/FIKRY666/CustomItemLevelValue-ModExtension/releases)


### 第三步：创建你的Mod项目
```xml
<!-- YourMod.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netstandard2.1</TargetFramework>
    <AssemblyName>YourMod</AssemblyName>
  </PropertyGroup>
  
  <!-- 模式2用户添加这个： -->
  <ItemGroup>
    <Reference Include="CustomItemLevelValue.dll" />
  </ItemGroup>
</Project>
```

### 第四步：测试运行
```csharp
// 完整的测试代码
public class TestMod : Duckov.Modding.ModBehaviour
{
    private const string PREFIX = "Test_";
    
    void OnEnable()
    {
        ItemHoveringUI.onSetupItem += OnItemHovered;
        Debug.Log("✅ Mod已启用");
    }
    
    void OnItemHovered(ItemHoveringUI ui, Item item)
    {
        item.Variables.SetString($"{PREFIX}Top1_测试", 
            "[color=green]🎉 框架工作正常！[/color]");
        
        // 模式1用户用这个：
        ModExtensionsAPI.RefreshItemCache(item);
        
        // 模式2用户用这个：
        // ModExtensionsManager.Instance.RefreshItemCache(item);
    }
}
```

## 🐛 常见问题

### ❓ 哪个模式适合我？
- **选模式1**：如果你是新手、想快速测试、项目简单
- **选模式2**：如果你是老手、需要高性能、项目复杂

### ❓ 字段不显示？
1. 检查前缀格式：`YourMod_Top1_字段名`
2. 确认调用了刷新API
3. 查看游戏日志：按F8打开控制台

### ❓ 框架没加载？
```csharp
// 添加检查代码
void Start()
{
    if (!ModExtensionsAPI.IsAvailable()) // 模式1
    // 或 if (ModExtensionsManager.Instance == null) // 模式2
    {
        Debug.LogError("❌ 框架未加载！请安装主Mod");
    }
}
```

## 📁 文件结构速览

```
CustomItemLevelValue-ModExtension/
├── 📄 README.md                    # 本文件
├── 📁 Releases/                    # 发布文件
│   ├── CustomItemLevelValue.dll    # 模式2：完整框架
│   └── ModExtensionsAPI.cs         # 模式1：便携API
├── 📁 Demo/                        # 示例代码
│   ├── DemoModAPI.cs              # 模式1示例
│   └── DemoModDLL.cs              # 模式2示例
└── 📁 Docs/                        # 文档
    ├── API-Reference.md           # API文档
    └── FAQ.md                     # 常见问题
```

## 🎮 实战示例

### 示例1：物品状态显示
```csharp
void ShowItemStatus(Item item)
{
    // 写入状态信息
    string status = item.Durability < 0.3f 
        ? "[color=red]⚠️ 即将损坏[/color]" 
        : "[color=green]✓ 状态良好[/color]";
    
    item.Variables.SetString($"{PREFIX}Top1_状态", status);
    
    // 刷新显示
    ModExtensionsAPI.RefreshItemCache(item); // 模式1
    // 或 ModExtensionsManager.Instance.RefreshItemCache(item); // 模式2
}
```

### 示例2：市场价显示
```csharp
void ShowMarketPrice(Item item)
{
    float price = CalculateMarketPrice(item);
    string priceText = $"[b]市场价:[/b] [color=yellow]{price:N0}金币[/color]";
    
    item.Variables.SetString($"{PREFIX}Top2_价格", priceText);
    RefreshDisplay(item); // 调用你的刷新方法
}
```


<div align="center">
    
## 📚 详细文档

- **[完整API参考](Docs/API-Reference.md)** - 查看所有API的详细说明和使用示例
- **[常见问题解答](FAQ.md)** - 遇到问题先看这里
- **[更新日志](CHANGELOG.md)** - 查看版本更新内容


</div>

## 🚀 开始你的Mod开发之旅！

选择你的模式 → 下载对应文件 → 编写三行代码 → 享受优雅的Mod信息显示！

**有问题？随时提交Issue！**

---
