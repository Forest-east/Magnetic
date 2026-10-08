using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 把仍然停留在旧序列化格式（version 17 / 2019.1 之前）的资源重新保存为当前格式。
/// 这能消除 Unity 启动时大量
///   "Serialized files [version 17] before 2019.1 are deprecated. Open and re-save the file: ..."
/// 警告。
///
/// 覆盖范围：
///   - Assets 下全部资源（场景 .unity / 预制体 / 材质 / 动画 / 控制器 ...）
///   - ProjectSettings 下的配置文件（InputManager / TagManager / GraphicsSettings ...）
///
/// 菜单：Tools/Resave All Assets (Unity 6 format)
/// 命令行：-executeMethod ResaveOldAssets.Resave
/// </summary>
public static class ResaveOldAssets
{
    static readonly string[] SettingsFiles =
    {
        "ProjectSettings/InputManager.asset",
        "ProjectSettings/TagManager.asset",
        "ProjectSettings/AudioManager.asset",
        "ProjectSettings/TimeManager.asset",
        "ProjectSettings/DynamicsManager.asset",
        "ProjectSettings/QualitySettings.asset",
        "ProjectSettings/EditorSettings.asset",
        "ProjectSettings/NavMeshAreas.asset",
        "ProjectSettings/Physics2DSettings.asset",
        "ProjectSettings/GraphicsSettings.asset",
        "ProjectSettings/UnityConnectSettings.asset",
        "ProjectSettings/ClusterInputManager.asset",
        "ProjectSettings/NetworkManager.asset",
    };

    [MenuItem("Tools/Resave All Assets (Unity 6 format)")]
    public static void Resave()
    {
        AssetDatabase.SaveAssets();

        var assetPaths = AssetDatabase.GetAllAssetPaths()
            .Where(p => p.StartsWith("Assets/") && File.Exists(p))
            .ToArray();
        Debug.Log("[Resave] Assets to process: " + assetPaths.Length);
        AssetDatabase.ForceReserializeAssets(assetPaths, ForceReserializeAssetsOptions.ReserializeAssets);

        var settings = SettingsFiles.Where(File.Exists).ToArray();
        Debug.Log("[Resave] ProjectSettings to process: " + settings.Length);
        if (settings.Length > 0)
            AssetDatabase.ForceReserializeAssets(settings, ForceReserializeAssetsOptions.ReserializeAssets);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[Resave] DONE. Assets=" + assetPaths.Length + " Settings=" + settings.Length);

        if (Application.isBatchMode) EditorApplication.Exit(0);
    }
}
