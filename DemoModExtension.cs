
# DemoModExtension.cs

```csharp
/*
 * DemoModExtension.cs
 * 
 * CustomItemLevelValue 框架 - 演示Mod
 * 
 * 这个文件演示了如何在CustomItemLevelValue框架中创建扩展Mod，
 * 为游戏物品添加自定义信息显示。
 * 
 * 使用说明:
 * 1. 将本文件编译为 DemoModExtension.dll
 * 2. 与 info.ini、preview.png 一起放入 Mods/DemoModExtension/ 文件夹
 * 3. 在游戏Mod界面中启用此Mod
 * 
 * 作者: YourName
 * 版本: 1.0.0
 * 日期: 2024
 */

using Duckov.Modding;
using Duckov.UI;
using ItemStatsSystem;
using System.Collections.Generic;
using UnityEngine;

namespace DemoModExtension
{
    /// <summary>
    /// ModExtensions 框架演示Mod
    /// 
    /// 这个Mod展示了如何使用CustomItemLevelValue的五段式显示系统，
    /// 在物品信息面板上添加自定义内容。
    /// 
    /// 五个预定义位置:
    /// 1. Top1 - 稀有度后 (状态信息)
    /// 2. Top2 - 价值后 (数值信息)
    /// 3. Top3 - 功能信息前 (特殊效果)
    /// 4. Bottom1 - 描述后 (背景故事)
    /// 5. Bottom2 - 耐久度前 (使用提示)
    /// </summary>
    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        // ========== 配置常量 ==========
        
        /// <summary>
        /// Mod唯一前缀
        /// 格式: [前缀]_[位置]_[字段名]
        /// 重要: 前缀必须唯一，避免与其他Mod冲突
        /// </summary>
        private const string MOD_PREFIX = "Demo_";
        
        /// <summary>
        /// 是否启用调试日志
        /// 开发时设为true，发布时设为false
        /// </summary>
        private const bool ENABLE_DEBUG = true;
        
        /// <summary>
        /// 调试快捷键
        /// F10: 显示统计信息
        /// F11: 重置处理记录
        /// F12: 手动清理字段
        /// </summary>
        private const KeyCode DEBUG_STATS_KEY = KeyCode.F10;
        private const KeyCode DEBUG_RESET_KEY = KeyCode.F11;
        private const KeyCode DEBUG_CLEANUP_KEY = KeyCode.F12;
        
        // ========== 状态变量 ==========
        
        /// <summary>
        /// 记录已处理物品的ID，避免重复添加字段
        /// 优化性能的关键：每个物品只处理一次
        /// </summary>
        private readonly HashSet<int> _processedItems = new HashSet<int>();
        
        /// <summary>
        /// Mod是否处于活动状态
        /// 用于优雅地处理Mod的启用/禁用
        /// </summary>
        private bool _isActive = true;
        
        // ========== Unity生命周期方法 ==========
        
        /// <summary>
        /// Mod启动时调用
        /// 进行一次性初始化操作
        /// </summary>
        private void Start()
        {
            Log("🎯 DemoModExtension 已加载");
            Log($"📌 Mod前缀: {MOD_PREFIX}");
            Log("📍 五个显示位置: Top1, Top2, Top3, Bottom1, Bottom2");
            Log("💡 提示: 悬停物品查看演示效果");
        }
        
        /// <summary>
        /// Mod启用时调用
        /// 注册事件监听器
        /// </summary>
        private void OnEnable()
        {
            // 注册物品悬停事件
            // 当玩家悬停在物品上时，此方法会被调用
            ItemHoveringUI.onSetupItem += OnItemHovered;
            
            Log("✅ 事件监听器已注册");
            Log("🔄 Mod已启用，等待物品悬停...");
        }
        
        /// <summary>
        /// Mod禁用时调用
        /// 清理事件监听器和字段数据
        /// 重要: 必须清理自己添加的字段，避免残留
        /// </summary>
        private void OnDisable()
        {
            // 取消事件注册
            ItemHoveringUI.onSetupItem -= OnItemHovered;
            
            // 清理本Mod添加的所有字段
            CleanupOwnFields();
            
            Log("🧹 Mod已禁用，字段已清理");
            Log("📊 统计: " + GetStatsString());
        }
        
        /// <summary>
        /// Mod销毁时调用
        /// 进行最终清理
        /// </summary>
        private void OnDestroy()
        {
            _isActive = false;
            Log("🗑️ Mod已销毁");
        }
        
        /// <summary>
        /// 每帧更新
        /// 处理调试快捷键
        /// </summary>
        private void Update()
        {
            HandleDebugInput();
        }
        
        // ========== 核心功能方法 ==========
        
        /// <summary>
        /// 物品悬停事件处理
        /// 
        /// 当玩家悬停在物品上时，为此物品添加演示字段
        /// 每个物品只处理一次，优化性能
        /// </summary>
        /// <param name="ui">物品悬停UI组件</param>
        /// <param name="item">被悬停的物品</param>
        private void OnItemHovered(ItemHoveringUI ui, Item item)
        {
            // 安全检查
            if (!_isActive || item == null || ui == null) return;
            
            // 避免重复处理同一物品（性能优化）
            if (_processedItems.Contains(item.TypeID))
                return;
            
            // 记录已处理物品
            _processedItems.Add(item.TypeID);
            
            Log($"🛠️ 为物品添加演示字段: {item.DisplayName} (ID: {item.TypeID})");
            
            // 为物品添加五个位置的演示内容
            AddDemoExtensions(item);
        }
        
        /// <summary>
        /// 为物品添加演示字段
        /// 
        /// 在五个预定义位置添加示例内容
        /// 你可以根据实际需求修改这些内容
        /// </summary>
        /// <param name="item">目标物品</param>
        private void AddDemoExtensions(Item item)
        {
            // ========== 位置1: Top1 (稀有度后) ==========
            // 用途: 显示物品的状态信息、等级评分等
            
            item.Variables.SetString($"{MOD_PREFIX}Top1_状态",
                "[c=#55FF55]✓ 可用状态[/c] | [c=#FFAA00]已充能[/c]");
            
            item.Variables.SetString($"{MOD_PREFIX}Top1_等级",
                "等级: [c=#FFD700]★★★☆☆[/c] (3/5)");
            
            // ========== 位置2: Top2 (价值后) ==========
            // 用途: 显示市场需求、交易趋势等数值信息
            
            item.Variables.SetString($"{MOD_PREFIX}Top2_评分",
                "评分: [c=#FFFF00]8.7[/c]/10.0");
            
            item.Variables.SetString($"{MOD_PREFIX}Top2_需求",
                "需求: [c=#FF5555]高涨[/c] (+15%)");
            
            // ========== 位置3: Top3 (功能信息前) ==========
            // 用途: 显示特殊效果、套装信息等功能性内容
            
            item.Variables.SetString($"{MOD_PREFIX}Top3_效果",
                "[b]特殊效果[/b]: [c=#55FFFF]冰冻[/c] + [c=#FFAA00]电击[/c]");
            
            item.Variables.SetString($"{MOD_PREFIX}Top3_套装",
                "套装: [c=#AAFFAA]演示套装 (2/4)[/c]");
            
            // ========== 位置4: Bottom1 (描述后) ==========
            // 用途: 显示背景故事、物品来源等描述性内容
            // 使用 [hr] 标签创建水平分隔线
            
            item.Variables.SetString($"{MOD_PREFIX}Bottom1_来源",
                "[hr][c=#AAAAAA]来源: 演示Mod添加[/c][hr]");
            
            item.Variables.SetString($"{MOD_PREFIX}Bottom1_背景",
                "[c=#888888]这把武器制造于2024年，采用先进复合材料。[/c]");
            
            // ========== 位置5: Bottom2 (耐久度前) ==========
            // 用途: 显示使用提示、维护建议等实用信息
            
            item.Variables.SetString($"{MOD_PREFIX}Bottom2_提示",
                "⚠️ [b]演示提示[/b]: 避免高温环境，定期维护");
            
            item.Variables.SetString($"{MOD_PREFIX}Bottom2_维护",
                "维护: [c=#FFFF55]建议每月检查[/c]");
            
            Log($"📝 已为 {item.DisplayName} 添加10个演示字段");
        }
        
        // ========== 清理与维护方法 ==========
        
        /// <summary>
        /// 清理本Mod创建的所有字段
        /// 
        /// 重要: 在Mod禁用时必须调用此方法
        /// 避免字段残留导致显示错误
        /// </summary>
        private void CleanupOwnFields()
        {
            try
            {
                Log("🧹 开始清理本Mod字段...");
                
                // 方法1: 尝试通过CustomItemLevelValue框架清理
                if (TryCleanupViaFramework())
                {
                    return; // 框架清理成功，直接返回
                }
                
                // 方法2: 直接清理（备选方案）
                DirectCleanupFields();
            }
            catch (System.Exception ex)
            {
                LogError($"清理字段失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 尝试通过CustomItemLevelValue框架清理字段
        /// 这是推荐的方式，效率更高
        /// </summary>
        private bool TryCleanupViaFramework()
        {
            // 反射获取CustomItemLevelValue的ModExtensionsManager
            var modExtensionsType = System.Type.GetType("CustomItemLevelValue.Core.ModExtensionsManager, CustomItemLevelValue");
            if (modExtensionsType == null)
            {
                Log("⚠️ 未找到CustomItemLevelValue框架，使用直接清理");
                return false;
            }
            
            // 获取Instance属性和清理方法
            var instanceProperty = modExtensionsType.GetProperty("Instance");
            var clearMethod = modExtensionsType.GetMethod("RemoveAllFieldsWithPrefix");
            
            if (instanceProperty == null || clearMethod == null)
            {
                Log("⚠️ 框架API不完整，使用直接清理");
                return false;
            }
            
            // 调用框架的清理方法
            var instance = instanceProperty.GetValue(null);
            clearMethod.Invoke(instance, new object[] { MOD_PREFIX });
            
            Log($"✅ 通过框架清理字段: {MOD_PREFIX}");
            return true;
        }
        
        /// <summary>
        /// 直接清理物品字段（备选方案）
        /// 在框架不可用时使用
        /// </summary>
        private void DirectCleanupFields()
        {
            int itemsProcessed = 0;
            int fieldsRemoved = 0;
            
            // 获取场景中所有物品
            var allItems = Object.FindObjectsOfType<Item>();
            Log($"🔍 扫描到 {allItems.Length} 个物品");
            
            foreach (var item in allItems)
            {
                if (item == null) continue;
                itemsProcessed++;
                
                // 清理Variables中的字段
                fieldsRemoved += RemoveFieldsFromCollection(item.Variables, MOD_PREFIX);
                
                // 清理Constants中的字段
                fieldsRemoved += RemoveFieldsFromCollection(item.Constants, MOD_PREFIX);
            }
            
            Log($"🧹 直接清理完成: 扫描{itemsProcessed}物品, 移除{fieldsRemoved}字段");
        }
        
        /// <summary>
        /// 从数据集合中移除指定前缀的字段
        /// </summary>
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
        
        // ========== 调试与工具方法 ==========
        
        /// <summary>
        /// 处理调试快捷键输入
        /// </summary>
        private void HandleDebugInput()
        {
            // F10: 显示统计信息
            if (Input.GetKeyDown(DEBUG_STATS_KEY))
            {
                Log($"📊 统计信息: {GetStatsString()}");
            }
            
            // F11: 重置处理记录
            if (Input.GetKeyDown(DEBUG_RESET_KEY))
            {
                _processedItems.Clear();
                Log("🔄 已重置处理记录");
            }
            
            // F12: 手动清理字段
            if (Input.GetKeyDown(DEBUG_CLEANUP_KEY))
            {
                CleanupOwnFields();
                Log("🧹 手动清理完成");
            }
        }
        
        /// <summary>
        /// 获取统计信息字符串
        /// </summary>
        private string GetStatsString()
        {
            return $"已处理{_processedItems.Count}个物品";
        }
        
        /// <summary>
        /// 条件日志输出
        /// 只在调试模式或ENABLE_DEBUG为true时输出
        /// </summary>
        private void Log(string message)
        {
            if (ENABLE_DEBUG)
            {
                Debug.Log($"[DemoMod] {message}");
            }
        }
        
        /// <summary>
        /// 错误日志输出
        /// 总是输出，无论调试模式
        /// </summary>
        private void LogError(string message)
        {
            Debug.LogError($"[DemoMod] ❌ {message}");
        }
        
        /// <summary>
        /// 警告日志输出
        /// 总是输出，无论调试模式
        /// </summary>
        private void LogWarning(string message)
        {
            Debug.LogWarning($"[DemoMod] ⚠️ {message}");
        }
    }
}

/*
 * ============================================================================
 * 文件结构说明
 * ============================================================================
 * 
 * 编译前需要的文件:
 * 
 * 1. DemoModExtension.cs (本文件)
 * 2. info.ini (Mod信息配置)
 * 3. preview.png (256x256预览图，可选)
 * 
 * info.ini 内容示例:
 * -------------------------------
 * name=DemoModExtension
 * displayName=ModExtensions演示Mod
 * description=演示CustomItemLevelValue框架的ModExtensions功能
 * -------------------------------
 * 
 * 编译命令 (在项目目录中):
 * -------------------------------
 * dotnet build --configuration Release
 * -------------------------------
 * 
 * 安装位置:
 * -------------------------------
 * 游戏目录/Duckov_Data/Mods/DemoModExtension/
 *   ├── DemoModExtension.dll
 *   ├── info.ini
 *   └── preview.png (可选)
 * -------------------------------
 * 
 * ============================================================================
 * 富文本语法参考
 * ============================================================================
 * 
 * 1. 颜色:
 *    [c=#FF5555]红色文字[/c]
 *    [c=#55FF55]绿色文字[/c]
 *    [c=#5555FF]蓝色文字[/c]
 * 
 * 2. 格式:
 *    [b]粗体文字[/b]
 *    [i]斜体文字[/i]
 *    [u]下划线文字[/u]
 * 
 * 3. 字号:
 *    [size=14]小号文字[/size]
 *    [size=18]中号文字[/size]
 *    [size=24]大号文字[/size]
 * 
 * 4. 分隔线:
 *    [hr] 水平分隔线
 * 
 * 5. 组合使用:
 *    [b][c=#FFD700]金色粗体[/c][/b]
 * 
 * ============================================================================
 * 最佳实践建议
 * ============================================================================
 * 
 * 1. 性能优化:
 *    - 每个物品只处理一次 (使用HashSet缓存)
 *    - 避免在Update()中进行复杂操作
 *    - 字段内容尽量简洁
 * 
 * 2. 兼容性:
 *    - 使用唯一的Mod前缀
 *    - 在OnDisable()中清理字段
 *    - 添加适当的错误处理
 * 
 * 3. 用户体验:
 *    - 使用友好的颜色搭配
 *    - 重要信息使用粗体
 *    - 提供有用的实用信息
 * 
 * 4. 调试:
 *    - 开发时启用调试日志
 *    - 发布前关闭调试输出
 *    - 提供调试快捷键
 * 
 * ============================================================================
 * 常见问题解答
 * ============================================================================
 * 
 * Q: 为什么我的字段不显示?
 * A: 检查以下几点:
 *    1. 字段名格式: [前缀]_[位置]_[字段名]
 *    2. CustomItemLevelValue是否已加载
 *    3. 游戏是否在物品悬停状态
 *    4. 查看游戏日志是否有错误
 * 
 * Q: 如何改变字段显示顺序?
 * A: 字段按字母顺序排序，可以通过调整字段名来控制:
 *    Demo_Top1_01状态
 *    Demo_Top1_02等级
 *    Demo_Top1_03评分
 * 
 * Q: 字段内容可以包含哪些特殊字符?
 * A: 支持完整的BBCode语法，但避免使用:
 *    - 过多的嵌套标签
 *    - 非常长的文本内容
 *    - 游戏不支持的Unicode字符
 * 
 * Q: 如何为特定物品添加字段?
 * A: 检查物品的TypeID或DisplayName:
 *    if (item.TypeID == 123 || item.DisplayName.Contains("特定"))
 *    {
 *        // 添加特殊字段
 *    }
 */
```
