# ModExtensions 实战示例 - DemoModExtension

## 📋 示例概述

这是一个完整的ModExtensions框架演示Mod，展示了：
- **基础集成** - 三行代码添加自定义显示
- **高级特性** - 渐变效果、动态更新、智能刷新
- **性能优化** - 分离式API、缓存策略、日志控制
- **最佳实践** - 错误处理、生命周期管理

## 🎯 功能演示

### 五个位置完整展示
| 位置 | 演示内容 | 技术亮点 |
|------|----------|----------|
| **Top1** | 主题文字池 + 完整跨度渐变 | 字符级横向渐变，BBCode标签处理 |
| **Top2** | 配色方案选择器 + 强渐变 | 用户交互响应，实时刷新 |
| **Top3** | 自动充能进度条 + 统一渐变 | 定时更新，智能变化检测 |
| **Bottom1** | 交互计数器 + 中等渐变 | 状态管理，按键响应 |
| **Bottom2** | 框架信息 + 温和渐变 | 元数据生成，调试信息 |

## 🚀 完整代码

```csharp
/*
 * DemoModExtension.cs - ModExtensions框架演示与教学Mod
 * 
 * 设计目标：
 * 1. 演示ModExtensions框架的完整功能
 * 2. 展示性能优化的刷新策略
 * 3. 提供清晰的API使用示例
 * 4. 解决常见的渐变和刷新问题
 * 
 * 核心概念教学：
 * 1. 分离式刷新API：RefreshCacheOnly() + RequestUIRefresh()
 * 2. 智能缓存策略：数据变化检测，避免不必要刷新
 * 3. 统一渐变系统：ApplyHorizontalGradient处理所有文本
 * 4. 日志性能优化：分级日志系统，减少控制台压力
 */

using Duckov.Modding;
using Duckov.UI;
using Duckov.Utilities;
using ItemStatsSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;
namespace DemoModExtension
{
    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        // ========== 核心配置 ==========
        private const string MOD_PREFIX = "Demo_";  // 所有字段使用此前缀，便于识别和清理

        // 日志级别控制系统 - 教学点：性能优化
        public enum LogLevel
        {
            None = 0,      // 无日志 - 发布版本使用
            Error = 1,     // 仅错误 - 玩家版本
            Warning = 2,   // 错误 + 警告
            Info = 3,      // 重要信息（默认）
            Debug = 4,     // 调试信息 - 开发者使用
            Verbose = 5    // 极度详细 - 性能测试
        }

        private LogLevel _currentLogLevel = LogLevel.Info;
        private const LogLevel DEFAULT_LOG_LEVEL = LogLevel.Info;

        // 交互按键配置 - 教学点：用户交互设计
        private const KeyCode COLOR_CHANGE_KEY = KeyCode.Alpha1;    // 切换配色方案
        private const KeyCode COUNTER_RESET_KEY = KeyCode.F2;       // 重置计数器
        private const KeyCode LOG_LEVEL_UP_KEY = KeyCode.F7;        // 提高日志级别
        private const KeyCode LOG_LEVEL_DOWN_KEY = KeyCode.F6;      // 降低日志级别

        // ========== 配色方案系统 ==========
        // 教学点：可配置的颜色系统设计
        private enum ColorScheme
        {
            热情火焰,    // 红→橙→黄渐变
            海洋之心,    // 蓝→青→绿渐变  
            紫幻梦境,    // 紫→粉→浅紫渐变
            森林之歌     // 绿→黄→浅绿渐变
        }

        private ColorScheme _currentScheme = ColorScheme.热情火焰;
        private int _schemeIndex = 0;

        // 明亮版配色方案 (主色 → 中间色 → 尾色)
        // 教学点：使用十六进制颜色代码，支持Unity富文本
        private readonly Dictionary<ColorScheme, string[]> _colorSchemes = new Dictionary<ColorScheme, string[]>
        {
            { ColorScheme.热情火焰, new[] { "#FF3333", "#FF9900", "#FFFF66" } }, // 红→橙→亮黄
            { ColorScheme.海洋之心, new[] { "#3366FF", "#33CCCC", "#66FF99" } }, // 蓝→青→亮绿
            { ColorScheme.紫幻梦境, new[] { "#CC66FF", "#FF66CC", "#FFCCFF" } }, // 紫→粉→浅粉
            { ColorScheme.森林之歌, new[] { "#33CC33", "#99FF33", "#CCFF99" } }  // 绿→黄绿→浅绿
        };

        // ========== TOP1文字演示池 ==========
        // 教学点：动态内容池，避免硬编码
        private int _textPoolIndex = 0;
        private readonly List<string[]> _top1TextPools = new List<string[]>
        {
            // 热情火焰主题
            new[] {
                "[b]火焰战意[/b]",      // BBCode演示：加粗文本
                "[b]荣耀之光[/b]",
                "[b]黄昏烈焰[/b]"
            },
            // 海洋之心主题  
            new[] {
                "[b]深海智慧[/b]",
                "[b]碧波荡漾[/b]",
                "[b]星空奥秘[/b]"
            },
            // 紫幻梦境主题
            new[] {
                "[b]紫雾笼罩[/b]",
                "[b]幻月流光[/b]",
                "[b]符文闪烁[/b]"
            },
            // 森林之歌主题
            new[] {
                "[b]森林呼吸[/b]",
                "[b]溪流清澈[/b]",
                "[b]新芽破土[/b]"
            }
        };

        // ========== TOP3进度条系统 ==========
        // 教学点：自动更新的动态数据
        private float _progressCharge = 0f;
        private const float PROGRESS_CHARGE_RATE = 0.25f; // 每次充能25%
        private const float PROGRESS_UPDATE_INTERVAL = 5f; // 每5秒更新一次
        private float _progressTimer = 0f;

        // ========== Bottom1计数器系统 ==========
        // 教学点：用户交互状态管理
        private int _hoverCounter = 0;
        private const int MAX_COUNTER = 10;
        private int _currentItemInstanceId = -1;

        // ========== 状态管理 ==========
        // 教学点：Mod状态管理最佳实践
        private Item _lastHoveredItem;
        private ItemHoveringUI _lastHoveredUI;
        private bool _isModActive = true;

        // ========== 数据变化检测系统 ==========
        // 教学点：智能刷新策略，避免不必要刷新
        private float _lastProgressCharge = -1f;
        private ColorScheme _lastColorScheme = ColorScheme.热情火焰;
        private int _lastHoverCounter = -1;
        private bool _forceNextRefresh = false;

        // ========== Unity生命周期 ==========
        // 教学点：正确的Mod初始化和清理

        private void Start()
        {
            // 根据构建类型设置日志级别
#if DEBUG
            _currentLogLevel = LogLevel.Debug;
            Log("🔧 调试模式已启用，日志级别: Debug", LogType.Important);
#else
            _currentLogLevel = DEFAULT_LOG_LEVEL;
#endif

            Log("🚀 多功能演示Mod已加载", LogType.Important);
            Log("📚 核心功能演示:", LogType.Info);
            Log("  1. TOP2 - 配色方案选择器 (按'1'切换，字符渐变)", LogType.Info);
            Log("  2. TOP1 - 主题文字演示池 (完整跨度字符渐变)", LogType.Info);
            Log("  3. TOP3 - 自动充能进度条 (统一横向渐变)", LogType.Info);
            Log("  4. Bottom1 - 交互计数器 (悬停计数，F2重置，字符渐变)", LogType.Info);
            Log("  5. Bottom2 - 框架信息 + 物品ID (字符渐变)", LogType.Info);
            Log("🔄 刷新策略: 分离式API (RefreshCacheOnly + RequestUIRefresh)", LogType.Info);
            Log("⚡ 性能优化: 数据变化检测 + 分级日志系统", LogType.Info);
            Log("🎮 提示: 悬停任何物品查看演示效果", LogType.Success);
            Log("📊 日志控制: F6降低级别, F7提高级别", LogType.Info);
        }

        private void OnEnable()
        {
            _isModActive = true;
            ItemHoveringUI.onSetupItem += OnItemHovered;
            Log("✅ 事件监听器已注册", LogType.Success);
        }

        private void OnDisable()
        {
            try
            {
                _isModActive = false;

                // 1. 移除事件监听
                ItemHoveringUI.onSetupItem -= OnItemHovered;

                // 2. 【简化】只需通知主Mod，主Mod会负责清理
                NotifyMainModForCleanup();

                // 3. 清理本地状态（不清理物品字段，由主Mod负责）
                ClearLocalState();

                Log("🛑 Mod已卸载，已通知主Mod清理字段", LogType.Warning);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[DemoMod] 卸载异常: {ex.Message}");
            }
        }

        private void NotifyMainModForCleanup()
        {
            try
            {
                if (!CheckFrameworkLoaded()) return;

                var modExtensionsType = System.Type.GetType("CustomItemLevelValue.Core.ModExtensionsManager, CustomItemLevelValue");
                if (modExtensionsType == null) return;

                var instanceProperty = modExtensionsType.GetProperty("Instance");
                var markMethod = modExtensionsType.GetMethod("MarkModAsDeleted");

                if (instanceProperty != null && markMethod != null)
                {
                    var instance = instanceProperty.GetValue(null);
                    markMethod.Invoke(instance, new object[] { MOD_PREFIX });
                    Log($"🏷️ 已通知主Mod清理 {MOD_PREFIX} 字段", LogType.Info);
                }
            }
            catch (System.Exception ex)
            {
                LogWarning($"通知主Mod失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 清理本地状态（不清理物品字段）
        /// 教学点：正确的状态清理，避免内存泄漏
        /// </summary>
        private void ClearLocalState()
        {
            try
            {
                Log("🧼 开始清理本地状态...", LogType.Info, LogLevel.Debug);

                // 1. 清理物品引用
                _lastHoveredItem = null;
                _lastHoveredUI = null;

                // 2. 清理计数器状态
                _currentItemInstanceId = -1;
                _hoverCounter = 0;

                // 3. 清理数据变化检测状态
                _forceNextRefresh = false;
                _lastProgressCharge = -1f;
                _lastColorScheme = ColorScheme.热情火焰;
                _lastHoverCounter = -1;

                // 4. 清理进度条状态
                _progressCharge = 0f;
                _progressTimer = 0f;

                // 5. 清理配色状态
                _schemeIndex = 0;
                _currentScheme = ColorScheme.热情火焰;
                _textPoolIndex = 0;

                Log("✅ 本地状态清理完成", LogType.Info, LogLevel.Debug);
            }
            catch (Exception ex)
            {
                LogError($"清理本地状态失败: {ex.Message}");
            }
        }

        private void Update()
        {
            if (!_isModActive) return;

            HandleDebugInput();
            UpdateProgressSystem();
        }

        // ========== 核心交互方法 ==========
        // 教学点：智能刷新策略实现

        /// <summary>
        /// 物品悬停事件处理 - 智能刷新策略
        /// 教学点：数据变化检测，避免不必要刷新
        /// </summary>
        private void OnItemHovered(ItemHoveringUI ui, Item item)
        {
            if (!_isModActive || item == null || ui == null) return;

            _lastHoveredItem = item;
            _lastHoveredUI = ui;

            // 更新计数器 (仅当悬停新物品时)
            int instanceId = item.GetInstanceID();
            bool isNewItem = _currentItemInstanceId != instanceId;

            if (isNewItem)
            {
                _currentItemInstanceId = instanceId;
                _hoverCounter = (_hoverCounter + 1) % (MAX_COUNTER + 1);
                Log($"🆕 新物品悬停: {item.DisplayName}, 计数器: {_hoverCounter}", LogType.Info, LogLevel.Debug);
            }

            // 检测数据变化（智能刷新核心）
            bool dataChanged = CheckIfDataChanged();

            // 决策：是否需要刷新？
            bool shouldRefresh = isNewItem || dataChanged || _forceNextRefresh;

            // 应用字段到物品
            ApplyDemoFields(item);

            if (shouldRefresh)
            {
                string reason = isNewItem ? "新物品" :
                               dataChanged ? "数据变化" :
                               "强制刷新";

                Log($"🔄 触发刷新: {item.DisplayName} ({reason})", LogType.Info, LogLevel.Debug);
                ExecuteImmediateRefresh(item, reason);

                // 更新记录值
                UpdateDataRecords();
                _forceNextRefresh = false;
            }
            else
            {
                Log($"💾 使用缓存显示: {item.DisplayName} (无变化)", LogType.Info, LogLevel.Verbose);
            }
        }

        /// <summary>
        /// 应用演示字段到物品 - 核心内容生成
        /// 教学点：ModExtensions字段格式规范
        /// </summary>
        private void ApplyDemoFields(Item item)
        {
            if (item == null) return;

            string[] colors = _colorSchemes[_currentScheme];

            Log($"🎨 应用配色方案: {_currentScheme}", LogType.Info, LogLevel.Verbose);

            // TOP2: 配色方案显示 - 强渐变 (起始色→中间色)
            // 教学点：多行文本的渐变处理
            string top2Text = $"当前配色: {_currentScheme}\n按'1'键切换方案";
            item.Variables.SetString($"{MOD_PREFIX}Top2_配色方案",
                ApplyHorizontalGradient(top2Text, colors[0], colors[1], 12));

            // TOP1: 主题文字演示 - 完整跨度渐变 (起始色→结束色)
            string[] currentTextPool = _top1TextPools[_schemeIndex];
            string top1Text = currentTextPool[_textPoolIndex % currentTextPool.Length];
            item.Variables.SetString($"{MOD_PREFIX}Top1_主题演示",
                ApplyHorizontalGradient(top1Text, colors[0], colors[2], 8));

            // TOP3: 进度条显示 - 统一横向渐变
            string progressBar = BuildProgressBar(_progressCharge, colors);
            // 修复：构建纯文本，让ApplyHorizontalGradient处理BBCode
            string progressText = $"自动充能系统\n{progressBar}\n进度: {(_progressCharge * 100):F0}%\n每5秒充能25%";
            // 应用横向渐变
            string top3Display = ApplyHorizontalGradient(progressText, colors[0], colors[2], 15);
            item.Variables.SetString($"{MOD_PREFIX}Top3_充能进度", top3Display);

            // Bottom1: 交互计数器 - 中等强度渐变 (起始色→结束色)
            string bottom1Text = $"交互计数器演示\n悬停计数: {_hoverCounter}/{MAX_COUNTER}\n按F2重置计数器";
            item.Variables.SetString($"{MOD_PREFIX}Bottom1_互动演示",
                ApplyHorizontalGradient(bottom1Text, colors[0], colors[2], 18));

            // Bottom2: 框架信息 + 物品ID - 温和渐变 (中间色→起始色)
            string itemId = GenerateItemId(item);
            string bottom2Text = $"ModExtensions框架演示\n框架版本: v1.4.2 \nAPI状态: 已连接 \n物品ID: {itemId}";
            item.Variables.SetString($"{MOD_PREFIX}Bottom2_框架信息",
                ApplyHorizontalGradient(bottom2Text, colors[1], colors[0], 20));
        }

        // ========== 颜色渐变系统 ==========
        // 教学点：字符级渐变算法实现

        /// <summary>
        /// 应用横向颜色渐变（字符级着色）
        /// 教学点：BBCode标签处理与字符级渲染
        /// </summary>
        /// <param name="text">原始文本（可包含BBCode）</param>
        /// <param name="startColorHex">起始颜色 #RRGGBB</param>
        /// <param name="endColorHex">结束颜色 #RRGGBB</param>
        /// <param name="cycleLength">渐变周期长度（字符数）</param>
        /// <returns>应用了渐变BBCode的文本</returns>
        private string ApplyHorizontalGradient(string text, string startColorHex, string endColorHex, int cycleLength = 15)
        {
            if (string.IsNullOrEmpty(text)) return text;

            Color startColor = HexToColor(startColorHex);
            Color endColor = HexToColor(endColorHex);

            System.Text.StringBuilder result = new System.Text.StringBuilder();
            int visibleCharCount = 0;

            // 遍历文本，应用字符级渐变
            for (int i = 0; i < text.Length; i++)
            {
                char currentChar = text[i];

                // 处理BBCode标签（直接复制，不进行着色）
                // 教学点：BBCode标签识别与跳过
                if (currentChar == '[')
                {
                    int tagEnd = text.IndexOf(']', i);
                    if (tagEnd > i)
                    {
                        // 复制整个BBCode标签
                        result.Append(text.Substring(i, tagEnd - i + 1));
                        i = tagEnd;
                        continue;
                    }
                }

                // 对可见字符应用渐变着色
                if (!char.IsWhiteSpace(currentChar))
                {
                    // 计算渐变位置
                    float t = (float)(visibleCharCount % cycleLength) / Mathf.Max(1, cycleLength - 1);
                    Color charColor = Color.Lerp(startColor, endColor, t);
                    string colorHex = $"#{ColorToHex(charColor)}";

                    // 正确的BBCode格式：[c=#RRGGBB]字符[/c]
                    result.Append($"[c={colorHex}]{currentChar}[/c]");
                    visibleCharCount++;
                }
                else
                {
                    // 空格和换行符直接添加
                    result.Append(currentChar);
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// Color转十六进制字符串
        /// 教学点：颜色编码转换
        /// </summary>
        private string ColorToHex(Color color)
        {
            int r = Mathf.Clamp(Mathf.RoundToInt(color.r * 255), 0, 255);
            int g = Mathf.Clamp(Mathf.RoundToInt(color.g * 255), 0, 255);
            int b = Mathf.Clamp(Mathf.RoundToInt(color.b * 255), 0, 255);
            return $"{r:X2}{g:X2}{b:X2}";
        }

        /// <summary>
        /// 十六进制字符串转Color
        /// 教学点：颜色解析与错误处理
        /// </summary>
        private Color HexToColor(string hex)
        {
            hex = hex.TrimStart('#');

            if (hex.Length == 6)
            {
                try
                {
                    byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                    byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                    byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
                    return new Color(r / 255f, g / 255f, b / 255f);
                }
                catch
                {
                    LogWarning($"颜色解析失败: #{hex}，使用白色作为回退");
                    return Color.white;
                }
            }

            return Color.white;
        }

        /// <summary>
        /// 构建进度条（返回纯文本，由ApplyHorizontalGradient处理渐变）
        /// 教学点：进度条可视化
        /// </summary>
        private string BuildProgressBar(float progress, string[] colors)
        {
            int filledBlocks = Mathf.RoundToInt(progress * 10f);
            System.Text.StringBuilder bar = new System.Text.StringBuilder();

            // 使用统一的BBCode格式，让ApplyHorizontalGradient能正确处理
            for (int i = 0; i < 10; i++)
            {
                string blockChar = i < filledBlocks ? "■" : "□";
                bar.Append(blockChar);
                if (i < 9) bar.Append(" ");
            }

            return bar.ToString();
        }

        // ========== 数据变化检测系统 ==========
        // 教学点：智能刷新策略实现细节

        /// <summary>
        /// 检测数据是否发生变化
        /// 教学点：变化检测算法，避免不必要刷新
        /// </summary>
        private bool CheckIfDataChanged()
        {
            bool progressChanged = Mathf.Abs(_progressCharge - _lastProgressCharge) > 0.01f;
            bool colorChanged = _currentScheme != _lastColorScheme;
            bool counterChanged = _hoverCounter != _lastHoverCounter;

            // 只在Debug级别输出详细变化
            if (_currentLogLevel >= LogLevel.Debug)
            {
                if (progressChanged) LogDetail($"📊 进度条变化: {_lastProgressCharge * 100:F0}% → {_progressCharge * 100:F0}%");
                if (colorChanged) LogDetail($"🎨 配色方案变化: {_lastColorScheme} → {_currentScheme}");
                if (counterChanged) LogDetail($"🔢 计数器变化: {_lastHoverCounter} → {_hoverCounter}");
            }

            return progressChanged || colorChanged || counterChanged;
        }

        /// <summary>
        /// 更新数据记录
        /// 教学点：状态快照保存
        /// </summary>
        private void UpdateDataRecords()
        {
            _lastProgressCharge = _progressCharge;
            _lastColorScheme = _currentScheme;
            _lastHoverCounter = _hoverCounter;
            Log($"💾 数据记录已更新", LogType.Info, LogLevel.Verbose);
        }

        // ========== 配色方案管理 ==========

        /// <summary>
        /// 切换配色方案 - 立即刷新
        /// 教学点：用户交互响应
        /// </summary>
        private void SwitchColorScheme()
        {
            _schemeIndex = (_schemeIndex + 1) % _colorSchemes.Count;
            _currentScheme = (ColorScheme)_schemeIndex;
            _textPoolIndex = _schemeIndex * 3; // 同步切换文字池

            Log($"🎨 切换到配色方案: {_currentScheme}", LogType.Success);

            // 标记数据变化，确保刷新
            _forceNextRefresh = true;

            // 立即刷新（如果当前有物品悬停）
            if (_lastHoveredItem != null)
            {
                Log($"🔄 立即刷新配色变化", LogType.Info, LogLevel.Debug);
                ExecuteImmediateRefresh(_lastHoveredItem, $"切换配色到{_currentScheme}");
            }
        }

        // ========== TOP3进度条系统 ==========
        // 教学点：自动更新系统设计

        /// <summary>
        /// 更新进度条系统 - 变化时自动刷新
        /// 教学点：定时更新与变化检测
        /// </summary>
        private void UpdateProgressSystem()
        {
            if (!_isModActive) return;

            _progressTimer += Time.deltaTime;
            if (_progressTimer >= PROGRESS_UPDATE_INTERVAL)
            {
                _progressTimer = 0f;
                float oldProgress = _progressCharge;
                _progressCharge = (_progressCharge + PROGRESS_CHARGE_RATE) % 1.01f;

                // 进度变化：标记数据变化
                if (Mathf.Abs(_progressCharge - oldProgress) > 0.01f)
                {
                    Log($"📊 进度条自动变化: {oldProgress * 100:F0}% → {_progressCharge * 100:F0}%", LogType.Info, LogLevel.Debug);
                    _forceNextRefresh = true;

                    // 如果当前有物品悬停，立即刷新
                    if (_lastHoveredItem != null)
                    {
                        Log($"🔄 立即刷新进度条变化", LogType.Info, LogLevel.Debug);
                        ExecuteImmediateRefresh(_lastHoveredItem,
                            $"进度条变化 {oldProgress * 100:F0}% → {_progressCharge * 100:F0}%");
                    }
                }
            }
        }

        // ========== 元数据生成 ==========

        /// <summary>
        /// 生成物品唯一ID（演示用）
        /// 教学点：唯一标识符生成
        /// </summary>
        private string GenerateItemId(Item item)
        {
            if (item == null) return "#000000";
            int hash = item.GetInstanceID() * 137 % 0xFFFFFF;
            return $"#{hash:X6}";
        }

        // ========== 交互处理 ==========
        // 教学点：用户输入处理

        private void HandleDebugInput()
        {
            // 1. 配色方案切换
            if (Input.GetKeyDown(COLOR_CHANGE_KEY))
            {
                SwitchColorScheme();
            }

            // 2. 计数器重置
            if (Input.GetKeyDown(COUNTER_RESET_KEY))
            {
                ResetCounter();
            }

            // 3. 日志级别控制
            if (Input.GetKeyDown(LOG_LEVEL_UP_KEY))
            {
                IncreaseLogLevel();
            }

            if (Input.GetKeyDown(LOG_LEVEL_DOWN_KEY))
            {
                DecreaseLogLevel();
            }

            // 4. 调试信息
            if (Input.GetKeyDown(KeyCode.F5))
            {
                ShowDebugStats();
            }
        }

        /// <summary>
        /// 重置计数器
        /// 教学点：状态重置与刷新
        /// </summary>
        private void ResetCounter()
        {
            _hoverCounter = 0;
            Log($"🔢 计数器重置为0", LogType.Success);

            // 标记数据变化
            _forceNextRefresh = true;

            // 立即刷新（如果当前有物品悬停）
            if (_lastHoveredItem != null)
            {
                Log($"🔄 立即刷新计数器变化", LogType.Info, LogLevel.Debug);
                ExecuteImmediateRefresh(_lastHoveredItem, "重置计数器");
            }
        }

        /// <summary>
        /// 提高日志级别
        /// 教学点：运行时配置调整
        /// </summary>
        private void IncreaseLogLevel()
        {
            if (_currentLogLevel < LogLevel.Verbose)
            {
                _currentLogLevel++;
                Log($"📈 日志级别提高至: {_currentLogLevel}", LogType.Success);
            }
        }

        /// <summary>
        /// 降低日志级别
        /// 教学点：运行时配置调整
        /// </summary>
        private void DecreaseLogLevel()
        {
            if (_currentLogLevel > LogLevel.None)
            {
                _currentLogLevel--;
                Log($"📉 日志级别降低至: {_currentLogLevel}", LogType.Success);
            }
        }

        // ========== 分离式刷新系统 ==========
        // 教学点：ModExtensions API最佳实践

        /// <summary>
        /// 执行立即刷新（用于数据变化时）
        /// 教学点：分离式刷新API使用
        /// </summary>
        private void ExecuteImmediateRefresh(Item item, string reason = "")
        {
            if (item == null)
            {
                LogError("刷新失败: 物品为null");
                return;
            }

            try
            {
                Log($"开始刷新: {item.DisplayName} ({reason})", LogType.Info, LogLevel.Debug);

                // 1. 更新字段
                ApplyDemoFields(item);

                // 2. 【关键API】仅更新ModExtensions缓存（性能优化）
                bool cacheSuccess = RefreshModExtensionsCacheOnly(item);

                // 3. 【关键API】请求UI重构（触发主Mod完整刷新流程）
                bool uiSuccess = RequestUIRefresh(item);

                if (!cacheSuccess || !uiSuccess)
                {
                    LogWarning($"刷新部分失败: 缓存={cacheSuccess}, UI={uiSuccess}");
                }
                else
                {
                    Log($"✅ 刷新完成: {item.DisplayName}", LogType.Info, LogLevel.Verbose);
                }
            }
            catch (System.Exception ex)
            {
                LogError($"刷新失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 【API示例】仅刷新ModExtensions缓存（不触发UI）
        /// 教学点：RefreshCacheOnly API使用
        /// 使用场景：批量更新多个字段后统一刷新UI
        /// </summary>
        private bool RefreshModExtensionsCacheOnly(Item item)
        {
            if (item == null) return false;

            try
            {
                var modExtensionsType = System.Type.GetType("CustomItemLevelValue.Core.ModExtensionsManager, CustomItemLevelValue");
                if (modExtensionsType == null)
                {
                    LogWarning("CustomItemLevelValue框架未加载");
                    return false;
                }

                var instanceProperty = modExtensionsType.GetProperty("Instance");
                var refreshCacheOnlyMethod = modExtensionsType.GetMethod("RefreshCacheOnly");

                if (instanceProperty == null || refreshCacheOnlyMethod == null)
                {
                    // 回退到兼容API
                    var refreshMethod = modExtensionsType.GetMethod("RefreshItemCache", new System.Type[] { typeof(Item), typeof(bool) });
                    if (refreshMethod != null)
                    {
                        var managerInstance_01 = instanceProperty.GetValue(null);
                        refreshMethod.Invoke(managerInstance_01, new object[] { item, false });
                        Log($"💾 缓存已刷新（兼容模式）", LogType.Info, LogLevel.Verbose);
                        return true;
                    }
                    return false;
                }

                var managerInstanceRef = instanceProperty.GetValue(null);  // ← 修改：使用managerInstanceRef
                refreshCacheOnlyMethod.Invoke(managerInstanceRef, new object[] { item });
                Log($"💾 缓存已刷新", LogType.Info, LogLevel.Verbose);
                return true;
            }
            catch (System.Exception ex)
            {
                LogError($"缓存刷新失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 【API示例】请求UI重构（触发主Mod完整刷新）
        /// 教学点：RequestUIRefresh API使用
        /// 使用场景：需要立即更新UI显示时调用
        /// </summary>
        private bool RequestUIRefresh(Item item)
        {
            if (item == null) return false;

            try
            {
                var refresherType = System.Type.GetType("CustomItemLevelValue.Core.ModExtensionsUIRefresher, CustomItemLevelValue");
                if (refresherType == null)
                {
                    LogWarning("ModExtensionsUIRefresher未找到");
                    return false;
                }

                var requestMethod = refresherType.GetMethod("RequestUIRefresh",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

                if (requestMethod == null) return false;

                requestMethod.Invoke(null, new object[] { item });
                Log($"🖥️ UI刷新请求已发送", LogType.Info, LogLevel.Verbose);
                return true;
            }
            catch (System.Exception ex)
            {
                LogError($"UI刷新请求失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 【兼容性示例】刷新物品缓存（使用新API实现）
        /// 教学点：向后兼容设计
        /// </summary>
        private bool RefreshItemCache(Item item)
        {
            // 使用分离式API实现，保持向后兼容
            bool cacheSuccess = RefreshModExtensionsCacheOnly(item);
            bool uiSuccess = RequestUIRefresh(item);
            return cacheSuccess && uiSuccess;
        }

        // ========== 调试信息 ==========
        // 教学点：Mod状态监控与调试

        private void ShowDebugStats()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("===== 演示Mod状态统计 =====");
            sb.AppendLine($"当前配色: {_currentScheme}");
            sb.AppendLine($"进度条: {_progressCharge * 100:F0}%");
            sb.AppendLine($"计数器: {_hoverCounter}/{MAX_COUNTER}");
            sb.AppendLine($"最后物品: {(_lastHoveredItem != null ? _lastHoveredItem.DisplayName : "无")}");
            sb.AppendLine($"框架状态: {(CheckFrameworkLoaded() ? "✅ 已连接" : "❌ 未连接")}");
            sb.AppendLine($"日志级别: {_currentLogLevel}");
            sb.AppendLine($"刷新策略: 分离式API (缓存+UI分离)");
            sb.AppendLine($"渐变系统: 统一横向字符级渐变");
            sb.AppendLine($"数据检测: {(_lastProgressCharge >= 0 ? "✅ 已启用" : "❌ 未启用")}");
            sb.AppendLine("==========================");

            Log(sb.ToString(), LogType.Important);
        }

        /// <summary>
        /// 检查框架是否加载
        /// 教学点：依赖检查
        /// </summary>
        private bool CheckFrameworkLoaded()
        {
            return System.Type.GetType("CustomItemLevelValue.Core.ModExtensionsManager, CustomItemLevelValue") != null;
        }

        // ========== 清理方法 ==========
        // 教学点：Mod卸载时的资源清理

        private void CleanupAllFields()
        {
            try
            {
                var allItems = Object.FindObjectsOfType<Item>();
                int fieldsRemoved = 0;

                foreach (var item in allItems)
                {
                    if (item == null) continue;
                    fieldsRemoved += RemoveFieldsFromCollection(item.Variables, MOD_PREFIX);
                    fieldsRemoved += RemoveFieldsFromCollection(item.Constants, MOD_PREFIX);
                }

                if (fieldsRemoved > 0)
                {
                    Log($"🧹 清理完成: {fieldsRemoved}个演示字段", LogType.Info);
                }
            }
            catch (System.Exception ex)
            {
                LogError($"清理失败: {ex.Message}");
            }
        }

        private int RemoveFieldsFromCollection(CustomDataCollection collection, string prefix)
        {
            if (collection == null) return 0;

            int removed = 0;
            foreach (var data in collection.ToArray())
            {
                if (data?.Key?.StartsWith(prefix) == true)
                {
                    try
                    {
                        collection.Remove(data);
                        removed++;
                    }
                    catch { }
                }
            }

            return removed;
        }

        // ========== 优化的日志系统 ==========
        // 教学点：分级日志系统实现

        private enum LogType { Info, Success, Warning, Error, Important, Detail }

        /// <summary>
        /// 核心日志方法（带级别控制）
        /// 教学点：性能优化的日志系统
        /// </summary>
        private void Log(string message, LogType type = LogType.Info, LogLevel minLevel = LogLevel.Info)
        {
            if (!_isModActive || _currentLogLevel < minLevel) return;

            string prefix = type switch
            {
                LogType.Success => "[DemoMod] ",
                LogType.Warning => "[DemoMod] 警告: ",
                LogType.Error => "[DemoMod] 错误: ",
                LogType.Important => "[DemoMod] ",
                LogType.Detail => "[DemoMod] ",
                _ => "[DemoMod] "
            };

            Debug.Log(prefix + message);
        }

        // 快捷方法（保持原有API兼容）
        private void LogDetail(string message) => Log(message, LogType.Detail, LogLevel.Debug);
        private void LogWarning(string message) => Log(message, LogType.Warning, LogLevel.Warning);
        private void LogError(string message) => Log(message, LogType.Error, LogLevel.Error);
        private void LogSuccess(string message) => Log(message, LogType.Success, LogLevel.Info);

        /// <summary>
        /// 设置日志级别（公开API，可供其他系统调用）
        /// </summary>
        public void SetLogLevel(LogLevel level)
        {
            _currentLogLevel = level;
            LogSuccess($"日志级别设置为: {level}");
        }

        /// <summary>
        /// 获取当前日志级别
        /// </summary>
        public LogLevel GetLogLevel()
        {
            return _currentLogLevel;
        }
    }
}
