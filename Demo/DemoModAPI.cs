using Duckov.Modding;
using Duckov.UI;
using ItemStatsSystem;
using UnityEngine;

namespace DemoModExtension
{
    /// <summary>
    /// ModExtensions框架演示Mod
    /// 功能：展示如何使用ModExtensions框架在物品面板上添加自定义信息
    /// </summary>
    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        // ========== 核心配置 ==========
        // Mod前缀：确保唯一性，避免与其他Mod冲突
        private const string MOD_PREFIX = "Demo_";

        // 当前正在处理的物品引用
        private Item _currentItem;

        // 点击计数器：演示动态数据
        private int _clickCount = 0;

        // Mod激活状态标志
        private bool _modActive = true;

        // ========== Unity生命周期方法 ==========

        /// <summary>
        /// Mod启动时执行一次
        /// 功能：输出加载日志，告知用户使用方法
        /// </summary>
        void Start()
        {
            // 告诉用户Mod已加载
            Debug.Log("[DemoMod] 简洁演示Mod已加载");

            // 告知用户操作方法
            Debug.Log("[DemoMod] 悬停物品后按数字键1增加计数");
        }

        /// <summary>
        /// Mod启用时执行（每次激活时调用）
        /// 功能：1.注册到ModExtensions框架 2.绑定事件监听
        /// </summary>
        void OnEnable()
        {
            try
            {
                // 1. 将Mod前缀注册到框架
                // 作用：告诉框架"这个Mod是活跃的，不要清理它的字段"
                ModExtensionsAPI.RegisterMod(MOD_PREFIX);
                Debug.Log($"[DemoMod] ✅ 已注册Mod前缀: {MOD_PREFIX}");
            }
            catch (System.Exception ex)
            {
                // 处理注册失败的情况
                Debug.LogWarning($"[DemoMod] 注册失败: {ex.Message}");
            }

            // 2. 监听物品悬停事件
            // 作用：当玩家鼠标悬停在物品上时，我们能收到通知
            ItemHoveringUI.onSetupItem += OnItemHovered;

            // 3. 标记Mod为激活状态
            _modActive = true;

            Debug.Log("[DemoMod] Mod已启用");
        }

        /// <summary>
        /// Mod禁用时执行（每次停用时调用）
        /// 功能：1.移除事件监听 2.通知框架清理字段 3.重置本地状态
        /// </summary>
        void OnDisable()
        {
            // 1. 移除物品悬停事件监听
            // 作用：避免Mod禁用后还响应事件
            ItemHoveringUI.onSetupItem -= OnItemHovered;

            // 2. 标记Mod为非激活状态
            _modActive = false;

            try
            {
                // 3. 告诉框架这个Mod已禁用，需要清理相关字段
                // 作用：框架会删除所有以"Demo_"开头的字段
                ModExtensionsAPI.MarkModAsDeleted(MOD_PREFIX);
                Debug.Log($"[DemoMod] ✅ 已通知框架清理: {MOD_PREFIX}");
            }
            catch (System.Exception ex)
            {
                // 处理清理通知失败的情况
                Debug.LogWarning($"[DemoMod] 清理通知失败: {ex.Message}");
            }

            // 4. 清理本地状态
            ClearLocalState();

            Debug.Log("[DemoMod] Mod已禁用");
        }

