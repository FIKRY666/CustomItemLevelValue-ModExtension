## 📄 DemoModExtension/ModBehaviour.cs（完整示例版）

```csharp
using Duckov.Modding;
using Duckov.UI;
using ItemStatsSystem;
using System.Collections.Generic;
using UnityEngine;

namespace DemoModExtension
{
    /// <summary>
    /// ModExtensions 演示Mod - 完整示例
    /// 演示如何在五个固定位置添加自定义信息
    /// </summary>
    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        // 🔧 配置区 - 根据需要修改
        private const string MOD_PREFIX = "Demo_";          // 你的Mod唯一前缀
        private const bool AUTO_CLEANUP = true;            // 禁用时自动清理字段
        private const bool DEBUG_MODE = false;             // 调试日志开关
        
        // 🏷️ 内部状态
        private bool _isActive = true;
        private HashSet<int> _processedInstances = new HashSet<int>();
        
        // ========== Unity生命周期 ==========
        
        private void Start()
        {
            Log("🎯 DemoModExtension 已加载");
            Log($"📌 前缀: {MOD_PREFIX}");
            Log($"🔄 自动清理: {AUTO_CLEANUP}");
            Log($"🐛 调试模式: {DEBUG_MODE}");
        }
        
        private void OnEnable()
        {
            // 注册物品悬停事件
            ItemHoveringUI.onSetupItem += OnItemHovered;
            Log("✅ 事件监听已注册");
        }
        
        private void OnDisable()
        {
            // 清理事件监听
            ItemHoveringUI.onSetupItem -= OnItemHovered;
            
            // 自动清理字段（如果启用）
            if (AUTO_CLEANUP)
            {
                CleanupOwnFields();
            }
            
            Log("🧹 Mod已禁用" + (AUTO_CLEANUP ? "，字段已清理" : ""));
        }
        
        private void OnDestroy()
        {
            _isActive = false;
            _processedInstances.Clear();
        }
        
        // ========== 核心逻辑 ==========
        
        /// <summary>
        /// 物品悬停事件处理
        /// </summary>
        private void OnItemHovered(ItemHoveringUI ui, Item item)
        {
            if (!_isActive || item == null || ui == null) return;
            
            int instanceId = item.GetInstanceID();
            
            // 检查是否已处理此实例
            if (_processedInstances.Contains(instanceId))
            {
                Log($"🔍 实例{instanceId}已处理，跳过");
                return;
            }
            
            _processedInstances.Add(instanceId);
            
            // 检查是否已有我们的字段
            if (HasDemoFields(item))
            {
                Log($"📋 物品已有Demo字段: {item.DisplayName}");
                return;
            }
            
            // 添加演示字段
            Log($"🛠️ 为物品添加演示字段: {item.DisplayName} (实例:{instanceId})");
            AddDemoExtensions(item);
        }
        
        /// <summary>
        /// 检查物品是否已有Demo字段
        /// </summary>
        private bool HasDemoFields(Item item)
        {
            if (item == null) return false;
            
            // 检查Variables
            foreach (var data in item.Variables)
            {
                if (data?.Key?.StartsWith(MOD_PREFIX) == true)
                {
                    Log($"✅ 确认已有字段: {data.Key}");
                    return true;
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// 为物品添加五个位置的演示字段
        /// </summary>
        private void AddDemoExtensions(Item item)
        {
            int instanceId = item.GetInstanceID();
            
            Log($"📝 开始为实例{instanceId}添加演示字段...");
            
            // 1. Top1 - 状态信息（稀有度后）
            item.Variables.SetString($"{MOD_PREFIX}Top1_状态", 
                "[c=#55FF55]✓ 可用[/c] | [c=#FFAA00]已充能[/c]");
            
            item.Variables.SetString($"{MOD_PREFIX}Top1_等级", 
                "等级: [c=#FFD700]★★★☆☆[/c] (3/5)");
            
            // 2. Top2 - 数值信息（价值后）
            item.Variables.SetString($"{MOD_PREFIX}Top2_需求", 
                "需求: [c=#FF5555]高涨[/c] (+15%)");
            
            item.Variables.SetString($"{MOD_PREFIX}Top2_趋势", 
                "趋势: [c=#55FFFF]↑ 上涨[/c]");
            
            // 3. Top3 - 功能信息（功能前）
            item.Variables.SetString($"{MOD_PREFIX}Top3_特效", 
                "[b]特殊效果[/b]: [c=#FF88FF]火焰[/c] + [c=#8888FF]冰冻[/c]");
            
            item.Variables.SetString($"{MOD_PREFIX}Top3_套装", 
                "套装: [c=#AAFFAA]演示套装 (2/4)[/c]");
            
            // 4. Bottom1 - 背景信息（描述后）
            item.Variables.SetString($"{MOD_PREFIX}Bottom1_来源", 
                "[hr][c=#AAAAAA]来源: 演示Mod制造[/c][hr]");
            
            item.Variables.SetString($"{MOD_PREFIX}Bottom1_背景", 
                "[c=#888888]此物品为ModExtensions框架演示用途。[/c]");
            
            // 5. Bottom2 - 提示信息（耐久度前）
            item.Variables.SetString($"{MOD_PREFIX}Bottom2_提示", 
                "⚠️ [b]演示提示[/b]: 此信息由DemoModExtension添加");
            
            item.Variables.SetString($"{MOD_PREFIX}Bottom2_维护", 
                "维护: [c=#FFFF55]建议定期检查[/c]");
            
            Log($"✅ 实例{instanceId}的演示字段添加完成");
        }
        
        // ========== 清理逻辑 ==========
        
        /// <summary>
        /// 清理本Mod创建的所有字段
        /// </summary>
        private void CleanupOwnFields()
        {
            Log($"🧹 开始清理{Demo字段...");
            
            try
            {
                // 方法1: 通过框架清理（推荐）
                var manager = GetModExtensionsManager();
                if (manager != null)
                {
                    manager.ClearCacheByPrefix(MOD_PREFIX);
                    manager.RemoveAllFieldsWithPrefix(MOD_PREFIX);
                    Log($"✅ 通过框架清理字段: {MOD_PREFIX}");
                    return;
                }
                
                // 方法2: 直接清理（备选）
                DirectCleanupFields();
            }
            catch (System.Exception ex)
            {
                LogWarning($"清理字段失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 直接清理物品字段（备选方案）
        /// </summary>
        private void DirectCleanupFields()
        {
            int fieldsRemoved = 0;
            var allItems = Object.FindObjectsOfType<Item>();
            
            Log($"🔍 扫描{allItems.Length}个物品进行清理...");
            
            foreach (var item in allItems)
            {
                if (item == null) continue;
                
                fieldsRemoved += RemoveFieldsFromCollection(item.Variables, MOD_PREFIX);
                fieldsRemoved += RemoveFieldsFromCollection(item.Constants, MOD_PREFIX);
            }
            
            Log($"🧹 直接清理完成: {fieldsRemoved}个字段");
        }
        
        private int RemoveFieldsFromCollection(CustomDataCollection collection, string prefix)
        {
            if (collection == null) return 0;
            
            int removed = 0;
            var itemsToRemove = new List<CustomData>();
            
            // 收集要删除的字段
            foreach (var data in collection)
            {
                if (data?.Key?.StartsWith(prefix) == true)
                {
                    itemsToRemove.Add(data);
                }
            }
            
            // 删除字段
            foreach (var item in itemsToRemove)
            {
                if (collection.Remove(item))
                    removed++;
            }
            
            return removed;
        }
        
        // ========== 工具方法 ==========
        
        /// <summary>
        /// 获取ModExtensionsManager实例
        /// </summary>
        private object GetModExtensionsManager()
        {
            try
            {
                var modExtensionsType = System.Type.GetType("CustomItemLevelValue.Core.ModExtensionsManager, CustomItemLevelValue");
                if (modExtensionsType != null)
                {
                    var instanceProperty = modExtensionsType.GetProperty("Instance");
                    return instanceProperty?.GetValue(null);
                }
            }
            catch { }
            
            return null;
        }
        
        /// <summary>
        /// 调试日志（受DEBUG_MODE控制）
        /// </summary>
        private void Log(string message)
        {
            if (DEBUG_MODE)
                Debug.Log($"[DemoMod] {message}");
        }
        
        private void LogWarning(string message)
        {
            Debug.LogWarning($"[DemoMod] ⚠️ {message}");
        }
        
        // ========== 调试功能 ==========
        
        private void Update()
        {
            // F10: 查看统计
            if (Input.GetKeyDown(KeyCode.F10))
            {
                int demoItemCount = 0;
                var allItems = Object.FindObjectsOfType<Item>();
                foreach (var item in allItems)
                {
                    if (HasDemoFields(item))
                        demoItemCount++;
                }
                Debug.Log($"[DemoMod] 📊 统计: {demoItemCount}个物品有Demo字段, {_processedInstances.Count}个实例已处理");
            }
            
            // F11: 强制清理
            if (Input.GetKeyDown(KeyCode.F11))
            {
                CleanupOwnFields();
                _processedInstances.Clear();
                Debug.Log($"[DemoMod] 🔄 强制清理完成，重置处理记录");
            }
            
            // F12: 重置处理记录（用于测试）
            if (Input.GetKeyDown(KeyCode.F12))
            {
                _processedInstances.Clear();
                Debug.Log($"[DemoMod] 🔄 已重置处理记录");
            }
        }
    }
}
```

