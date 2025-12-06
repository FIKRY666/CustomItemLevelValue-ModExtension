


# CustomItemLevelValue - ModExtensions 框架

[![GitHub License](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/yourusername/CustomItemLevelValue/blob/main/LICENSE)
[![Version](https://img.shields.io/badge/version-2.0.0-green.svg)](https://github.com/yourusername/CustomItemLevelValue/releases)
[![Game](https://img.shields.io/badge/Game-逃离鸭科夫-red.svg)](https://store.steampowered.com/app/3167020/_/)

一个为《逃离鸭科夫》游戏设计的强大Mod框架，允许其他Mod在物品信息面板上添加自定义信息显示。

## ✨ 特性

### 🎯 五段式显示系统
框架预定义了五个固定位置，让你的Mod内容以标准化的方式显示：

| 位置 | 显示时机 | 用途建议 |
|------|---------|----------|
| **Top1** | 紧接在稀有度显示后 | 状态信息、等级评分 |
| **Top2** | 在物品价值显示后 | 市场需求、趋势数据 |
| **Top3** | 在核心属性显示前 | 特殊效果、套装信息 |
| **Bottom1** | 在物品描述后 | 背景故事、来源说明 |
| **Bottom2** | 在耐久度显示前 | 使用提示、维护建议 |

### 🎨 富文本支持
- **颜色标记**: `[c=#FF5555]红色文字[/c]`
- **粗体**: `[b]重要信息[/b]`
- **字号**: `[size=14]自定义大小[/size]`
- **分隔线**: `[hr]` 水平分隔线
- **完整BBCode语法支持**

### ⚡ 自动集成
- **颜色跟随**: 自动适配主Mod的颜色方案
- **字体调节**: 继承主Mod的字体设置
- **界面布局**: 无缝集成到物品信息面板
- **多语言**: 支持主Mod的语言切换

## 📦 快速开始

### 1. 前置要求
- 《逃离鸭科夫》游戏本体
- [CustomItemLevelValue Mod](https://steamcommunity.com/sharedfiles/filedetails/?id=3612733981) 

- .NET Standard 2.1开发环境

### 2. 创建你的第一个扩展Mod
```csharp
using Duckov.Modding;
using Duckov.UI;
using ItemStatsSystem;
using UnityEngine;

namespace YourModName
{
    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        private const string MOD_PREFIX = "YourPrefix_";
        
        private void OnEnable()
        {
            ItemHoveringUI.onSetupItem += OnItemHovered;
        }
        
        private void OnItemHovered(ItemHoveringUI ui, Item item)
        {
            if (item == null) return;
            
            // 在Top1位置添加信息
            item.Variables.SetString($"{MOD_PREFIX}Top1_状态", 
                "[c=#55FF55]✓ 可用[/c] | [c=#FFAA00]已充能[/c]");
                
            // 在Top2位置添加信息
            item.Variables.SetString($"{MOD_PREFIX}Top2_需求", 
                "需求: [c=#FF5555]高涨[/c] (+15%)");
        }
        
        private void OnDisable()
        {
            ItemHoveringUI.onSetupItem -= OnItemHovered;
        }
    }
}
```

### 3. 安装与测试
1. 将编译好的Mod放在 `Duckov_Data/Mods/YourModName/` 文件夹
2. 启动游戏，在Mod管理界面启用你的Mod
3. 悬停任何物品查看你的自定义信息

## 📖 详细文档

- **[快速入门指南](./QuickStart.md)** - 5分钟上手教程
- **[API参考](./API-Reference.md)** - 完整API文档
- **[示例Mod](./DemoModExtension.md)** - 完整演示代码

## 🔧 字段命名规范

### 基本格式
```
[Mod前缀]_[位置]_[字段名]
```

### 示例
```
// 正确
Demo_Top1_状态
MyMod_Top2_评分
Test_Bottom1_背景故事

// 错误
状态  // 缺少前缀和位置
Top1_Demo_状态  // 位置在前
Demo_状态  // 缺少位置
```

### 推荐前缀
- 使用简短、独特的Mod标识
- 建议包含下划线结尾
- 示例: `Demo_`, `Market_`, `Quest_`, `RPG_`

## 🎨 富文本语法

### 基础语法
```csharp
// 颜色
"[c=#FF5555]红色文字[/c]"

// 粗体
"[b]重要信息[/b]"

// 组合使用
"[b][c=#FFD700]金色粗体[/c][/b]"
```

### 颜色推荐
| 用途 | 颜色代码 | 示例 |
|------|----------|------|
| 正面效果 | `#55FF55` | ✓ 可用 |
| 负面效果 | `#FF5555` | ✗ 损坏 |
| 警告信息 | `#FFAA00` | ⚠️ 注意 |
| 重要数据 | `#FFD700` | ★★★★★ |
| 普通信息 | `#AAAAAA` | 常规说明 |

## ⚠️ 注意事项

### 性能优化
1. **避免频繁更新**: 只在物品首次悬停时写入字段
2. **缓存已处理物品**: 使用HashSet记录已添加字段的物品
3. **精简字段内容**: 避免过长的文本内容

### 兼容性
- 确保你的Mod在CustomItemLevelValue **之后**加载
- 字段名称避免与其他Mod冲突
- 卸载Mod时清理自己的字段

### 错误处理
```csharp
try
{
    item.Variables.SetString($"{PREFIX}Top1_状态", value);
}
catch (Exception ex)
{
    Debug.LogError($"写入字段失败: {ex.Message}");
}
```

## 🤝 贡献指南

1. Fork本仓库
2. 创建功能分支 (`git checkout -b feature/AmazingFeature`)
3. 提交更改 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 创建Pull Request

## 📄 许可证

本项目采用 MIT 许可证 - 查看 [LICENSE](LICENSE) 文件了解详情。

## 🙏 致谢

- 感谢《逃离鸭科夫》开发团队提供的优秀Mod系统
- 感谢所有社区贡献者的建议和反馈
- 特别感谢测试人员的宝贵意见

## 📞 支持与反馈
- [QQ群](https://qm.qq.com/q/c1uzZfNW8w) 
---

**开始为你的Mod添加酷炫的自定义显示吧！** 🚀