        /// <summary>
        /// 每帧执行一次
        /// 功能：检测按键输入并响应
        /// </summary>
        void Update()
        {
            // 仅在Mod激活且有当前物品时处理按键
            if (!_modActive || _currentItem == null) return;

            // 按数字键1测试刷新功能
            // 作用：提供用户交互方式，演示动态更新
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                IncreaseCounter();
            }
        }

        // ========== 事件处理方法 ==========

        /// <summary>
        /// 物品悬停事件处理函数
        /// 触发时机：玩家鼠标悬停在物品上时
        /// 功能：1.保存物品引用 2.写入字段 3.刷新显示
        /// </summary>
        /// <param name="ui">悬停UI实例</param>
        /// <param name="item">被悬停的物品</param>
        private void OnItemHovered(ItemHoveringUI ui, Item item)
        {
            // 安全检查：确保物品不为空
            if (item == null) return;

            // 1. 保存当前物品引用
            // 作用：后续按键操作需要知道对哪个物品生效
            _currentItem = item;
            Debug.Log($"[DemoMod] 悬停物品: {item.DisplayName}");

            // 2. 将演示字段写入物品
            // 作用：在物品上添加自定义数据
            WriteDemoFields(item);

            // 3. 立即刷新UI显示
            // 作用：让写入的字段立刻显示出来
            RefreshDisplay(item);
        }

        // ========== 核心功能方法 ==========

        /// <summary>
        /// 写入演示字段到物品
        /// 功能：在物品的指定位置添加自定义文本字段
        /// 字段命名规则：{前缀}_{位置}_{字段名}
        /// </summary>
        /// <param name="item">要写入字段的物品</param>
        private void WriteDemoFields(Item item)
        {
            if (item == null) return;

            // 1. Top1位置：显示计数器（在物品面板顶部第一段）
            // 格式：Demo_Top1_计数器
            // 内容：[b]点击次数: X[/b] + 操作提示
            string top1Text = $"[b]点击次数: {_clickCount}[/b]\n按1键增加计数";
            item.Variables.SetString($"{MOD_PREFIX}Top1_计数器", top1Text);

            // 2. Bottom1位置：显示状态信息（在物品面板底部第一段）
            // 格式：Demo_Bottom1_状态
            // 内容：物品ID + 最后刷新时间
            string bottom1Text = $"物品ID: {item.TypeID}\n最后刷新: {System.DateTime.Now:HH:mm:ss}";
            item.Variables.SetString($"{MOD_PREFIX}Bottom1_状态", bottom1Text);

            Debug.Log($"[DemoMod] 字段已写入: {item.DisplayName}");
        }

        /// <summary>
        /// 增加计数器并刷新UI
        /// 功能：演示如何动态更新字段并刷新显示
        /// 这是ModExtensions框架的核心使用示例
        /// </summary>
        private void IncreaseCounter()
        {
            // 安全检查
            if (_currentItem == null)
            {
                Debug.LogError("[DemoMod] ❌ 没有当前物品");
                return;
            }

            // 1. 增加计数器
            _clickCount++;
            Debug.Log($"[DemoMod] 🧪 点击第{_clickCount}次");

            // 2. 更新字段内容
            // 注意：只更新字段值是不够的，必须刷新UI才能看到变化
            WriteDemoFields(_currentItem);

            // 3. 调用API刷新UI（这是关键步骤！）
            // 参数说明：
            // - _currentItem: 要刷新的物品
            // - true: 是否触发UI刷新（必须为true才能看到变化）
            Debug.Log("[DemoMod] 调用: ModExtensionsManager.Instance.RefreshItemCache(item, true)");
            try
            {
                ModExtensionsAPI.RefreshItemCache(_currentItem, true);
                Debug.Log("[DemoMod] ✅ API调用成功");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[DemoMod] ❌ API调用失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 刷新显示
        /// 功能：调用框架API刷新物品面板的显示
        /// 这是保证自定义字段能显示出来的关键方法
        /// </summary>
        /// <param name="item">要刷新显示物品</param>
        private void RefreshDisplay(Item item)
        {
            if (item == null) return;

            try
            {
                // 调用刷新API
                // 作用：通知框架重新扫描字段并更新UI
                ModExtensionsAPI.RefreshItemCache(item, true);
                Debug.Log($"[DemoMod] ✅ 显示已刷新");
            }
            catch
            {
                // 处理框架未加载的情况
                Debug.LogWarning("[DemoMod] ⚠️ 刷新失败 (可能框架未加载)");
            }
        }

        /// <summary>
        /// 清理本地状态
        /// 功能：重置所有本地变量，避免状态残留
        /// 在Mod禁用时调用
        /// </summary>
        private void ClearLocalState()
        {
            // 清空当前物品引用
            _currentItem = null;

            // 重置计数器
            _clickCount = 0;

            Debug.Log("[DemoMod] 本地状态已清理");
        }
    }
}