## 📄 DemoModExtension/info.ini（配置文件）

```ini
name=DemoModExtension
displayName=ModExtensions演示Mod
description=演示CustomItemLevelValue的ModExtensions框架使用，包含五个位置的示例字段。
```

## 📄 DemoModExtension.csproj（项目文件）

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netstandard2.1</TargetFramework>
    <AssemblyName>DemoModExtension</AssemblyName>
    <RootNamespace>DemoModExtension</RootNamespace>
  </PropertyGroup>
  <ItemGroup>
    <Reference Include="$(DuckovPath)\Duckov_Data\Managed\*.dll" />
  </ItemGroup>
</Project>
```

## 📦 文件结构

```
DemoModExtension/
├── DemoModExtension.dll         # 编译后的Mod
├── info.ini                    # Mod信息文件
├── preview.png                 # 预览图（可选）
└── ModBehaviour.cs            # 主代码文件
```

## 🚀 使用说明

### 1. 基础配置
修改 `MOD_PREFIX` 为你的Mod唯一前缀，避免与其他Mod冲突。

### 2. 五个位置说明
框架提供五个固定位置，按此顺序显示：
1. **Top1** - 紧接在稀有度信息后
2. **Top2** - 在物品价值信息后  
3. **Top3** - 在功能属性信息前
4. **Bottom1** - 在描述文本后
5. **Bottom2** - 在耐久度信息前

### 3. 调试功能
- **F10**: 查看统计信息
- **F11**: 强制清理所有Demo字段
- **F12**: 重置处理记录（测试用）

### 4. 清理机制
启用 `AUTO_CLEANUP` 后，Mod禁用时会自动清理自己添加的字段。建议保持启用。

## 🔧 自定义字段示例

```csharp
// RPG类Mod示例
item.Variables.SetString($"{MOD_PREFIX}Top1_等级", 
    "等级: [c=#FFD700]Lv.42[/c]");
item.Variables.SetString($"{MOD_PREFIX}Top1_品质", 
    "[c=#FF88FF]史诗[/c]");

// 市场类Mod示例  
item.Variables.SetString($"{MOD_PREFIX}Top2_价格", 
    "均价: [c=#FFFF55]1,250[/c] 金币");
item.Variables.SetString($"{MOD_PREFIX}Top2_波动", 
    "24h: [c=#55FF55]+5.2%[/c]");

// 任务类Mod示例
item.Variables.SetString($"{MOD_PREFIX}Bottom1_任务", 
    "[c=#55FFFF]主线任务道具[/c]");
item.Variables.SetString($"{MOD_PREFIX}Bottom2_提示", 
    "⚠️ [b]任务物品[/b]: 无法交易");
```
