using ItemStatsSystem;
using System;
using System.Reflection;
using UnityEngine;
using Debug = UnityEngine.Debug;

public static class ModExtensionsAPI
{
    private const string MOD_NAME = "CustomItemLevelValue";
    private const string MANAGER_TYPE = "CustomItemLevelValue.Core.ModExtensionsManager, CustomItemLevelValue";

    private static Type _modExtensionsManagerType;
    private static object _managerInstance;
    private static bool _isInitialized = false;

    /// <summary>
    /// 初始化 ModExtensions API
    /// </summary>
    /// <returns>是否成功初始化</returns>
    public static bool Init()
    {
        if (_isInitialized) return true;

        try
        {
            Debug.Log("[ModExtensionsAPI] 开始初始化...");

            // 1. 查找类型（带上程序集名）
            _modExtensionsManagerType = Type.GetType(MANAGER_TYPE);

            if (_modExtensionsManagerType == null)
            {
                Debug.LogWarning("[ModExtensionsAPI] 未找到 ModExtensionsManager 类型");
                return false;
            }

            Debug.Log($"[ModExtensionsAPI] 找到类型: {_modExtensionsManagerType.FullName}");

            // 2. 获取 Instance 属性
            var instanceProperty = _modExtensionsManagerType.GetProperty("Instance",
                BindingFlags.Public | BindingFlags.Static);

            if (instanceProperty == null)
            {
                Debug.LogWarning("[ModExtensionsAPI] 未找到 Instance 属性");
                return false;
            }

            // 3. 获取单例实例
            _managerInstance = instanceProperty.GetValue(null);

            if (_managerInstance == null)
            {
                Debug.LogWarning("[ModExtensionsAPI] Instance 为 null");
                return false;
            }

            _isInitialized = true;
            Debug.Log("[ModExtensionsAPI] ✅ 初始化成功");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[ModExtensionsAPI] 初始化失败: {ex.Message}");
            Debug.LogWarning($"[ModExtensionsAPI] 堆栈: {ex.StackTrace}");
            return false;
        }
    }

    /// <summary>
    /// 注册 Mod 前缀
    /// </summary>
    public static bool RegisterMod(string prefix)
    {
        if (!_isInitialized && !Init()) return false;

        try
        {
            Debug.Log($"[ModExtensionsAPI] 注册 Mod: {prefix}");
            var method = _modExtensionsManagerType.GetMethod("RegisterMod",
                new Type[] { typeof(string) });

            method?.Invoke(_managerInstance, new object[] { prefix });
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[ModExtensionsAPI] RegisterMod 失败: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 标记 Mod 为已删除
    /// </summary>
    public static bool MarkModAsDeleted(string prefix)
    {
        if (!_isInitialized && !Init()) return false;

        try
        {
            Debug.Log($"[ModExtensionsAPI] 标记删除: {prefix}");
            var method = _modExtensionsManagerType.GetMethod("MarkModAsDeleted",
                new Type[] { typeof(string) });

            method?.Invoke(_managerInstance, new object[] { prefix });
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[ModExtensionsAPI] MarkModAsDeleted 失败: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 刷新物品缓存
    /// </summary>
    public static bool RefreshItemCache(Item item, bool refreshUI = true)
    {
        if (!_isInitialized && !Init()) return false;

        try
        {
            Debug.Log($"[ModExtensionsAPI] 刷新物品: {item?.DisplayName}");
            var method = _modExtensionsManagerType.GetMethod("RefreshItemCache",
                new Type[] { typeof(Item), typeof(bool) });

            method?.Invoke(_managerInstance, new object[] { item, refreshUI });
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[ModExtensionsAPI] RefreshItemCache 失败: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 检查是否可用
    /// </summary>
    public static bool IsAvailable()
    {
        return _isInitialized || Init();
    }

    /// <summary>
    /// 简单测试方法
    /// </summary>
    public static void Test()
    {
        Debug.Log("[ModExtensionsAPI] 开始测试...");

        if (Init())
        {
            Debug.Log("[ModExtensionsAPI] ✅ 测试通过");
        }
        else
        {
            Debug.Log("[ModExtensionsAPI] ❌ 测试失败");
        }
    }
}
