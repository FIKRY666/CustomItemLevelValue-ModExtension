# 🚀 ModExtensions 快速入门指南

本文档将帮助你在 **5分钟** 内创建第一个基于CustomItemLevelValue框架的扩展Mod。

## 📋 准备工作

### 1. 环境要求
- 《逃离鸭科夫》游戏本体
- [CustomItemLevelValue Mod](https://github.com/yourusername/CustomItemLevelValue) (v2.0+)
- Visual Studio 或任何C#开发环境
- .NET Standard 2.1 SDK

### 2. 项目引用
确保你的项目引用了以下DLL（位于游戏目录 `Duckov_Data/Managed/`）：
- `TeamSoda.Duckov.Core.dll`
- `TeamSoda.Duckov.Utilities.dll`
- `ItemStatsSystem.dll`
- `UnityEngine.dll` 和 `UnityEngine.*.dll`

## 🎯 5分钟创建第一个Mod

### 第1步：创建项目
```xml
<!-- MyFirstExtension.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netstandard2.1</TargetFramework>
    <AssemblyName>MyFirstExtension</AssemblyName>
    <RootNamespace>MyFirstExtension</RootNamespace>
  </PropertyGroup>
  
  <ItemGroup>
    <Reference Include="$(GamePath)\Duckov_Data\Managed\*.dll" />
  </ItemGroup>
</Project>
```

### 第2步：编写Mod代码
```csharp
// ModBehaviour.cs
using Duckov.Modding;
using Duckov.UI;
using ItemStatsSystem;
using UnityEngine;

namespace MyFirstExtension
{
    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        private void OnEnable()
        {
            // 监听物品悬停事件
            ItemHoveringUI.onSetupItem += OnItemHovered;
            Debug.Log("✅ 我的第一个扩展Mod已加载");
        }
        
        private void OnItemHovered(ItemHoveringUI ui, Item item)
        {
            if (item == null) return;
            
            // 在Top1位置添加问候信息
            item.Variables.SetString("MyMod_Top1_问候", 
                "[c=#55FF55]你好！这是你的第一个Mod扩展！[/c]");
                
            // 在Bottom2位置添加提示
            item.Variables.SetString("MyMod_Bottom2_提示",
                "💡 提示: 这个Mod正在运行中");
        }
        
        private void OnDisable()
        {
            ItemHoveringUI.onSetupItem -= OnItemHovered;
        }
    }
}
```

### 第3步：创建Mod配置文件
```ini
; info.ini
name=MyFirstExtension
displayName=我的第一个扩展Mod
description=学习ModExtensions框架的入门示例
```

### 第4步：编译和安装
```bash
# 编译项目
dotnet build --configuration Release

# 文件结构
# MyFirstExtension/
#   ├── MyFirstExtension.dll
#   ├── info.ini
#   └── preview.png (可选)
```

将文件夹复制到游戏目录：
```
Duckov_Data/Mods/MyFirstExtension/
```

### 第5步：测试运行
1. 启动游戏
2. 进入Mod管理界面
3. 启用你的Mod
4. 在游戏中悬停任何物品
5. 查看你添加的自定义信息！

## 📝 核心概念详解

### 字段命名规范
```
[Mod前缀]_[位置]_[字段名]
```

**示例:**
```
MyMod_Top1_状态
Quest_Top2_需求
Market_Bottom1_来源
```

### 五个显示位置
| 位置 | 显示时机 | 示例用途 |
|------|---------|----------|
| **Top1** | 稀有度显示后 | 物品状态、可用性 |
| **Top2** | 价值显示后 | 市场数据、趋势 |
| **Top3** | 属性显示前 | 特殊效果、套装 |
| **Bottom1** | 描述后 | 背景故事、来源 |
| **Bottom2** | 耐久度前 | 使用提示、维护 |

### 富文本语法速查
```csharp
// 基础颜色
"[c=#FF5555]红色[/c]"
"[c=#55FF55]绿色[/c]"
"[c=#5555FF]蓝色[/c]"

// 格式组合
"[b][c=#FFD700]金色粗体[/c][/b]"

// 分隔线
"[hr]"

// 图标+文字
"✓ [c=#55FF55]可用[/c]"
"⚠️ [c=#FFAA00]警告[/c]"
```

## 🎨 实用示例集

### 示例1：状态指示器
```csharp
item.Variables.SetString($"{PREFIX}Top1_状态", 
    "[c=#55FF55]✓ 可用[/c] | [c=#FFAA00]充能中[/c] | [c=#FF5555]✗ 损坏[/c]");
```

### 示例2：等级评分
```csharp
item.Variables.SetString($"{PREFIX}Top1_评分", 
    "评分: [c=#FFD700]★★★★☆[/c] (4.5/5.0)");
```

### 示例3：市场信息
```csharp
item.Variables.SetString($"{PREFIX}Top2_需求", 
    "需求: [c=#FF5555]高涨[/c] (+25%)");
item.Variables.SetString($"{PREFIX}Top2_趋势", 
    "趋势: [c=#55FFFF]↑ 上涨[/c]");
```

### 示例4：背景故事
```csharp
item.Variables.SetString($"{PREFIX}Bottom1_故事", 
    "[hr][c=#888888]这把武器制造于战争年代，见证了无数战斗。[/c][hr]");
```

### 示例5：实用提示
```csharp
item.Variables.SetString($"{PREFIX}Bottom2_提示", 
    "⚠️ [b]注意[/b]: 避免水浸，定期保养");
```

## 🔧 进阶技巧

### 条件性字段添加
```csharp
private void OnItemHovered(ItemHoveringUI ui, Item item)
{
    if (item == null) return;
    
    // 只为特定类型的物品添加字段
    if (item.DisplayName.Contains("枪"))
    {
        item.Variables.SetString($"{PREFIX}Top1_类型", 
            "[c=#FFAA00]🔫 武器类物品[/c]");
    }
    else if (item.DisplayName.Contains("药"))
    {
        item.Variables.SetString($"{PREFIX}Top1_类型", 
            "[c=#55FF55]💊 医疗类物品[/c]");
    }
}
```

### 性能优化
```csharp
private HashSet<int> _processedItems = new HashSet<int>();

private void OnItemHovered(ItemHoveringUI ui, Item item)
{
    if (item == null) return;
    
    // 避免重复处理同一物品
    if (_processedItems.Contains(item.TypeID))
        return;
    
    _processedItems.Add(item.TypeID);
    
    // 添加字段...
}
```

### 动态内容生成
```csharp
private void AddDynamicInfo(Item item)
{
    // 根据物品属性生成动态内容
    int quality = item.Quality;
    string qualityText = quality switch
    {
        > 80 => "[c=#FFD700]极品[/c]",
        > 60 => "[c=#FFAA00]良好[/c]",
        > 40 => "[c=#55FF55]普通[/c]",
        _    => "[c=#AAAAAA]劣质[/c]"
    };
    
    item.Variables.SetString($"{PREFIX}Top1_品质", 
        $"品质: {qualityText}");
}
```

## 🐛 故障排除

### 问题1：字段不显示
**检查清单：**
1. ✅ Mod前缀是否正确？ (如 `MyMod_Top1_字段`)
2. ✅ 位置是否正确？ (`Top1`, `Top2`, `Top3`, `Bottom1`, `Bottom2`)
3. ✅ CustomItemLevelValue是否已加载且启用？
4. ✅ 游戏日志是否有错误信息？
5. ✅ 字段值是否非空？

### 问题2：显示位置错误
**可能原因：**
- 字段名位置部分拼写错误
- 使用了不支持的位置名称
- 与其他Mod的字段冲突

**解决方案：**
```csharp
// 正确
"MyMod_Top1_状态"
"MyMod_Bottom2_提示"

// 错误  
"MyMod_Top_状态"      // 缺少数字
"MyMod_Top1状态"      // 缺少下划线
"MyMod_状态"          // 缺少位置
```

### 问题3：游戏崩溃或报错
**调试步骤：**
1. 查看游戏日志文件
2. 添加try-catch异常处理
3. 简化字段内容测试
4. 检查DLL引用是否正确

```csharp
try
{
    item.Variables.SetString($"{PREFIX}Top1_测试", "内容");
}
catch (Exception ex)
{
    Debug.LogError($"添加字段失败: {ex.Message}");
}
```

## 📚 下一步学习

### 推荐阅读
1. **[完整API文档](./API-Reference.md)** - 详细的类和方法说明
2. **[最佳实践](./BestPractices.md)** - 开发规范和建议
3. **[演示Mod源码](./DemoModExtension.cs)** - 完整的功能演示
4. **[框架原理](./Architecture.md)** - 深入了解实现机制

### 进阶主题
- 🔗 与其他Mod交互
- 🎨 自定义颜色方案
- 📊 数据持久化存储
- 🌐 网络数据获取
- 🔧 Harmony补丁技术

### 社区资源
- GitHub仓库问题区
- Discord讨论频道
- 开发者Wiki文档
- 示例项目集合

---

## 🎉 恭喜！

你已经掌握了ModExtensions框架的基本用法。现在你可以：

1. ✅ 创建自己的扩展Mod
2. ✅ 在五个位置添加自定义信息
3. ✅ 使用富文本美化显示
4. ✅ 处理基本的错误情况

**开始创造吧！** 如果你创建了有趣的Mod，欢迎分享到社区！

---

*更多帮助？查看 [完整文档](../README.md) 或加入社区讨论。*
```
